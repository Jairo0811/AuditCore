import { ResourceManager } from "../../../components/ResourceManager";
import { hasPermission, Permissions } from "../../auth/permissions";
import { useLookupOptions } from "../../../hooks/useLookupOptions";
import { codesToIds } from "../../../lib/codeMapping";
import { splitIds } from "../../../lib/formatters";

interface PermissionLookup {
  id: string;
  code: string;
  name: string;
}

const activeOptions = [
  { label: "Activo", value: "true" },
  { label: "Inactivo", value: "false" },
];

export function RolesPage() {
  const permissions = useLookupOptions<PermissionLookup>(
    "permissions",
    "/permissions",
    (item) => `${item.code} — ${item.name}`,
    [Permissions.RolesView],
  );
  const canManage = hasPermission(Permissions.RolesManage);

  return (
    <ResourceManager
      title="Roles"
      description="Administra perfiles de acceso y permisos asignados a cada rol."
      endpoint="/roles"
      queryKey="roles"
      canCreate={canManage}
      canUpdate={() => canManage}
      getRowLabel={(row) => String(row.code ?? row.name ?? "rol")}
      emptyMessage="No hay roles registrados."
      columns={[
        { key: "code", label: "Código" },
        { key: "name", label: "Nombre" },
        { key: "description", label: "Descripción" },
        { key: "permissions", label: "Permisos", render: (value) => `${Array.isArray(value) ? value.length : 0} permisos` },
        { key: "isActive", label: "Activo" },
      ]}
      createFields={[
        { name: "name", label: "Nombre", required: true, maxLength: 100 },
        { name: "code", label: "Código", required: true, maxLength: 50, helpText: "Identificador único en mayúsculas, por ejemplo AUDITOR_SENIOR." },
        { name: "description", label: "Descripción", type: "textarea" },
      ]}
      mapCreate={(values) => ({ name: values.name, code: values.code, description: values.description || null })}
      updateFields={[
        { name: "name", label: "Nombre", required: true, maxLength: 100 },
        { name: "code", label: "Código", required: true, maxLength: 50 },
        { name: "description", label: "Descripción", type: "textarea" },
        { name: "isActive", label: "Estado", type: "select", required: true, options: activeOptions },
      ]}
      mapUpdate={(values) => ({
        name: values.name,
        code: values.code,
        description: values.description || null,
        isActive: values.isActive === "true",
      })}
      rowActions={[
        {
          label: "Permisos",
          permission: Permissions.RolesManage,
          endpoint: (row) => `/roles/${row.id}/permissions`,
          // Solo se ofrece cuando el catálogo permite precargar exactamente la asignación actual.
          isVisible: (row) => permissions.isSuccess && codesToIds(row.permissions, permissions.records) !== null,
          description: "Marca los permisos del rol. Al guardar, la selección reemplaza la asignación actual.",
          fields: [
            {
              name: "permissionIds",
              label: "Permisos asignados",
              type: "multiselect",
              options: permissions.options,
            },
          ],
          getInitialValues: (row) => ({
            permissionIds: (codesToIds(row.permissions, permissions.records) ?? []).join(","),
          }),
          submitLabel: "Guardar permisos",
          successMessage: "Permisos del rol actualizados.",
          body: (_row, values) => ({ permissionIds: splitIds(values.permissionIds) }),
        },
      ]}
    />
  );
}
