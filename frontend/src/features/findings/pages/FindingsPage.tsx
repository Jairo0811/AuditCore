import { ResourceManager } from "../../../components/ResourceManager";
import { useLookupOptions } from "../../../hooks/useLookupOptions";

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

interface UserLookup {
  id: string;
  fullName: string;
  email: string;
}

export function FindingsPage() {
  const audits = useLookupOptions<AuditLookup>(
    "audits",
    "/audits",
    (item) => `${item.code} — ${item.title}`,
  );

  const risks = useLookupOptions<RiskLookup>(
    "risks",
    "/risks",
    (item) => `${item.code} — ${item.title}`,
  );

  const users = useLookupOptions<UserLookup>(
    "users",
    "/users",
    (item) => `${item.fullName} — ${item.email}`,
  );

  return (
    <ResourceManager
      title="Hallazgos"
      description="Documenta condiciones, criterios, causas, efectos, recomendaciones y seguimiento. El código se genera automáticamente."
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
        {
          name: "auditId",
          label: "Auditoría",
          type: "select",
          required: true,
          options: audits.options,
          clearFieldsOnChange: ["riskId"],
          placeholder: audits.isLoading ? "Cargando auditorías..." : "Selecciona una auditoría",
        },
        {
          name: "riskId",
          label: "Riesgo relacionado (opcional)",
          type: "select",
          options: (values) => risks.records
            .filter((risk) => !values.auditId || risk.auditId === values.auditId)
            .map((risk) => ({ label: `${risk.code} — ${risk.title}`, value: risk.id })),
          placeholder: "Sin riesgo relacionado / selecciona un riesgo",
        },
        { name: "title", label: "Título", required: true },
        { name: "condition", label: "Condición", type: "textarea", required: true },
        { name: "criteria", label: "Criterio", type: "textarea", required: true },
        { name: "cause", label: "Causa", type: "textarea" },
        { name: "effect", label: "Efecto", type: "textarea" },
        { name: "recommendation", label: "Recomendación", type: "textarea" },
        {
          name: "severity",
          label: "Severidad",
          type: "select",
          required: true,
          options: [
            { label: "Baja", value: 1 },
            { label: "Media", value: 2 },
            { label: "Alta", value: 3 },
            { label: "Crítica", value: 4 },
          ],
        },
        {
          name: "responsibleUserId",
          label: "Responsable (opcional)",
          type: "select",
          options: users.options,
          placeholder: "Sin responsable / selecciona un usuario",
        },
        { name: "dueDateUtc", label: "Fecha límite", type: "datetime-local" },
      ]}
      updateFields={[
        {
          name: "riskId",
          label: "Riesgo relacionado (opcional)",
          type: "select",
          options: risks.options,
          placeholder: "Sin riesgo relacionado / selecciona un riesgo",
        },
        { name: "title", label: "Título", required: true },
        { name: "condition", label: "Condición", type: "textarea", required: true },
        { name: "criteria", label: "Criterio", type: "textarea", required: true },
        { name: "cause", label: "Causa", type: "textarea" },
        { name: "effect", label: "Efecto", type: "textarea" },
        { name: "recommendation", label: "Recomendación", type: "textarea" },
        {
          name: "severity",
          label: "Severidad",
          type: "select",
          required: true,
          options: [
            { label: "Baja", value: 1 },
            { label: "Media", value: 2 },
            { label: "Alta", value: 3 },
            { label: "Crítica", value: 4 },
          ],
        },
        {
          name: "responsibleUserId",
          label: "Responsable (opcional)",
          type: "select",
          options: users.options,
          placeholder: "Sin responsable / selecciona un usuario",
        },
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
