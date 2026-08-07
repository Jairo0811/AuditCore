import { ResourceManager } from "../../../components/ResourceManager";

export function FindingsPage() {
  return (
    <ResourceManager
      title="Hallazgos"
      description="Documenta condiciones, criterios, causas, efectos, recomendaciones y seguimiento."
      endpoint="/findings"
      queryKey="findings"
      columns={[
        { key: "code", label: "Código" },
        { key: "auditCode", label: "Auditoría" },
        { key: "title", label: "Hallazgo" },
        { key: "severity", label: "Severidad" },
        { key: "responsibleName", label: "Responsable" },
        { key: "dueDateUtc", label: "Vence", render: (value) => typeof value === "string" ? new Date(value).toLocaleDateString() : "—" },
        { key: "status", label: "Estado" },
      ]}
      createFields={[
        { name: "auditId", label: "ID de auditoría", required: true },
        { name: "riskId", label: "ID de riesgo relacionado" },
        { name: "code", label: "Código", required: true },
        { name: "title", label: "Título", required: true },
        { name: "condition", label: "Condición", type: "textarea", required: true },
        { name: "criteria", label: "Criterio", type: "textarea", required: true },
        { name: "cause", label: "Causa", type: "textarea" },
        { name: "effect", label: "Efecto", type: "textarea" },
        { name: "recommendation", label: "Recomendación", type: "textarea" },
        { name: "severity", label: "Severidad", type: "select", required: true, options: [
          { label: "Baja", value: 1 }, { label: "Media", value: 2 }, { label: "Alta", value: 3 }, { label: "Crítica", value: 4 },
        ] },
        { name: "responsibleUserId", label: "ID de responsable" },
        { name: "dueDateUtc", label: "Fecha límite", type: "datetime-local" },
      ]}
      updateFields={[
        { name: "riskId", label: "ID de riesgo relacionado" },
        { name: "code", label: "Código", required: true },
        { name: "title", label: "Título", required: true },
        { name: "condition", label: "Condición", type: "textarea", required: true },
        { name: "criteria", label: "Criterio", type: "textarea", required: true },
        { name: "cause", label: "Causa", type: "textarea" },
        { name: "effect", label: "Efecto", type: "textarea" },
        { name: "recommendation", label: "Recomendación", type: "textarea" },
        { name: "severity", label: "Severidad", type: "select", required: true, options: [
          { label: "Baja", value: 1 }, { label: "Media", value: 2 }, { label: "Alta", value: 3 }, { label: "Crítica", value: 4 },
        ] },
        { name: "responsibleUserId", label: "ID de responsable" },
        { name: "dueDateUtc", label: "Fecha límite", type: "datetime-local" },
      ]}
      rowActions={[
        { label: "Revisar", endpoint: (row) => `/findings/${row.id}/review` },
        { label: "Aceptar", endpoint: (row) => `/findings/${row.id}/accept` },
        { label: "Resolver", endpoint: (row) => `/findings/${row.id}/resolve` },
        { label: "Cerrar", endpoint: (row) => `/findings/${row.id}/close`, confirm: "¿Cerrar este hallazgo?" },
      ]}
    />
  );
}
