import { ResourceManager } from "../../../components/ResourceManager";

export function UsersPage() {
  return (
    <ResourceManager
      title="Usuarios"
      description="Gestiona usuarios, acceso, bloqueo y pertenencia organizacional."
      endpoint="/users"
      queryKey="users"
      columns={[
        { key: "fullName", label: "Nombre" },
        { key: "email", label: "Correo" },
        { key: "organizationName", label: "Organización" },
        { key: "roles", label: "Roles", render: (value) => Array.isArray(value) ? value.join(", ") : "—" },
        { key: "isActive", label: "Activo" },
        { key: "isLocked", label: "Bloqueado" },
      ]}
      createFields={[
        { name: "organizationId", label: "ID de organización", required: true },
        { name: "firstName", label: "Nombre", required: true },
        { name: "lastName", label: "Apellido", required: true },
        { name: "email", label: "Correo", required: true },
        { name: "password", label: "Contraseña", required: true },
        { name: "roleIds", label: "IDs de roles (separados por coma)" },
      ]}
      updateFields={[
        { name: "firstName", label: "Nombre", required: true },
        { name: "lastName", label: "Apellido", required: true },
        { name: "email", label: "Correo", required: true },
      ]}
      mapCreate={(values) => ({
        organizationId: values.organizationId,
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        password: values.password,
        roleIds: values.roleIds ? values.roleIds.split(",").map((value) => value.trim()).filter(Boolean) : [],
      })}
      rowActions={[
        { label: "Activar", endpoint: (row) => `/users/${row.id}/activate` },
        { label: "Desactivar", endpoint: (row) => `/users/${row.id}/deactivate` },
        { label: "Bloquear", endpoint: (row) => `/users/${row.id}/lock` },
        { label: "Desbloquear", endpoint: (row) => `/users/${row.id}/unlock` },
        {
          label: "Contraseña",
          endpoint: (row) => `/users/${row.id}/password`,
          body: () => ({ password: window.prompt("Nueva contraseña:") ?? "" }),
        },
      ]}
    />
  );
}
