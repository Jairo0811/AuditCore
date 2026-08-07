import { ResourceManager } from "../../../components/ResourceManager";

export function ActionPlansPage() {
  return (
    <ResourceManager
      title="Planes de acción"
      description="Gestiona responsables, fechas compromiso, avance, vencimientos y cierre de acciones correctivas."
      endpoint="/action-plans"
      queryKey="action-plans"
      columns={[
        { key: "findingCode", label: "Hallazgo" },
        { key: "title", label: "Plan" },
        { key: "responsibleName", label: "Responsable" },
        { key: "dueDateUtc", label: "Fecha límite", render: (value) => typeof value === "string" ? new Date(value).toLocaleDateString() : "—" },
        { key: "progressPercent", label: "Avance", render: (value) => `${String(value ?? 0)}%` },
        { key: "status", label: "Estado" },
      ]}
      createFields={[
        { name: "findingId", label: "ID de hallazgo", required: true },
        { name: "title", label: "Título", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        { name: "responsibleUserId", label: "ID de responsable", required: true },
        { name: "dueDateUtc", label: "Fecha límite", type: "datetime-local", required: true },
      ]}
      updateFields={[
        { name: "title", label: "Título", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        { name: "responsibleUserId", label: "ID de responsable", required: true },
        { name: "dueDateUtc", label: "Fecha límite", type: "datetime-local", required: true },
      ]}
      rowActions={[
        {
          label: "Progreso",
          endpoint: (row) => `/action-plans/${row.id}/progress`,
          body: () => ({ progressPercent: Number(window.prompt("Nuevo progreso (0-100):", "50") ?? 0) }),
        },
        {
          label: "Completar",
          endpoint: (row) => `/action-plans/${row.id}/complete`,
          body: () => ({ notes: window.prompt("Notas de finalización:") }),
        },
        { label: "Cancelar", endpoint: (row) => `/action-plans/${row.id}/cancel`, confirm: "¿Cancelar este plan de acción?" },
      ]}
    />
  );
}
