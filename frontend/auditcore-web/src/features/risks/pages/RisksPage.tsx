import { ResourceManager } from "../../../components/ResourceManager";

export function RisksPage() {
  return (
    <ResourceManager
      title="Riesgos"
      description="Registra, evalúa y da seguimiento al tratamiento de riesgos asociados a auditorías."
      endpoint="/risks"
      queryKey="risks"
      columns={[
        { key: "code", label: "Código" },
        { key: "auditCode", label: "Auditoría" },
        { key: "title", label: "Riesgo" },
        { key: "score", label: "Puntuación" },
        { key: "level", label: "Nivel" },
        { key: "ownerName", label: "Responsable" },
        { key: "status", label: "Estado" },
      ]}
      createFields={[
        { name: "auditId", label: "ID de auditoría", required: true },
        { name: "code", label: "Código", required: true },
        { name: "title", label: "Título", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        { name: "probability", label: "Probabilidad (1-5)", type: "number", required: true, defaultValue: 1 },
        { name: "impact", label: "Impacto (1-5)", type: "number", required: true, defaultValue: 1 },
        { name: "treatment", label: "Tratamiento", type: "textarea" },
        { name: "ownerUserId", label: "ID de responsable" },
      ]}
      updateFields={[
        { name: "code", label: "Código", required: true },
        { name: "title", label: "Título", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        { name: "probability", label: "Probabilidad (1-5)", type: "number", required: true },
        { name: "impact", label: "Impacto (1-5)", type: "number", required: true },
        { name: "treatment", label: "Tratamiento", type: "textarea" },
        { name: "ownerUserId", label: "ID de responsable" },
      ]}
      rowActions={[
        { label: "Tratar", endpoint: (row) => `/risks/${row.id}/start-treatment` },
        { label: "Aceptar", endpoint: (row) => `/risks/${row.id}/accept` },
        { label: "Mitigar", endpoint: (row) => `/risks/${row.id}/mitigate` },
        { label: "Cerrar", endpoint: (row) => `/risks/${row.id}/close`, confirm: "¿Cerrar este riesgo?" },
      ]}
    />
  );
}
