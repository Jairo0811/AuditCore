import { ResourceManager } from "../../../components/ResourceManager";
import { useLookupOptions } from "../../../hooks/useLookupOptions";

interface OrganizationLookup {
  id: string;
  code: string;
  name: string;
}

interface RoleLookup {
  id: string;
  code: string;
  name: string;
}

export function UsersPage() {
  const organizations = useLookupOptions<OrganizationLookup>(
    "organizations",
    "/organizations",
    (item) => `${item.code} — ${item.name}`,
  );

  const roles = useLookupOptions<RoleLookup>(
    "roles",
    "/roles",
    (item) => `${item.code} — ${item.name}`,
  );

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
        {
          name: "organizationId",
          label: "Organización",
          type: "select",
          required: true,
          options: organizations.options,
          placeholder: organizations.isLoading ? "Cargando organizaciones..." : "Selecciona una organización",
        },
        { name: "firstName", label: "Nombre", required: true },
        { name: "lastName", label: "Apellido", required: true },
        { name: "email", label: "Correo", required: true },
        { name: "password", label: "Contraseña", type: "password", required: true },
        {
          name: "roleIds",
          label: "Roles",
          type: "multiselect",
          options: roles.options,
          placeholder: "Selecciona uno o varios roles",
        },
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
        roleIds: values.roleIds ? values.roleIds.split(",").filter(Boolean) : [],
      })}
      rowActions={[
        { label: "Activar", endpoint: (row) => `/users/${row.id}/activate` },
        { label: "Desactivar", endpoint: (row) => `/users/${row.id}/deactivate` },
        { label: "Bloquear", endpoint: (row) => `/users/${row.id}/lock` },
        { label: "Desbloquear", endpoint: (row) => `/users/${row.id}/unlock` },
        {
          label: "Contraseña",
          endpoint: (row) => `/users/${row.id}/password`,
          fields: [
            { name: "password", label: "Nueva contraseña", type: "password", required: true },
          ],
          submitLabel: "Cambiar contraseña",
          body: (_row, values) => ({ password: values.password }),
        },
      ]}
    />
  );
}
