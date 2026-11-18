import { ResourceManager, type ResourceField, type ResourceRecord } from "../../../components/ResourceManager";
import { hasPermission, Permissions } from "../../auth/permissions";
import { getSession } from "../../auth/session";
import { useOrganizationLookup, useUserLookup } from "../../../hooks/useLookupOptions";
import { auditStatusLabels, labelFromMap } from "../../../lib/domainLabels";
import { formatDate } from "../../../lib/formatters";

// AuditStatus: 1 Borrador, 2 Planificada, 3 En ejecución, 4 Completada, 5 Cerrada, 6 Cancelada.
const status = (row: ResourceRecord) => Number(row.status);

export function AuditsPage() {
  const organizations = useOrganizationLookup();
  const users = useUserLookup();
  const canUpdate = hasPermission(Permissions.AuditsUpdate);

  // Sin ORGANIZATIONS.VIEW (p. ej. gestor de auditorías) la auditoría se crea en la organización de la sesión.
  const organizationField: ResourceField[] = organizations.allowed
    ? [{
        name: "organizationId",
        label: "Organización",
        type: "select",
        required: true,
        options: organizations.options,
        placeholder: organizations.isLoading ? "Cargando organizaciones..." : "Selecciona una organización",
      }]
    : [];

  return (
    <ResourceManager
      title="Auditorías"
      description="Planifica, ejecuta y da seguimiento al ciclo completo de auditoría. El código se genera automáticamente."
      endpoint="/audits"
      queryKey="audits"
      canCreate={hasPermission(Permissions.AuditsCreate)}
      canUpdate={(row) => canUpdate && [1, 2].includes(status(row))}
      emptyMessage="Todavía no hay auditorías registradas."
      columns={[
        { key: "code", label: "Código" },
        { key: "title", label: "Título" },
        { key: "organizationName", label: "Organización" },
        { key: "leadAuditorName", label: "Auditor líder" },
        { key: "status", label: "Estado", render: (value) => labelFromMap(value, auditStatusLabels) },
        { key: "startDateUtc", label: "Inicio", render: formatDate },
        { key: "endDateUtc", label: "Fin", render: formatDate },
      ]}
      createFields={[
        ...organizationField,
        { name: "title", label: "Título", required: true, maxLength: 250 },
        { name: "objective", label: "Objetivo", type: "textarea" },
        { name: "scope", label: "Alcance", type: "textarea" },
      ]}
      mapCreate={(values) => ({
        organizationId: values.organizationId || getSession()?.organizationId,
        title: values.title,
        objective: values.objective || null,
        scope: values.scope || null,
      })}
      updateFields={[
        { name: "title", label: "Título", required: true, maxLength: 250 },
        { name: "objective", label: "Objetivo", type: "textarea" },
        { name: "scope", label: "Alcance", type: "textarea" },
      ]}
      mapUpdate={(values) => ({
        title: values.title,
        objective: values.objective || null,
        scope: values.scope || null,
      })}
      rowActions={[
        {
          label: "Planificar",
          permission: Permissions.AuditsUpdate,
          endpoint: (row) => `/audits/${row.id}/plan`,
          isVisible: (row) => status(row) === 1,
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
          successMessage: "Auditoría planificada correctamente.",
          body: (_row, values) => ({
            leadAuditorUserId: values.leadAuditorUserId,
            startDateUtc: new Date(values.startDateUtc).toISOString(),
            endDateUtc: new Date(values.endDateUtc).toISOString(),
          }),
        },
        {
          label: "Iniciar",
          permission: Permissions.AuditsExecute,
          endpoint: (row) => `/audits/${row.id}/start`,
          isVisible: (row) => status(row) === 2,
          successMessage: "Auditoría iniciada.",
        },
        {
          label: "Completar",
          permission: Permissions.AuditsExecute,
          endpoint: (row) => `/audits/${row.id}/complete`,
          isVisible: (row) => status(row) === 3,
          successMessage: "Auditoría completada.",
        },
        {
          label: "Cerrar",
          permission: Permissions.AuditsClose,
          endpoint: (row) => `/audits/${row.id}/close`,
          isVisible: (row) => status(row) === 4,
          successMessage: "Auditoría cerrada.",
          confirm: (row) => ({
            title: "Cerrar auditoría",
            message: `¿Cerrar definitivamente la auditoría «${String(row.code)}»? Una auditoría cerrada ya no puede modificarse.`,
            confirmLabel: "Cerrar auditoría",
            tone: "danger",
          }),
        },
        {
          label: "Cancelar",
          permission: Permissions.AuditsUpdate,
          endpoint: (row) => `/audits/${row.id}/cancel`,
          isVisible: (row) => [1, 2, 3, 4].includes(status(row)),
          successMessage: "Auditoría cancelada.",
          confirm: (row) => ({
            title: "Cancelar auditoría",
            message: `¿Cancelar la auditoría «${String(row.code)}»? Esta acción no se puede deshacer.`,
            confirmLabel: "Cancelar auditoría",
            cancelLabel: "Volver",
            tone: "danger",
          }),
        },
      ]}
    />
  );
}
