import { ResourceManager, type ResourceField, type ResourceRecord } from "../../../components/ResourceManager";
import { hasPermission, Permissions } from "../../auth/permissions";
import { useLookupOptions, useUserLookup } from "../../../hooks/useLookupOptions";
import { labelFromMap, riskLevelLabels, riskStatusLabels } from "../../../lib/domainLabels";

interface AuditLookup {
  id: string;
  code: string;
  title: string;
}

// RiskStatus: 1 Identificado, 2 En tratamiento, 3 Aceptado, 4 Mitigado, 5 Cerrado.
const status = (row: ResourceRecord) => Number(row.status);

export function RisksPage() {
  const audits = useLookupOptions<AuditLookup>("audits", "/audits", (item) => `${item.code} — ${item.title}`, [Permissions.AuditsView]);
  const users = useUserLookup();
  const canManage = hasPermission(Permissions.RisksManage);

  const editableFields: ResourceField[] = [
    { name: "title", label: "Título", required: true, maxLength: 250 },
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
  ];

  return (
    <ResourceManager
      title="Riesgos"
      description="Registra, evalúa y da seguimiento al tratamiento de riesgos asociados a auditorías. El código se genera automáticamente."
      endpoint="/risks"
      queryKey="risks"
      canCreate={canManage}
      canUpdate={(row) => canManage && status(row) !== 5}
      emptyMessage="Todavía no hay riesgos registrados."
      columns={[
        { key: "code", label: "Código" },
        { key: "auditCode", label: "Auditoría" },
        { key: "title", label: "Riesgo" },
        { key: "score", label: "Puntuación" },
        { key: "level", label: "Nivel", render: (value) => labelFromMap(value, riskLevelLabels) },
        { key: "ownerName", label: "Responsable" },
        { key: "status", label: "Estado", render: (value) => labelFromMap(value, riskStatusLabels) },
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
        ...editableFields,
      ]}
      updateFields={editableFields}
      rowActions={[
        {
          label: "Tratar",
          permission: Permissions.RisksManage,
          endpoint: (row) => `/risks/${row.id}/start-treatment`,
          isVisible: (row) => status(row) === 1,
          successMessage: "Riesgo en tratamiento.",
        },
        {
          label: "Aceptar",
          permission: Permissions.RisksManage,
          endpoint: (row) => `/risks/${row.id}/accept`,
          isVisible: (row) => [1, 2, 4].includes(status(row)),
          successMessage: "Riesgo aceptado.",
        },
        {
          label: "Mitigar",
          permission: Permissions.RisksManage,
          endpoint: (row) => `/risks/${row.id}/mitigate`,
          isVisible: (row) => [1, 2].includes(status(row)),
          successMessage: "Riesgo mitigado.",
        },
        {
          label: "Cerrar",
          permission: Permissions.RisksManage,
          endpoint: (row) => `/risks/${row.id}/close`,
          isVisible: (row) => [3, 4].includes(status(row)),
          successMessage: "Riesgo cerrado.",
          confirm: (row) => ({
            title: "Cerrar riesgo",
            message: `¿Cerrar el riesgo «${String(row.code)}»? Un riesgo cerrado ya no puede modificarse.`,
            confirmLabel: "Cerrar riesgo",
            tone: "danger",
          }),
        },
      ]}
    />
  );
}
