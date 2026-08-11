import { useMutation, useQuery } from "@tanstack/react-query";
import { X } from "lucide-react";
import { useState } from "react";
import { useLookupOptions } from "../../../hooks/useLookupOptions";
import { apiClient } from "../../../services/apiClient";

interface QuestionDto {
  id: string;
  controlId: string;
  text: string;
  weight: number;
  order: number;
  isRequired: boolean;
  isActive: boolean;
}

interface EvaluationDto {
  id: string;
  auditId: string;
  controlId: string;
  controlCode: string;
  score?: number | null;
  status: string | number;
  notes?: string | null;
  evaluatedAtUtc?: string | null;
}

interface ControlLookup {
  id: string;
  code: string;
  title: string;
}

interface AuditLookup {
  id: string;
  code: string;
  title: string;
}

interface UserLookup {
  id: string;
  fullName: string;
  email: string;
}

const complianceOptions = [
  { value: "1", label: "No evaluado" },
  { value: "2", label: "No cumple" },
  { value: "3", label: "Cumple parcialmente" },
  { value: "4", label: "Cumple" },
  { value: "5", label: "No aplica" },
];

function getErrorMessage(error: unknown, fallback: string) {
  if (typeof error !== "object" || error === null) return fallback;
  const response = (error as { response?: { data?: { detail?: string; title?: string } | string } }).response;
  if (typeof response?.data === "string" && response.data.trim()) return response.data;
  if (response?.data && typeof response.data === "object") return response.data.detail || response.data.title || fallback;
  return error instanceof Error && error.message ? error.message : fallback;
}

function formatStatus(value: string | number) {
  const numericValue = Number(value);
  return complianceOptions.find((option) => Number(option.value) === numericValue)?.label ?? String(value);
}

