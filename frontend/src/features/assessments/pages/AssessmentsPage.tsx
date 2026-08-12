import { useMutation, useQuery } from "@tanstack/react-query";
import { Pencil, Plus, X } from "lucide-react";
import { useState } from "react";
import { useLookupOptions } from "../../../hooks/useLookupOptions";
import { apiClient } from "../../../services/apiClient";

interface QuestionDto { id: string; controlId: string; text: string; weight: number; order: number; isRequired: boolean; isActive: boolean; }
interface EvaluationDto { id: string; auditId: string; controlId: string; controlCode: string; score?: number | null; status: string | number; notes?: string | null; evaluatedAtUtc?: string | null; }
interface ControlLookup { id: string; code: string; title: string; }
interface AuditLookup { id: string; code: string; title: string; }
interface UserLookup { id: string; fullName: string; email: string; }

type QuestionForm = { text: string; weight: string; order: string; isRequired: boolean; isActive: boolean };
const emptyQuestion: QuestionForm = { text: "", weight: "1", order: "1", isRequired: true, isActive: true };

const complianceOptions = [
  { value: "1", label: "No evaluado" }, { value: "2", label: "No cumple" },
  { value: "3", label: "Cumple parcialmente" }, { value: "4", label: "Cumple" }, { value: "5", label: "No aplica" },
];

function getErrorMessage(error: unknown, fallback: string) {
  if (typeof error !== "object" || error === null) return fallback;
  const response = (error as { response?: { data?: { detail?: string; title?: string } | string } }).response;
  if (typeof response?.data === "string" && response.data.trim()) return response.data;
  if (response?.data && typeof response.data === "object") return response.data.detail || response.data.title || fallback;
  return error instanceof Error && error.message ? error.message : fallback;
}
function formatStatus(value: string | number) { return complianceOptions.find((x) => Number(x.value) === Number(value))?.label ?? String(value); }

