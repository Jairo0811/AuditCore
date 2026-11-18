import { ResourceManager, type ResourceField, type ResourceRecord } from "../../../components/ResourceManager";
import { hasPermission, Permissions } from "../../auth/permissions";
import { useLookupOptions, useUserLookup } from "../../../hooks/useLookupOptions";
import { actionPlanStatusLabels, labelFromMap } from "../../../lib/domainLabels";
import { formatDate } from "../../../lib/formatters";

interface FindingLookup {
  id: string;
  code: string;
  title: string;
}

// ActionPlanStatus: 1 Pendiente, 2 En progreso, 3 Completado, 4 Vencido, 5 Cancelado.
const isActionablePlan = (row: ResourceRecord) => [1, 2, 4].includes(Number(row.status));

export function ActionPlansPage() {
  const findings = useLookupOptions<FindingLookup>("findings", "/findings", (item) => `${item.code} — ${item.title}`, [Permissions.FindingsView]);
  const users = useUserLookup();
  const canManage = hasPermission(Permissions.ActionPlansManage);

  const editableFields: ResourceField[] = [
    { name: "title", label: "Título", required: true, maxLength: 250 },
    { name: "description", label: "Descripción", type: "textarea" },
    {
      name: "responsibleUserId",
      label: "Responsable",
      type: "select",
      required: true,
      options: users.options,
      placeholder: users.isLoading ? "Cargando usuarios..." : "Selecciona un responsable",
    },
    { name: "dueDateUtc", label: "Fecha límite", type: "datetime-local", required: true },
  ];

  return (
    <ResourceManager
      title="Planes de acción"
      description="Gestiona responsables, fechas compromiso, avance, vencimientos y cierre de acciones correctivas."
      endpoint="/action-plans"
      queryKey="action-plans"
      canCreate={canManage}
      canUpdate={(row) => canManage && isActionablePlan(row)}
      getRowLabel={(row) => String(row.title ?? "plan de acción")}
      emptyMessage="Todavía no hay planes de acción registrados."
      columns={[
        { key: "findingCode", label: "Hallazgo" },
        { key: "title", label: "Plan" },
        { key: "responsibleName", label: "Responsable" },
        { key: "dueDateUtc", label: "Fecha límite", render: formatDate },
        { key: "progressPercent", label: "Avance", render: (value) => `${String(value ?? 0)}%` },
        { key: "status", label: "Estado", render: (value) => labelFromMap(value, actionPlanStatusLabels) },
      ]}
      createFields={[
        {
          name: "findingId",
          label: "Hallazgo",
          type: "select",
          required: true,
          options: findings.options,
          placeholder: findings.isLoading ? "Cargando hallazgos..." : "Selecciona un hallazgo",
        },
        ...editableFields,
      ]}
      updateFields={editableFields}
      rowActions={[
        {
          label: "Progreso",
          permission: Permissions.ActionPlansManage,
          endpoint: (row) => `/action-plans/${row.id}/progress`,
          isVisible: isActionablePlan,
          fields: [
            { name: "progressPercent", label: "Nuevo progreso (%)", type: "number", min: 0, max: 100, required: true },
          ],
          getInitialValues: (row) => ({ progressPercent: String(row.progressPercent ?? 0) }),
          submitLabel: "Actualizar progreso",
          successMessage: "Progreso actualizado.",
          body: (_row, values) => ({ progressPercent: Number(values.progressPercent) }),
        },
        {
          label: "Completar",
          permission: Permissions.ActionPlansManage,
          endpoint: (row) => `/action-plans/${row.id}/complete`,
          isVisible: isActionablePlan,
          fields: [
            { name: "notes", label: "Notas de finalización", type: "textarea", placeholder: "Describe el resultado final de la acción." },
          ],
          submitLabel: "Completar plan",
          successMessage: "Plan de acción completado.",
          body: (_row, values) => ({ notes: values.notes || null }),
        },
        {
          label: "Cancelar",
          permission: Permissions.ActionPlansManage,
          endpoint: (row) => `/action-plans/${row.id}/cancel`,
          isVisible: isActionablePlan,
          successMessage: "Plan de acción cancelado.",
          confirm: (row) => ({
            title: "Cancelar plan de acción",
            message: `¿Cancelar el plan «${String(row.title)}»? Esta acción no se puede deshacer.`,
            confirmLabel: "Cancelar plan",
            cancelLabel: "Volver",
            tone: "danger",
          }),
        },
      ]}
    />
  );
}
