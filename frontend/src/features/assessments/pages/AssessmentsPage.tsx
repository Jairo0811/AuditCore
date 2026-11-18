import { useMutation, useQuery } from "@tanstack/react-query";
import { Pencil, Plus } from "lucide-react";
import { useState, type FormEvent } from "react";
import { FeedbackMessage, type Feedback } from "../../../components/ui/FeedbackMessage";
import { Modal } from "../../../components/ui/Modal";
import { hasPermission, Permissions } from "../../auth/permissions";
import { getSession } from "../../auth/session";
import { useLookupOptions, useUserLookup } from "../../../hooks/useLookupOptions";
import { getApiErrorMessage } from "../../../lib/apiErrors";
import { formatDateTime } from "../../../lib/formatters";
import { apiClient } from "../../../services/apiClient";

interface QuestionDto { id: string; controlId: string; text: string; weight: number; order: number; isRequired: boolean; isActive: boolean }
interface EvaluationDto { id: string; auditId: string; controlId: string; controlCode: string; score?: number | null; status: string | number; notes?: string | null; evaluatedByUserId?: string | null; evaluatedAtUtc?: string | null }
interface ControlLookup { id: string; code: string; title: string }
interface AuditLookup { id: string; code: string; title: string }

type QuestionForm = { text: string; weight: string; order: string; isRequired: boolean; isActive: boolean };
type EvaluationForm = { controlId: string; score: string; status: string; evaluatedByUserId: string; notes: string };

const emptyQuestion: QuestionForm = { text: "", weight: "1", order: "1", isRequired: true, isActive: true };

const complianceOptions = [
  { value: "1", label: "No evaluado" },
  { value: "2", label: "No cumple" },
  { value: "3", label: "Cumple parcialmente" },
  { value: "4", label: "Cumple" },
  { value: "5", label: "No aplica" },
];

function formatStatus(value: string | number) {
  return complianceOptions.find((option) => Number(option.value) === Number(value))?.label ?? String(value);
}

