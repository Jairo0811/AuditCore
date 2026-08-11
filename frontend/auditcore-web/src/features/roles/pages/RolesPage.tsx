import { ResourceManager } from "../../../components/ResourceManager";
import { useLookupOptions } from "../../../hooks/useLookupOptions";

interface PermissionLookup {
  id: string;
  code: string;
  name: string;
}

export function RolesPage() {
  const permissions = useLookupOptions<PermissionLookup>(
    "permissions",
    "/permissions",
    (item) => `${item.code} — ${item.name}`,
  );

  return (
    <ResourceManager
      title="Roles"
      description="Administra perfiles de acceso y permisos asignados a cada rol."
      endpoint="/roles"
      queryKey="roles"
      columns={[
        { key: "code", label: "Código" },
        { key: "name", label: "Nombre" },
        { key: "description", label: "Descripción" },
        { key: "permissions", label: "Permisos", render: (value) => Array.isArray(value) ? `${value.length} permisos` : "0 permisos" },
        { key: "isActive", label: "Activo" },
      ]}
      createFields={[
        { name: "name", label: "Nombre", required: true },
        { name: "code", label: "Código", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
      ]}
      updateFields={[
        { name: "name", label: "Nombre", required: true },
        { name: "code", label: "Código", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        {
          name: "isActive",
          label: "Estado",
          type: "select",
          required: true,
          options: [
            { label: "Activo", value: "true" },
            { label: "Inactivo", value: "false" },
          ],
        },
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
          endpoint: (row) => `/roles/${row.id}/permissions`,
          fields: [
            {
              name: "permissionIds",
              label: "Permisos asignados",
              type: "multiselect",
              options: permissions.options,
            },
          ],
          submitLabel: "Guardar permisos",
          body: (_row, values) => ({
            permissionIds: values.permissionIds ? values.permissionIds.split(",").filter(Boolean) : [],
          }),
        },
      ]}
    />
  );
}