export function AssessmentsPage() {
  const [controlId, setControlId] = useState("");
  const [auditId, setAuditId] = useState("");
  const [selectedEvaluation, setSelectedEvaluation] = useState<EvaluationDto | null>(null);
  const [score, setScore] = useState("");
  const [status, setStatus] = useState("1");
  const [evaluatedByUserId, setEvaluatedByUserId] = useState("");
  const [notes, setNotes] = useState("");
  const [error, setError] = useState<string | null>(null);

  const controls = useLookupOptions<ControlLookup>(
    "framework-controls",
    "/frameworks/controls",
    (item) => `${item.code} — ${item.title}`,
  );

  const audits = useLookupOptions<AuditLookup>(
    "audits",
    "/audits",
    (item) => `${item.code} — ${item.title}`,
  );

  const users = useLookupOptions<UserLookup>(
    "users",
    "/users",
    (item) => `${item.fullName} — ${item.email}`,
  );

  const questions = useQuery({
    queryKey: ["questions", controlId],
    enabled: Boolean(controlId),
    queryFn: async () => (await apiClient.get<QuestionDto[]>(`/frameworks/controls/${controlId}/questions`)).data,
  });

  const evaluations = useQuery({
    queryKey: ["evaluations", auditId],
    enabled: Boolean(auditId),
    queryFn: async () => (await apiClient.get<EvaluationDto[]>(`/frameworks/evaluations/${auditId}`)).data,
  });

  const evaluate = useMutation({
    mutationFn: async () => {
      if (!selectedEvaluation) return;
      setError(null);
      await apiClient.put(`/frameworks/evaluations/${selectedEvaluation.auditId}/${selectedEvaluation.controlId}`, {
        score: score === "" ? null : Number(score),
        status: Number(status),
        notes: notes || null,
        evaluatedByUserId,
      });
    },
    onSuccess: async () => {
      setSelectedEvaluation(null);
      await evaluations.refetch();
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError, "No fue posible registrar la evaluación.")),
  });

  function openEvaluation(item: EvaluationDto) {
    setSelectedEvaluation(item);
    setScore(item.score === null || item.score === undefined ? "" : String(item.score));
    setStatus(String(item.status ?? 1));
    setEvaluatedByUserId("");
    setNotes(item.notes ?? "");
    setError(null);
  }

  return (
    <main className="module-page">
      <header className="module-header">
        <div>
          <p className="eyebrow">EJECUCIÓN</p>
          <h1>Evaluaciones</h1>
          <p>Consulta preguntas de control y registra la evaluación de cumplimiento por auditoría.</p>
        </div>
      </header>

      <section className="data-panel upload-panel">
        <h2>Preguntas por control</h2>
        <div className="inline-form assessment-filter">
          <label>
            Control
            <select value={controlId} onChange={(event) => setControlId(event.target.value)}>
              <option value="">{controls.isLoading ? "Cargando controles..." : "Selecciona un control"}</option>
              {controls.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
            </select>
          </label>
        </div>
        {controlId && (
          <div className="table-scroll">
            <table className="data-table">
              <thead><tr><th>Orden</th><th>Pregunta</th><th>Peso</th><th>Requerida</th><th>Activa</th></tr></thead>
              <tbody>{(questions.data ?? []).map((item) => (
                <tr key={item.id}><td>{item.order}</td><td>{item.text}</td><td>{item.weight}</td><td>{item.isRequired ? "Sí" : "No"}</td><td>{item.isActive ? "Sí" : "No"}</td></tr>
              ))}</tbody>
            </table>
          </div>
        )}
      </section>

      <section className="data-panel upload-panel">
        <h2>Evaluaciones por auditoría</h2>
        <div className="inline-form assessment-filter">
          <label>
            Auditoría
            <select value={auditId} onChange={(event) => setAuditId(event.target.value)}>
              <option value="">{audits.isLoading ? "Cargando auditorías..." : "Selecciona una auditoría"}</option>
              {audits.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
            </select>
          </label>
        </div>
        {auditId && (
          <div className="table-scroll">
            <table className="data-table">
              <thead><tr><th>Control</th><th>Puntuación</th><th>Estado</th><th>Notas</th><th>Evaluado</th><th>Acciones</th></tr></thead>
              <tbody>{(evaluations.data ?? []).map((item) => (
                <tr key={item.id}>
                  <td>{item.controlCode}</td>
                  <td>{item.score ?? "—"}</td>
                  <td>{formatStatus(item.status)}</td>
                  <td>{item.notes ?? "—"}</td>
                  <td>{item.evaluatedAtUtc ? new Date(item.evaluatedAtUtc).toLocaleString() : "—"}</td>
                  <td><button className="text-action" type="button" onClick={() => openEvaluation(item)}>Evaluar</button></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
        )}
      </section>

      {selectedEvaluation && (
        <div className="modal-backdrop" role="presentation">
          <section className="modal-card" role="dialog" aria-modal="true" aria-label="Registrar evaluación">
            <header>
              <div>
                <p className="eyebrow">EVALUACIÓN</p>
                <h2>{selectedEvaluation.controlCode}</h2>
              </div>
              <button type="button" className="icon-button" onClick={() => setSelectedEvaluation(null)}><X size={18} /></button>
            </header>
            <form className="resource-form" onSubmit={(event) => { event.preventDefault(); evaluate.mutate(); }}>
              <label>
                Puntuación (0-100)
                <input type="number" min={0} max={100} value={score} onChange={(event) => setScore(event.target.value)} />
              </label>
              <label>
                Estado de cumplimiento
                <select value={status} onChange={(event) => setStatus(event.target.value)} required>
                  {complianceOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
                </select>
              </label>
              <label>
                Usuario evaluador
                <select value={evaluatedByUserId} onChange={(event) => setEvaluatedByUserId(event.target.value)} required>
                  <option value="">{users.isLoading ? "Cargando usuarios..." : "Selecciona un evaluador"}</option>
                  {users.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
                </select>
              </label>
              <label>
                Notas
                <textarea value={notes} onChange={(event) => setNotes(event.target.value)} />
              </label>
              {error && <div className="form-error">{error}</div>}
              <div className="form-actions">
                <button type="button" className="secondary-button" onClick={() => setSelectedEvaluation(null)}>Cancelar</button>
                <button type="submit" disabled={evaluate.isPending}>{evaluate.isPending ? "Guardando..." : "Guardar evaluación"}</button>
              </div>
            </form>
          </section>
        </div>
      )}
    </main>
  );
}
