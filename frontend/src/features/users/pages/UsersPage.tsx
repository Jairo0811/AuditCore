import { ResourceManager } from "../../../components/ResourceManager";
import { useLookupOptions } from "../../../hooks/useLookupOptions";

interface OrganizationLookup {
  id: string;
  code: string;
  name: string;
}

interface BranchLookup {
  id: string;
  organizationId: string;
  code: string;
  name: string;
  isActive: boolean;
}

interface DepartmentLookup {
  id: string;
  organizationId: string;
  branchId?: string | null;
  code: string;
  name: string;
  isActive: boolean;
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

  const branches = useLookupOptions<BranchLookup>(
    "branches",
    "/branches",
    (item) => `${item.code} — ${item.name}`,
  );

  const departments = useLookupOptions<DepartmentLookup>(
    "departments",
    "/departments",
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
      description="Gestiona usuarios, acceso, roles y pertenencia a organización, sucursal y departamento."
      endpoint="/users"
      queryKey="users"
      columns={[
        { key: "fullName", label: "Nombre" },
        { key: "email", label: "Correo" },
        { key: "organizationName", label: "Organización" },
        { key: "branchName", label: "Sucursal" },
        { key: "departmentName", label: "Departamento" },
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
          clearFieldsOnChange: ["branchId", "departmentId"],
          placeholder: organizations.isLoading ? "Cargando organizaciones..." : "Selecciona una organización",
        },
        {
          name: "branchId",
          label: "Sucursal (opcional)",
          type: "select",
          options: (values) => branches.records
            .filter((branch) => branch.isActive && (!values.organizationId || branch.organizationId === values.organizationId))
            .map((branch) => ({ label: `${branch.code} — ${branch.name}`, value: branch.id })),
          clearFieldsOnChange: ["departmentId"],
          placeholder: "Sin sucursal / selecciona una sucursal",
        },
        {
          name: "departmentId",
          label: "Departamento (opcional)",
          type: "select",
          options: (values) => departments.records
            .filter((department) => department.isActive)
            .filter((department) => !values.organizationId || department.organizationId === values.organizationId)
            .filter((department) => !values.branchId || !department.branchId || department.branchId === values.branchId)
            .map((department) => ({ label: `${department.code} — ${department.name}`, value: department.id })),
          placeholder: "Sin departamento / selecciona un departamento",
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
        {
          name: "branchId",
          label: "Sucursal (opcional)",
          type: "select",
          options: (values) => branches.records
            .filter((branch) => branch.isActive && (!values.organizationId || branch.organizationId === values.organizationId))
            .map((branch) => ({ label: `${branch.code} — ${branch.name}`, value: branch.id })),
          clearFieldsOnChange: ["departmentId"],
          placeholder: "Sin sucursal / selecciona una sucursal",
        },
        {
          name: "departmentId",
          label: "Departamento (opcional)",
          type: "select",
          options: (values) => departments.records
            .filter((department) => department.isActive)
            .filter((department) => !values.organizationId || department.organizationId === values.organizationId)
            .filter((department) => !values.branchId || !department.branchId || department.branchId === values.branchId)
            .map((department) => ({ label: `${department.code} — ${department.name}`, value: department.id })),
          placeholder: "Sin departamento / selecciona un departamento",
        },
        { name: "firstName", label: "Nombre", required: true },
        { name: "lastName", label: "Apellido", required: true },
        { name: "email", label: "Correo", required: true },
      ]}
      mapCreate={(values) => ({
        organizationId: values.organizationId,
        branchId: values.branchId || null,
        departmentId: values.departmentId || null,
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        password: values.password,
        roleIds: values.roleIds ? values.roleIds.split(",").filter(Boolean) : [],
      })}
      mapUpdate={(values, row) => ({
        branchId: values.branchId || null,
        departmentId: values.departmentId || null,
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        organizationId: row.organizationId,
      })}
      rowActions={[
        { label: "Activar", endpoint: (row) => `/users/${row.id}/activate`, isVisible: (row) => row.isActive !== true },
        { label: "Desactivar", endpoint: (row) => `/users/${row.id}/deactivate`, isVisible: (row) => row.isActive === true },
        { label: "Bloquear", endpoint: (row) => `/users/${row.id}/lock`, isVisible: (row) => row.isLocked !== true },
        { label: "Desbloquear", endpoint: (row) => `/users/${row.id}/unlock`, isVisible: (row) => row.isLocked === true },
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
