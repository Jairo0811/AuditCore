import { ResourceManager } from "../../../components/ResourceManager";

export function AuditsPage() {
  return (
    <ResourceManager
      title="Auditorías"
      description="Planifica, ejecuta y da seguimiento al ciclo completo de auditoría."
      endpoint="/audits"
      queryKey="audits"
      columns={[
        { key: "code", label: "Código" },
        { key: "title", label: "Título" },
        { key: "organizationName", label: "Organización" },
        { key: "leadAuditorName", label: "Auditor líder" },
        { key: "status", label: "Estado" },
        { key: "startDateUtc", label: "Inicio", render: (value) => typeof value === "string" ? new Date(value).toLocaleDateString() : "—" },
        { key: "endDateUtc", label: "Fin", render: (value) => typeof value === "string" ? new Date(value).toLocaleDateString() : "—" },
      ]}
      createFields={[
        { name: "organizationId", label: "ID de organización", required: true },
        { name: "code", label: "Código", required: true },
        { name: "title", label: "Título", required: true },
        { name: "objective", label: "Objetivo", type: "textarea" },
        { name: "scope", label: "Alcance", type: "textarea" },
      ]}
      updateFields={[
        { name: "code", label: "Código", required: true },
        { name: "title", label: "Título", required: true },
        { name: "objective", label: "Objetivo", type: "textarea" },
        { name: "scope", label: "Alcance", type: "textarea" },
      ]}
      rowActions={[
        {
          label: "Planificar",
          endpoint: (row) => `/audits/${row.id}/plan`,
          body: () => {
            const leadAuditorUserId = window.prompt("ID del auditor líder:") ?? "";
            const startDateUtc = window.prompt("Inicio (YYYY-MM-DD):") ?? "";
            const endDateUtc = window.prompt("Fin (YYYY-MM-DD):") ?? "";
            return {
              leadAuditorUserId,
              startDateUtc: new Date(startDateUtc).toISOString(),
              endDateUtc: new Date(endDateUtc).toISOString(),
            };
          },
        },
        { label: "Iniciar", endpoint: (row) => `/audits/${row.id}/start` },
        { label: "Completar", endpoint: (row) => `/audits/${row.id}/complete` },
        { label: "Cerrar", endpoint: (row) => `/audits/${row.id}/close`, confirm: "¿Cerrar definitivamente esta auditoría?" },
        { label: "Cancelar", endpoint: (row) => `/audits/${row.id}/cancel`, confirm: "¿Cancelar esta auditoría?" },
      ]}
    />
  );
}