export function AssessmentsPage() {
  const canManageQuestions = hasPermission(Permissions.FrameworksManage);
  const canViewEvaluations = hasPermission(Permissions.AuditsView);
  const canEvaluate = hasPermission(Permissions.AuditsExecute);

  const [controlId, setControlId] = useState("");
  const [auditId, setAuditId] = useState("");
  const [questionEditor, setQuestionEditor] = useState<QuestionDto | "new" | null>(null);
  const [questionForm, setQuestionForm] = useState<QuestionForm>(emptyQuestion);
  const [evaluationEditor, setEvaluationEditor] = useState<EvaluationDto | "new" | null>(null);
  const [evaluationForm, setEvaluationForm] = useState<EvaluationForm>({ controlId: "", score: "", status: "1", evaluatedByUserId: "", notes: "" });
  const [formError, setFormError] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<Feedback | null>(null);

  const controls = useLookupOptions<ControlLookup>("framework-controls", "/frameworks/controls", (item) => `${item.code} — ${item.title}`, [Permissions.FrameworksView]);
  const audits = useLookupOptions<AuditLookup>("audits", "/audits", (item) => `${item.code} — ${item.title}`, [Permissions.AuditsView]);
  const users = useUserLookup();

  const questions = useQuery({
    queryKey: ["questions", controlId],
    enabled: Boolean(controlId),
    queryFn: async () => (await apiClient.get<QuestionDto[]>(`/frameworks/controls/${controlId}/questions`)).data,
  });

  const evaluations = useQuery({
    queryKey: ["evaluations", auditId],
    enabled: Boolean(auditId) && canViewEvaluations,
    queryFn: async () => (await apiClient.get<EvaluationDto[]>(`/frameworks/evaluations/${auditId}`)).data,
  });

  const saveQuestion = useMutation({
    mutationFn: async () => {
      if (!controlId || !questionEditor) return;
      const common = {
        text: questionForm.text.trim(),
        weight: Number(questionForm.weight),
        order: Number(questionForm.order),
        isRequired: questionForm.isRequired,
      };
      if (questionEditor === "new") await apiClient.post("/frameworks/questions", { controlId, ...common });
      else await apiClient.put(`/frameworks/questions/${questionEditor.id}`, { ...common, isActive: questionForm.isActive });
    },
    onSuccess: async () => {
      setFeedback({ tone: "success", message: questionEditor === "new" ? "Pregunta creada correctamente." : "Pregunta actualizada correctamente." });
      setQuestionEditor(null);
      await questions.refetch();
    },
    onError: (error) => setFormError(getApiErrorMessage(error, "No fue posible guardar la pregunta.")),
  });

  const saveEvaluation = useMutation({
    mutationFn: async () => {
      if (!auditId || !evaluationEditor) return;
      const targetControlId = evaluationEditor === "new" ? evaluationForm.controlId : evaluationEditor.controlId;
      if (!targetControlId) throw new Error("Selecciona un control.");
      await apiClient.put(`/frameworks/evaluations/${auditId}/${targetControlId}`, {
        score: evaluationForm.score === "" ? null : Number(evaluationForm.score),
        status: Number(evaluationForm.status),
        notes: evaluationForm.notes || null,
        evaluatedByUserId: evaluationForm.evaluatedByUserId,
      });
    },
    onSuccess: async () => {
      setFeedback({ tone: "success", message: "Evaluación registrada correctamente." });
      setEvaluationEditor(null);
      await evaluations.refetch();
    },
    onError: (error) => setFormError(getApiErrorMessage(error, "No fue posible registrar la evaluación.")),
  });

  // Evaluador sugerido: el usuario autenticado, si figura en el catálogo seleccionable.
  function defaultEvaluator(current?: string | null) {
    if (current && users.records.some((user) => user.id === current)) return current;
    const self = getSession()?.userId;
    return self && users.records.some((user) => user.id === self) ? self : "";
  }

  function openQuestion(item: QuestionDto | "new") {
    setQuestionForm(item === "new"
      ? { ...emptyQuestion, order: String((questions.data?.length ?? 0) + 1) }
      : { text: item.text, weight: String(item.weight), order: String(item.order), isRequired: item.isRequired, isActive: item.isActive });
    setFormError(null);
    setFeedback(null);
    setQuestionEditor(item);
  }

  function openEvaluation(item: EvaluationDto | "new") {
    setEvaluationForm(item === "new"
      ? { controlId, score: "", status: "1", evaluatedByUserId: defaultEvaluator(), notes: "" }
      : {
          controlId: item.controlId,
          score: item.score == null ? "" : String(item.score),
          status: String(item.status ?? 1),
          evaluatedByUserId: defaultEvaluator(item.evaluatedByUserId),
          notes: item.notes ?? "",
        });
    setFormError(null);
    setFeedback(null);
    setEvaluationEditor(item);
  }

  function submit(event: FormEvent, action: () => void) {
    event.preventDefault();
    setFormError(null);
    action();
  }

  return (
    <div className="module-page">
      <header className="module-header">
        <div>
          <p className="eyebrow">EJECUCIÓN</p>
          <h1>Evaluaciones</h1>
          <p>Administra preguntas por control y registra evaluaciones de cumplimiento por auditoría.</p>
        </div>
      </header>

      <FeedbackMessage feedback={feedback} onDismiss={() => setFeedback(null)} />

      <section className="data-panel upload-panel" aria-labelledby="questions-title">
        <div className="section-heading">
          <div>
            <h2 id="questions-title">Preguntas por control</h2>
            <p>Define el cuestionario que utilizará cada control durante la evaluación.</p>
          </div>
          {controlId && canManageQuestions && (
            <button type="button" onClick={() => openQuestion("new")}><Plus size={16} aria-hidden="true" /> Nueva pregunta</button>
          )}
        </div>
        <div className="inline-form assessment-filter">
          <label>
            Control
            <select value={controlId} onChange={(event) => setControlId(event.target.value)}>
              <option value="">{controls.isLoading ? "Cargando controles..." : "Selecciona un control"}</option>
              {controls.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
            </select>
          </label>
        </div>
        {controls.isError && <p className="panel-state error-message" role="alert">{getApiErrorMessage(controls.error, "No fue posible cargar los controles.")}</p>}
        {controlId && questions.isPending && <p className="panel-state" role="status">Cargando preguntas...</p>}
        {controlId && questions.isError && (
          <div className="panel-state error-message" role="alert">
            <p>{getApiErrorMessage(questions.error, "No fue posible cargar las preguntas.")}</p>
            <button type="button" className="secondary-button" onClick={() => questions.refetch()}>Reintentar</button>
          </div>
        )}
        {controlId && questions.isSuccess && (
          <div className="table-scroll">
            <table className="data-table">
              <caption className="sr-only">Preguntas del control seleccionado</caption>
              <thead>
                <tr>
                  <th scope="col">Orden</th><th scope="col">Pregunta</th><th scope="col">Peso</th>
                  <th scope="col">Requerida</th><th scope="col">Activa</th>
                  {canManageQuestions && <th scope="col">Acciones</th>}
                </tr>
              </thead>
              <tbody>
                {questions.data.map((item) => (
                  <tr key={item.id}>
                    <td>{item.order}</td><td>{item.text}</td><td>{item.weight}</td>
                    <td>{item.isRequired ? "Sí" : "No"}</td><td>{item.isActive ? "Sí" : "No"}</td>
                    {canManageQuestions && (
                      <td>
                        <button className="icon-button" type="button" aria-label={`Editar pregunta ${item.order}`} title="Editar pregunta" onClick={() => openQuestion(item)}>
                          <Pencil size={15} aria-hidden="true" />
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
                {questions.data.length === 0 && (
                  <tr><td colSpan={canManageQuestions ? 6 : 5} className="empty-cell">Este control todavía no tiene preguntas.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {canViewEvaluations && (
        <section className="data-panel upload-panel" aria-labelledby="evaluations-title">
          <div className="section-heading">
            <div>
              <h2 id="evaluations-title">Evaluaciones por auditoría</h2>
              <p>Registra y actualiza el nivel de cumplimiento de los controles auditados.</p>
            </div>
            {auditId && canEvaluate && (
              <button type="button" onClick={() => openEvaluation("new")}><Plus size={16} aria-hidden="true" /> Nueva evaluación</button>
            )}
          </div>
          <div className="inline-form assessment-filter">
            <label>
              Auditoría
              <select value={auditId} onChange={(event) => setAuditId(event.target.value)}>
                <option value="">{audits.isLoading ? "Cargando auditorías..." : "Selecciona una auditoría"}</option>
                {audits.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
              </select>
            </label>
          </div>
          {auditId && evaluations.isPending && <p className="panel-state" role="status">Cargando evaluaciones...</p>}
          {auditId && evaluations.isError && (
            <div className="panel-state error-message" role="alert">
              <p>{getApiErrorMessage(evaluations.error, "No fue posible cargar las evaluaciones.")}</p>
              <button type="button" className="secondary-button" onClick={() => evaluations.refetch()}>Reintentar</button>
            </div>
          )}
          {auditId && evaluations.isSuccess && (
            <div className="table-scroll">
              <table className="data-table">
                <caption className="sr-only">Evaluaciones de la auditoría seleccionada</caption>
                <thead>
                  <tr>
                    <th scope="col">Control</th><th scope="col">Puntuación</th><th scope="col">Estado</th>
                    <th scope="col">Notas</th><th scope="col">Evaluado</th>
                    {canEvaluate && <th scope="col">Acciones</th>}
                  </tr>
                </thead>
                <tbody>
                  {evaluations.data.map((item) => (
                    <tr key={item.id}>
                      <td>{item.controlCode}</td><td>{item.score ?? "—"}</td><td>{formatStatus(item.status)}</td>
                      <td>{item.notes ?? "—"}</td><td>{formatDateTime(item.evaluatedAtUtc)}</td>
                      {canEvaluate && (
                        <td>
                          <button className="text-action" type="button" aria-label={`Evaluar ${item.controlCode}`} onClick={() => openEvaluation(item)}>Evaluar</button>
                        </td>
                      )}
                    </tr>
                  ))}
                  {evaluations.data.length === 0 && (
                    <tr><td colSpan={canEvaluate ? 6 : 5} className="empty-cell">No hay controles evaluados en esta auditoría.</td></tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </section>
      )}

      {questionEditor && (
        <Modal
          eyebrow={questionEditor === "new" ? "NUEVA PREGUNTA" : "EDICIÓN"}
          title="Pregunta de control"
          onClose={() => setQuestionEditor(null)}
        >
          <form className="resource-form" onSubmit={(event) => submit(event, () => saveQuestion.mutate())}>
            <label className="full-width">Pregunta
              <textarea required maxLength={1000} value={questionForm.text} onChange={(event) => setQuestionForm({ ...questionForm, text: event.target.value })} />
            </label>
            <label>Peso
              <input type="number" min="0.01" step="0.01" required value={questionForm.weight} onChange={(event) => setQuestionForm({ ...questionForm, weight: event.target.value })} />
            </label>
            <label>Orden
              <input type="number" min="1" required value={questionForm.order} onChange={(event) => setQuestionForm({ ...questionForm, order: event.target.value })} />
            </label>
            <label className="checkbox-field">
              <input type="checkbox" checked={questionForm.isRequired} onChange={(event) => setQuestionForm({ ...questionForm, isRequired: event.target.checked })} /> Pregunta requerida
            </label>
            {questionEditor !== "new" && (
              <label className="checkbox-field">
                <input type="checkbox" checked={questionForm.isActive} onChange={(event) => setQuestionForm({ ...questionForm, isActive: event.target.checked })} /> Pregunta activa
              </label>
            )}
            {formError && <div className="form-error" role="alert">{formError}</div>}
            <div className="form-actions">
              <button type="button" className="secondary-button" onClick={() => setQuestionEditor(null)}>Cancelar</button>
              <button type="submit" disabled={saveQuestion.isPending}>{saveQuestion.isPending ? "Guardando..." : "Guardar"}</button>
            </div>
          </form>
        </Modal>
      )}

      {evaluationEditor && (
        <Modal
          eyebrow="EVALUACIÓN"
          title={evaluationEditor === "new" ? "Nueva evaluación" : `Evaluar ${evaluationEditor.controlCode}`}
          onClose={() => setEvaluationEditor(null)}
        >
          <form className="resource-form" onSubmit={(event) => submit(event, () => saveEvaluation.mutate())}>
            {evaluationEditor === "new" && (
              <label className="full-width">Control
                <select required value={evaluationForm.controlId} onChange={(event) => setEvaluationForm({ ...evaluationForm, controlId: event.target.value })}>
                  <option value="">Selecciona un control</option>
                  {controls.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
                </select>
              </label>
            )}
            <label>Puntuación (0-100)
              <input type="number" min={0} max={100} value={evaluationForm.score} onChange={(event) => setEvaluationForm({ ...evaluationForm, score: event.target.value })} />
            </label>
            <label>Estado de cumplimiento
              <select required value={evaluationForm.status} onChange={(event) => setEvaluationForm({ ...evaluationForm, status: event.target.value })}>
                {complianceOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
              </select>
            </label>
            <label className="full-width">Usuario evaluador
              <select required value={evaluationForm.evaluatedByUserId} onChange={(event) => setEvaluationForm({ ...evaluationForm, evaluatedByUserId: event.target.value })}>
                <option value="">{users.isLoading ? "Cargando usuarios..." : "Selecciona un evaluador"}</option>
                {users.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
              </select>
            </label>
            <label className="full-width">Notas
              <textarea maxLength={3000} value={evaluationForm.notes} onChange={(event) => setEvaluationForm({ ...evaluationForm, notes: event.target.value })} />
            </label>
            {formError && <div className="form-error" role="alert">{formError}</div>}
            <div className="form-actions">
              <button type="button" className="secondary-button" onClick={() => setEvaluationEditor(null)}>Cancelar</button>
              <button type="submit" disabled={saveEvaluation.isPending}>{saveEvaluation.isPending ? "Guardando..." : "Guardar evaluación"}</button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}
