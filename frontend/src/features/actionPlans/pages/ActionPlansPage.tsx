import { ResourceManager } from "../../../components/ResourceManager";
import { actionPlanStatusLabels, labelFromMap } from "../../../lib/domainLabels";
import { useLookupOptions } from "../../../hooks/useLookupOptions";

interface FindingLookup {
  id: string;
  code: string;
  title: string;
}

interface UserLookup {
  id: string;
  fullName: string;
  email: string;
}

const isActionablePlan = (status: unknown) => [1, 2, 4].includes(Number(status));

export function ActionPlansPage() {
  const findings = useLookupOptions<FindingLookup>(
    "findings",
    "/findings",
    (item) => `${item.code} — ${item.title}`,
  );

  const users = useLookupOptions<UserLookup>(
    "users",
    "/users",
    (item) => `${item.fullName} — ${item.email}`,
  );

  return (
    <ResourceManager
      title="Planes de acción"
      description="Gestiona responsables, fechas compromiso, avance, vencimientos y cierre de acciones correctivas."
      endpoint="/action-plans"
      queryKey="action-plans"
      canEdit={(row) => isActionablePlan(row.status)}
      columns={[
        { key: "findingCode", label: "Hallazgo" },
        { key: "title", label: "Plan" },
        { key: "responsibleName", label: "Responsable" },
        { key: "dueDateUtc", label: "Fecha límite", render: (value) => typeof value === "string" ? new Date(value).toLocaleDateString() : "—" },
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
        { name: "title", label: "Título", required: true },
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
      ]}
      updateFields={[
        { name: "title", label: "Título", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        {
          name: "responsibleUserId",
          label: "Responsable",
          type: "select",
          required: true,
          options: users.options,
          placeholder: "Selecciona un responsable",
        },
        { name: "dueDateUtc", label: "Fecha límite", type: "datetime-local", required: true },
      ]}
      rowActions={[
        {
          label: "Progreso",
          endpoint: (row) => `/action-plans/${row.id}/progress`,
          isVisible: (row) => isActionablePlan(row.status),
          fields: [
            { name: "progressPercent", label: "Nuevo progreso (%)", type: "number", min: 0, max: 100, required: true, defaultValue: 50 },
          ],
          submitLabel: "Actualizar progreso",
          body: (_row, values) => ({ progressPercent: Number(values.progressPercent) }),
        },
        {
          label: "Completar",
          endpoint: (row) => `/action-plans/${row.id}/complete`,
          isVisible: (row) => isActionablePlan(row.status),
          fields: [
            { name: "notes", label: "Notas de finalización", type: "textarea", placeholder: "Describe el resultado final de la acción." },
          ],
          submitLabel: "Completar plan",
          body: (_row, values) => ({ notes: values.notes || null }),
        },
        {
          label: "Cancelar",
          endpoint: (row) => `/action-plans/${row.id}/cancel`,
          isVisible: (row) => isActionablePlan(row.status),
          confirm: "¿Cancelar este plan de acción?",
        },
      ]}
    />
  );
}
