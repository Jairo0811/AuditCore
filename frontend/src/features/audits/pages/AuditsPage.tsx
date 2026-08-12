import { ResourceManager } from "../../../components/ResourceManager";
import { auditStatusLabels, labelFromMap } from "../../../lib/domainLabels";
import { useLookupOptions } from "../../../hooks/useLookupOptions";

interface OrganizationLookup {
  id: string;
  code: string;
  name: string;
}

interface UserLookup {
  id: string;
  fullName: string;
  email: string;
}

export function AuditsPage() {
  const organizations = useLookupOptions<OrganizationLookup>(
    "organizations",
    "/organizations",
    (item) => `${item.code} — ${item.name}`,
  );

  const users = useLookupOptions<UserLookup>(
    "users",
    "/users",
    (item) => `${item.fullName} — ${item.email}`,
  );

  return (
    <ResourceManager
      title="Auditorías"
      description="Planifica, ejecuta y da seguimiento al ciclo completo de auditoría. El código se genera automáticamente."
      endpoint="/audits"
      queryKey="audits"
      canEdit={(row) => [1, 2].includes(Number(row.status))}
      columns={[
        { key: "code", label: "Código" },
        { key: "title", label: "Título" },
        { key: "organizationName", label: "Organización" },
        { key: "leadAuditorName", label: "Auditor líder" },
        { key: "status", label: "Estado", render: (value) => labelFromMap(value, auditStatusLabels) },
        { key: "startDateUtc", label: "Inicio", render: (value) => typeof value === "string" ? new Date(value).toLocaleDateString() : "—" },
        { key: "endDateUtc", label: "Fin", render: (value) => typeof value === "string" ? new Date(value).toLocaleDateString() : "—" },
      ]}
      createFields={[
        {
          name: "organizationId",
          label: "Organización",
          type: "select",
          required: true,
          options: organizations.options,
          placeholder: organizations.isLoading ? "Cargando organizaciones..." : "Selecciona una organización",
        },
        { name: "title", label: "Título", required: true },
        { name: "objective", label: "Objetivo", type: "textarea" },
        { name: "scope", label: "Alcance", type: "textarea" },
      ]}
      updateFields={[
        { name: "title", label: "Título", required: true },
        { name: "objective", label: "Objetivo", type: "textarea" },
        { name: "scope", label: "Alcance", type: "textarea" },
      ]}
      rowActions={[
        {
          label: "Planificar",
          endpoint: (row) => `/audits/${row.id}/plan`,
          isVisible: (row) => [1, 2].includes(Number(row.status)),
          fields: [
            {
              name: "leadAuditorUserId",
              label: "Auditor líder",
              type: "select",
              required: true,
              options: users.options,
              placeholder: users.isLoading ? "Cargando usuarios..." : "Selecciona un auditor líder",
            },
            { name: "startDateUtc", label: "Fecha de inicio", type: "datetime-local", required: true },
            { name: "endDateUtc", label: "Fecha de fin", type: "datetime-local", required: true },
          ],
          submitLabel: "Planificar auditoría",
          body: (_row, values) => ({
            leadAuditorUserId: values.leadAuditorUserId,
            startDateUtc: new Date(values.startDateUtc).toISOString(),
            endDateUtc: new Date(values.endDateUtc).toISOString(),
          }),
        },
        { label: "Iniciar", endpoint: (row) => `/audits/${row.id}/start`, isVisible: (row) => Number(row.status) === 2 },
        { label: "Completar", endpoint: (row) => `/audits/${row.id}/complete`, isVisible: (row) => Number(row.status) === 3 },
        { label: "Cerrar", endpoint: (row) => `/audits/${row.id}/close`, isVisible: (row) => Number(row.status) === 4, confirm: "¿Cerrar definitivamente esta auditoría?" },
        { label: "Cancelar", endpoint: (row) => `/audits/${row.id}/cancel`, isVisible: (row) => [1, 2, 3].includes(Number(row.status)), confirm: "¿Cancelar esta auditoría?" },
      ]}
    />
  );
}
