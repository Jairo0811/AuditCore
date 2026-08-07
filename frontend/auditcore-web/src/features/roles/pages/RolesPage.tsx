import { ResourceManager } from "../../../components/ResourceManager";

export function RolesPage() {
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
        { name: "isActive", label: "Activo (true/false)", required: true },
      ]}
      mapUpdate={(values) => ({
        name: values.name,
        code: values.code,
        description: values.description || null,
        isActive: values.isActive === "true" || values.isActive === "1",
      })}
      rowActions={[
        {
          label: "Permisos",
          endpoint: (row) => `/roles/${row.id}/permissions`,
          body: () => ({
            permissionIds: (window.prompt("IDs de permisos separados por coma:") ?? "")
              .split(",")
              .map((value) => value.trim())
              .filter(Boolean),
          }),
        },
      ]}
    />
  );
}
