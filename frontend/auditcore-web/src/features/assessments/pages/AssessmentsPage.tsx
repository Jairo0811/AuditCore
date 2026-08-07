import { useMutation, useQuery } from "@tanstack/react-query";
import { useState } from "react";
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

export function AssessmentsPage() {
  const [controlId, setControlId] = useState("");
  const [auditId, setAuditId] = useState("");

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
    mutationFn: async (item: EvaluationDto) => {
      const score = window.prompt("Puntuación del control (0-100):", String(item.score ?? 0));
      const status = window.prompt("Estado de cumplimiento (valor numérico del enum):", String(item.status ?? 1));
      const evaluatedByUserId = window.prompt("ID del usuario evaluador:") ?? "";
      const notes = window.prompt("Notas de evaluación:") ?? "";
      await apiClient.put(`/frameworks/evaluations/${item.auditId}/${item.controlId}`, {
        score: score === null || score === "" ? null : Number(score),
        status: Number(status ?? 1),
        notes: notes || null,
        evaluatedByUserId,
      });
    },
    onSuccess: () => evaluations.refetch(),
  });

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
          <label>ID del control<input value={controlId} onChange={(event) => setControlId(event.target.value)} placeholder="GUID del control" /></label>
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
          <label>ID de auditoría<input value={auditId} onChange={(event) => setAuditId(event.target.value)} placeholder="GUID de la auditoría" /></label>
        </div>
        {auditId && (
          <div className="table-scroll">
            <table className="data-table">
              <thead><tr><th>Control</th><th>Puntuación</th><th>Estado</th><th>Notas</th><th>Evaluado</th><th>Acciones</th></tr></thead>
              <tbody>{(evaluations.data ?? []).map((item) => (
                <tr key={item.id}>
                  <td>{item.controlCode}</td><td>{item.score ?? "—"}</td><td>{String(item.status)}</td><td>{item.notes ?? "—"}</td>
                  <td>{item.evaluatedAtUtc ? new Date(item.evaluatedAtUtc).toLocaleString() : "—"}</td>
                  <td><button className="text-action" type="button" onClick={() => evaluate.mutate(item)}>Evaluar</button></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
        )}
      </section>
    </main>
  );
}