export function AssessmentsPage() {
  const [controlId, setControlId] = useState("");
  const [auditId, setAuditId] = useState("");
  const [questionEditor, setQuestionEditor] = useState<QuestionDto | "new" | null>(null);
  const [questionForm, setQuestionForm] = useState<QuestionForm>(emptyQuestion);
  const [selectedEvaluation, setSelectedEvaluation] = useState<EvaluationDto | "new" | null>(null);
  const [evaluationControlId, setEvaluationControlId] = useState("");
  const [score, setScore] = useState("");
  const [status, setStatus] = useState("1");
  const [evaluatedByUserId, setEvaluatedByUserId] = useState("");
  const [notes, setNotes] = useState("");
  const [error, setError] = useState<string | null>(null);

  const controls = useLookupOptions<ControlLookup>("framework-controls", "/frameworks/controls", (x) => `${x.code} — ${x.title}`);
  const audits = useLookupOptions<AuditLookup>("audits", "/audits", (x) => `${x.code} — ${x.title}`);
  const users = useLookupOptions<UserLookup>("users", "/users", (x) => `${x.fullName} — ${x.email}`);

  const questions = useQuery({ queryKey: ["questions", controlId], enabled: Boolean(controlId), queryFn: async () => (await apiClient.get<QuestionDto[]>(`/frameworks/controls/${controlId}/questions`)).data });
  const evaluations = useQuery({ queryKey: ["evaluations", auditId], enabled: Boolean(auditId), queryFn: async () => (await apiClient.get<EvaluationDto[]>(`/frameworks/evaluations/${auditId}`)).data });

  const saveQuestion = useMutation({
    mutationFn: async () => {
      if (!controlId || !questionEditor) return;
      const common = { text: questionForm.text.trim(), weight: Number(questionForm.weight), order: Number(questionForm.order), isRequired: questionForm.isRequired };
      if (questionEditor === "new") await apiClient.post("/frameworks/questions", { controlId, ...common });
      else await apiClient.put(`/frameworks/questions/${questionEditor.id}`, { ...common, isActive: questionForm.isActive });
    },
    onSuccess: async () => { setQuestionEditor(null); setError(null); await questions.refetch(); },
    onError: (e) => setError(getErrorMessage(e, "No fue posible guardar la pregunta.")),
  });

  const evaluate = useMutation({
    mutationFn: async () => {
      if (!auditId || !selectedEvaluation) return;
      const targetControlId = selectedEvaluation === "new" ? evaluationControlId : selectedEvaluation.controlId;
      if (!targetControlId) throw new Error("Selecciona un control.");
      await apiClient.put(`/frameworks/evaluations/${auditId}/${targetControlId}`, { score: score === "" ? null : Number(score), status: Number(status), notes: notes || null, evaluatedByUserId });
    },
    onSuccess: async () => { setSelectedEvaluation(null); setError(null); await evaluations.refetch(); },
    onError: (e) => setError(getErrorMessage(e, "No fue posible registrar la evaluación.")),
  });

  function newQuestion() { setQuestionForm({ ...emptyQuestion, order: String((questions.data?.length ?? 0) + 1) }); setQuestionEditor("new"); setError(null); }
  function editQuestion(item: QuestionDto) { setQuestionForm({ text: item.text, weight: String(item.weight), order: String(item.order), isRequired: item.isRequired, isActive: item.isActive }); setQuestionEditor(item); setError(null); }
  function newEvaluation() { setEvaluationControlId(controlId); setScore(""); setStatus("1"); setEvaluatedByUserId(""); setNotes(""); setSelectedEvaluation("new"); setError(null); }
  function openEvaluation(item: EvaluationDto) { setScore(item.score == null ? "" : String(item.score)); setStatus(String(item.status ?? 1)); setEvaluatedByUserId(""); setNotes(item.notes ?? ""); setSelectedEvaluation(item); setError(null); }

  return <main className="module-page">
    <header className="module-header"><div><p className="eyebrow">EJECUCIÓN</p><h1>Evaluaciones</h1><p>Administra preguntas por control y registra evaluaciones de cumplimiento por auditoría.</p></div></header>

    <section className="data-panel upload-panel">
      <div className="section-heading"><div><h2>Preguntas por control</h2><p>Define el cuestionario que utilizará cada control durante la evaluación.</p></div>{controlId && <button type="button" onClick={newQuestion}><Plus size={16}/> Nueva pregunta</button>}</div>
      <div className="inline-form assessment-filter"><label>Control<select value={controlId} onChange={(e) => setControlId(e.target.value)}><option value="">{controls.isLoading ? "Cargando controles..." : "Selecciona un control"}</option>{controls.options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}</select></label></div>
      {controlId && <div className="table-scroll"><table className="data-table"><thead><tr><th>Orden</th><th>Pregunta</th><th>Peso</th><th>Requerida</th><th>Activa</th><th>Acciones</th></tr></thead><tbody>
        {(questions.data ?? []).map((item) => <tr key={item.id}><td>{item.order}</td><td>{item.text}</td><td>{item.weight}</td><td>{item.isRequired ? "Sí" : "No"}</td><td>{item.isActive ? "Sí" : "No"}</td><td><button className="icon-button" type="button" title="Editar pregunta" onClick={() => editQuestion(item)}><Pencil size={15}/></button></td></tr>)}
        {!questions.isLoading && (questions.data?.length ?? 0) === 0 && <tr><td colSpan={6} className="empty-cell">Este control todavía no tiene preguntas. Crea la primera para iniciar su evaluación.</td></tr>}
      </tbody></table></div>}
    </section>

    <section className="data-panel upload-panel">
      <div className="section-heading"><div><h2>Evaluaciones por auditoría</h2><p>Registra y actualiza el nivel de cumplimiento de los controles auditados.</p></div>{auditId && <button type="button" onClick={newEvaluation}><Plus size={16}/> Nueva evaluación</button>}</div>
      <div className="inline-form assessment-filter"><label>Auditoría<select value={auditId} onChange={(e) => setAuditId(e.target.value)}><option value="">{audits.isLoading ? "Cargando auditorías..." : "Selecciona una auditoría"}</option>{audits.options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}</select></label></div>
      {auditId && <div className="table-scroll"><table className="data-table"><thead><tr><th>Control</th><th>Puntuación</th><th>Estado</th><th>Notas</th><th>Evaluado</th><th>Acciones</th></tr></thead><tbody>
        {(evaluations.data ?? []).map((item) => <tr key={item.id}><td>{item.controlCode}</td><td>{item.score ?? "—"}</td><td>{formatStatus(item.status)}</td><td>{item.notes ?? "—"}</td><td>{item.evaluatedAtUtc ? new Date(item.evaluatedAtUtc).toLocaleString() : "—"}</td><td><button className="text-action" type="button" onClick={() => openEvaluation(item)}>Evaluar</button></td></tr>)}
        {!evaluations.isLoading && (evaluations.data?.length ?? 0) === 0 && <tr><td colSpan={6} className="empty-cell">No hay controles evaluados en esta auditoría. Usa “Nueva evaluación” para comenzar.</td></tr>}
      </tbody></table></div>}
    </section>

    {questionEditor && <div className="modal-backdrop"><section className="modal-card" role="dialog" aria-modal="true"><header><div><p className="eyebrow">{questionEditor === "new" ? "NUEVA PREGUNTA" : "EDICIÓN"}</p><h2>Pregunta de control</h2></div><button type="button" className="icon-button" onClick={() => setQuestionEditor(null)}><X size={18}/></button></header><form className="resource-form" onSubmit={(e) => { e.preventDefault(); saveQuestion.mutate(); }}>
      <label className="full-width">Pregunta<textarea required value={questionForm.text} onChange={(e) => setQuestionForm({ ...questionForm, text: e.target.value })}/></label>
      <label>Peso<input type="number" min="0.01" step="0.01" required value={questionForm.weight} onChange={(e) => setQuestionForm({ ...questionForm, weight: e.target.value })}/></label>
      <label>Orden<input type="number" min="1" required value={questionForm.order} onChange={(e) => setQuestionForm({ ...questionForm, order: e.target.value })}/></label>
      <label className="checkbox-field"><input type="checkbox" checked={questionForm.isRequired} onChange={(e) => setQuestionForm({ ...questionForm, isRequired: e.target.checked })}/> Pregunta requerida</label>
      {questionEditor !== "new" && <label className="checkbox-field"><input type="checkbox" checked={questionForm.isActive} onChange={(e) => setQuestionForm({ ...questionForm, isActive: e.target.checked })}/> Pregunta activa</label>}
      {error && <div className="form-error full-width">{error}</div>}<div className="form-actions full-width"><button type="button" className="secondary-button" onClick={() => setQuestionEditor(null)}>Cancelar</button><button type="submit" disabled={saveQuestion.isPending}>{saveQuestion.isPending ? "Guardando..." : "Guardar"}</button></div>
    </form></section></div>}

    {selectedEvaluation && <div className="modal-backdrop"><section className="modal-card" role="dialog" aria-modal="true"><header><div><p className="eyebrow">EVALUACIÓN</p><h2>{selectedEvaluation === "new" ? "Nueva evaluación" : selectedEvaluation.controlCode}</h2></div><button type="button" className="icon-button" onClick={() => setSelectedEvaluation(null)}><X size={18}/></button></header><form className="resource-form" onSubmit={(e) => { e.preventDefault(); evaluate.mutate(); }}>
      {selectedEvaluation === "new" && <label className="full-width">Control<select required value={evaluationControlId} onChange={(e) => setEvaluationControlId(e.target.value)}><option value="">Selecciona un control</option>{controls.options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}</select></label>}
      <label>Puntuación (0-100)<input type="number" min={0} max={100} value={score} onChange={(e) => setScore(e.target.value)}/></label>
      <label>Estado de cumplimiento<select value={status} onChange={(e) => setStatus(e.target.value)} required>{complianceOptions.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}</select></label>
      <label className="full-width">Usuario evaluador<select value={evaluatedByUserId} onChange={(e) => setEvaluatedByUserId(e.target.value)} required><option value="">{users.isLoading ? "Cargando usuarios..." : "Selecciona un evaluador"}</option>{users.options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}</select></label>
      <label className="full-width">Notas<textarea value={notes} onChange={(e) => setNotes(e.target.value)}/></label>
      {error && <div className="form-error full-width">{error}</div>}<div className="form-actions full-width"><button type="button" className="secondary-button" onClick={() => setSelectedEvaluation(null)}>Cancelar</button><button type="submit" disabled={evaluate.isPending}>{evaluate.isPending ? "Guardando..." : "Guardar evaluación"}</button></div>
    </form></section></div>}
  </main>;
}
