import { ResourceManager } from "../../../components/ResourceManager";
import { useLookupOptions } from "../../../hooks/useLookupOptions";

interface AuditLookup {
  id: string;
  code: string;
  title: string;
}

interface UserLookup {
  id: string;
  fullName: string;
  email: string;
}

export function RisksPage() {
  const audits = useLookupOptions<AuditLookup>(
    "audits",
    "/audits",
    (item) => `${item.code} — ${item.title}`,
  );

  const users = useLookupOptions<UserLookup>(
    "users",
    "/users",
    (item) => `${item.fullName} — ${item.email}`,
  );

  return (
    <ResourceManager
      title="Riesgos"
      description="Registra, evalúa y da seguimiento al tratamiento de riesgos asociados a auditorías. El código se genera automáticamente."
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
        {
          name: "auditId",
          label: "Auditoría",
          type: "select",
          required: true,
          options: audits.options,
          placeholder: audits.isLoading ? "Cargando auditorías..." : "Selecciona una auditoría",
        },
        { name: "title", label: "Título", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        { name: "probability", label: "Probabilidad (1-5)", type: "number", min: 1, max: 5, required: true, defaultValue: 1 },
        { name: "impact", label: "Impacto (1-5)", type: "number", min: 1, max: 5, required: true, defaultValue: 1 },
        { name: "treatment", label: "Tratamiento", type: "textarea" },
        {
          name: "ownerUserId",
          label: "Responsable (opcional)",
          type: "select",
          options: users.options,
          placeholder: "Sin responsable / selecciona un usuario",
        },
      ]}
      updateFields={[
        { name: "title", label: "Título", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        { name: "probability", label: "Probabilidad (1-5)", type: "number", min: 1, max: 5, required: true },
        { name: "impact", label: "Impacto (1-5)", type: "number", min: 1, max: 5, required: true },
        { name: "treatment", label: "Tratamiento", type: "textarea" },
        {
          name: "ownerUserId",
          label: "Responsable (opcional)",
          type: "select",
          options: users.options,
          placeholder: "Sin responsable / selecciona un usuario",
        },
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
