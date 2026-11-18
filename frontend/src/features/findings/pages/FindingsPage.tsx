import { ResourceManager, type FormValues, type ResourceField, type ResourceRecord } from "../../../components/ResourceManager";
import { hasPermission, Permissions } from "../../auth/permissions";
import { useLookupOptions, useUserLookup } from "../../../hooks/useLookupOptions";
import { findingSeverityLabels, findingStatusLabels, labelFromMap } from "../../../lib/domainLabels";
import { formatDate } from "../../../lib/formatters";

interface AuditLookup {
  id: string;
  code: string;
  title: string;
}

interface RiskLookup {
  id: string;
  auditId: string;
  code: string;
  title: string;
}

// FindingStatus: 1 Abierto, 2 En revisión, 3 Aceptado, 4 Resuelto, 5 Cerrado.
const status = (row: ResourceRecord) => Number(row.status);

const severityOptions = [
  { label: "Baja", value: 1 },
  { label: "Media", value: 2 },
  { label: "Alta", value: 3 },
  { label: "Crítica", value: 4 },
];

function mapFindingBody(values: FormValues) {
  return {
    riskId: values.riskId || null,
    title: values.title,
    condition: values.condition,
    criteria: values.criteria,
    cause: values.cause || null,
    effect: values.effect || null,
    recommendation: values.recommendation || null,
    severity: Number(values.severity),
    responsibleUserId: values.responsibleUserId || null,
    dueDateUtc: values.dueDateUtc ? new Date(values.dueDateUtc).toISOString() : null,
  };
}

export function FindingsPage() {
  const audits = useLookupOptions<AuditLookup>("audits", "/audits", (item) => `${item.code} — ${item.title}`, [Permissions.AuditsView]);
  const risks = useLookupOptions<RiskLookup>("risks", "/risks", (item) => `${item.code} — ${item.title}`, [Permissions.RisksView]);
  const users = useUserLookup();
  const canManage = hasPermission(Permissions.FindingsManage);

  const riskField: ResourceField = {
    name: "riskId",
    label: "Riesgo relacionado (opcional)",
    type: "select",
    options: (values) => risks.records
      .filter((risk) => !values.auditId || risk.auditId === values.auditId)
      .map((risk) => ({ label: `${risk.code} — ${risk.title}`, value: risk.id })),
    placeholder: "Sin riesgo relacionado / selecciona un riesgo",
  };

  const detailFields: ResourceField[] = [
    { name: "title", label: "Título", required: true, maxLength: 250 },
    { name: "condition", label: "Condición", type: "textarea", required: true },
    { name: "criteria", label: "Criterio", type: "textarea", required: true },
    { name: "cause", label: "Causa", type: "textarea" },
    { name: "effect", label: "Efecto", type: "textarea" },
    { name: "recommendation", label: "Recomendación", type: "textarea" },
    { name: "severity", label: "Severidad", type: "select", required: true, options: severityOptions },
    {
      name: "responsibleUserId",
      label: "Responsable (opcional)",
      type: "select",
      options: users.options,
      placeholder: "Sin responsable / selecciona un usuario",
    },
    { name: "dueDateUtc", label: "Fecha límite", type: "datetime-local" },
  ];

  return (
    <ResourceManager
      title="Hallazgos"
      description="Documenta condiciones, criterios, causas, efectos, recomendaciones y seguimiento. El código se genera automáticamente."
      endpoint="/findings"
      queryKey="findings"
      canCreate={canManage}
      canUpdate={(row) => canManage && status(row) !== 5}
      emptyMessage="Todavía no hay hallazgos registrados."
      mapCreate={(values) => ({ auditId: values.auditId, ...mapFindingBody(values) })}
      mapUpdate={(values) => mapFindingBody(values)}
      columns={[
        { key: "code", label: "Código" },
        { key: "auditCode", label: "Auditoría" },
        { key: "title", label: "Hallazgo" },
        { key: "severity", label: "Severidad", render: (value) => labelFromMap(value, findingSeverityLabels) },
        { key: "responsibleName", label: "Responsable" },
        { key: "dueDateUtc", label: "Vence", render: formatDate },
        { key: "status", label: "Estado", render: (value) => labelFromMap(value, findingStatusLabels) },
      ]}
      createFields={[
        {
          name: "auditId",
          label: "Auditoría",
          type: "select",
          required: true,
          options: audits.options,
          clearFieldsOnChange: ["riskId"],
          placeholder: audits.isLoading ? "Cargando auditorías..." : "Selecciona una auditoría",
        },
        riskField,
        ...detailFields,
      ]}
      updateFields={[riskField, ...detailFields]}
      rowActions={[
        {
          label: "Enviar a revisión",
          permission: Permissions.FindingsManage,
          endpoint: (row) => `/findings/${row.id}/review`,
          isVisible: (row) => status(row) === 1,
          successMessage: "Hallazgo enviado a revisión.",
        },
        {
          label: "Aceptar",
          permission: Permissions.FindingsManage,
          endpoint: (row) => `/findings/${row.id}/accept`,
          isVisible: (row) => status(row) === 2,
          successMessage: "Hallazgo aceptado.",
        },
        {
          label: "Resolver",
          permission: Permissions.FindingsManage,
          endpoint: (row) => `/findings/${row.id}/resolve`,
          isVisible: (row) => status(row) === 3,
          successMessage: "Hallazgo resuelto.",
        },
        {
          label: "Cerrar",
          permission: Permissions.FindingsManage,
          endpoint: (row) => `/findings/${row.id}/close`,
          isVisible: (row) => status(row) === 4,
          successMessage: "Hallazgo cerrado.",
          confirm: (row) => ({
            title: "Cerrar hallazgo",
            message: `¿Cerrar el hallazgo «${String(row.code)}»? Un hallazgo cerrado ya no puede modificarse.`,
            confirmLabel: "Cerrar hallazgo",
            tone: "danger",
          }),
        },
      ]}
    />
  );
}
