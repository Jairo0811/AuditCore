import { ResourceManager, type FormValues, type ResourceField, type ResourceRecord } from "../../../components/ResourceManager";
import { hasPermission, isSuperAdmin, Permissions, Roles } from "../../auth/permissions";
import { getSession } from "../../auth/session";
import { useLookupOptions, useOrganizationLookup } from "../../../hooks/useLookupOptions";
import { codesToIds } from "../../../lib/codeMapping";
import { splitIds } from "../../../lib/formatters";

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
  isActive: boolean;
}

// Refleja PasswordPolicy del backend (fuente de verdad): 10-128 caracteres, al menos una letra y un número.
const PASSWORD_HELP = "Entre 10 y 128 caracteres, con al menos una letra y un número.";

const hasRole = (row: ResourceRecord, role: string) => Array.isArray(row.roles) && row.roles.includes(role);

export function UsersPage() {
  const organizations = useOrganizationLookup();
  const branches = useLookupOptions<BranchLookup>("branches", "/branches", (item) => `${item.code} — ${item.name}`, [Permissions.OrganizationsView]);
  const departments = useLookupOptions<DepartmentLookup>("departments", "/departments", (item) => `${item.code} — ${item.name}`, [Permissions.OrganizationsView]);
  const roles = useLookupOptions<RoleLookup>("roles", "/roles", (item) => `${item.code} — ${item.name}`, [Permissions.RolesView]);

  const canManage = hasPermission(Permissions.UsersManage);
  const actorIsSuperAdmin = isSuperAdmin();
  // El backend impide que un actor restringido a su organización modifique cuentas SUPER_ADMIN.
  const canModify = (row: ResourceRecord) => canManage && (actorIsSuperAdmin || !hasRole(row, Roles.SuperAdmin));

  // Solo un SUPER_ADMIN puede asignar SUPER_ADMIN; los roles inactivos no se ofrecen.
  const assignableRoles = roles.records.filter((role) => role.isActive && (actorIsSuperAdmin || role.code !== Roles.SuperAdmin));
  const roleOptions = assignableRoles.map((role) => ({ label: `${role.code} — ${role.name}`, value: role.id }));

  const branchOptions = (values: FormValues) => branches.records
    .filter((branch) => branch.isActive && (!values.organizationId || branch.organizationId === values.organizationId))
    .map((branch) => ({ label: `${branch.code} — ${branch.name}`, value: branch.id }));

  const departmentOptions = (values: FormValues) => departments.records
    .filter((department) => department.isActive)
    .filter((department) => !values.organizationId || department.organizationId === values.organizationId)
    .filter((department) => !values.branchId || !department.branchId || department.branchId === values.branchId)
    .map((department) => ({ label: `${department.code} — ${department.name}`, value: department.id }));

  const placementFields: ResourceField[] = branches.allowed
    ? [
        { name: "branchId", label: "Sucursal (opcional)", type: "select", options: branchOptions, clearFieldsOnChange: ["departmentId"], placeholder: "Sin sucursal / selecciona una sucursal" },
        { name: "departmentId", label: "Departamento (opcional)", type: "select", options: departmentOptions, placeholder: "Sin departamento / selecciona un departamento" },
      ]
    : [];

  const identityFields: ResourceField[] = [
    { name: "firstName", label: "Nombre", required: true, maxLength: 100, autoComplete: "off" },
    { name: "lastName", label: "Apellido", required: true, maxLength: 100, autoComplete: "off" },
    { name: "email", label: "Correo", type: "email", required: true, maxLength: 320, autoComplete: "off" },
  ];

  return (
    <ResourceManager
      title="Usuarios"
      description="Gestiona usuarios, acceso, roles y pertenencia a organización, sucursal y departamento."
      endpoint="/users"
      queryKey="users"
      canCreate={canManage}
      canUpdate={canModify}
      getRowLabel={(row) => String(row.fullName ?? row.email ?? "usuario")}
      emptyMessage="No hay usuarios registrados."
      columns={[
        { key: "fullName", label: "Nombre" },
        { key: "email", label: "Correo" },
        { key: "organizationName", label: "Organización" },
        { key: "branchName", label: "Sucursal" },
        { key: "departmentName", label: "Departamento" },
        { key: "roles", label: "Roles" },
        {
          key: "isActive",
          label: "Estado",
          render: (_value, row) => [row.isActive === true ? "Activo" : "Inactivo", row.isLocked === true ? "Bloqueado" : null]
            .filter(Boolean)
            .join(" · "),
        },
      ]}
      createFields={[
        ...(organizations.allowed
          ? [{
              name: "organizationId",
              label: "Organización",
              type: "select",
              required: true,
              options: organizations.options,
              clearFieldsOnChange: ["branchId", "departmentId"],
              placeholder: organizations.isLoading ? "Cargando organizaciones..." : "Selecciona una organización",
            } satisfies ResourceField]
          : []),
        ...placementFields,
        ...identityFields,
        {
          name: "password",
          label: "Contraseña",
          type: "password",
          required: true,
          minLength: 10,
          maxLength: 128,
          autoComplete: "new-password",
          helpText: PASSWORD_HELP,
        },
        ...(roles.allowed
          ? [{ name: "roleIds", label: "Roles", type: "multiselect", options: roleOptions, placeholder: "No hay roles disponibles." } satisfies ResourceField]
          : []),
      ]}
      mapCreate={(values) => ({
        organizationId: values.organizationId || getSession()?.organizationId,
        branchId: values.branchId || null,
        departmentId: values.departmentId || null,
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        password: values.password,
        roleIds: splitIds(values.roleIds),
      })}
      updateFields={[...placementFields, ...identityFields]}
      mapUpdate={(values) => ({
        branchId: values.branchId || null,
        departmentId: values.departmentId || null,
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
      })}
      rowActions={[
        {
          label: "Roles",
          permission: Permissions.UsersManage,
          endpoint: (row) => `/users/${row.id}/roles`,
          isVisible: (row) => canModify(row) && roles.isSuccess && codesToIds(row.roles, roles.records) !== null,
          description: "Selecciona uno o varios roles. Al guardar, la selección reemplaza los roles actuales del usuario.",
          fields: [{ name: "roleIds", label: "Roles asignados", type: "multiselect", options: roleOptions }],
          getInitialValues: (row) => ({ roleIds: (codesToIds(row.roles, roles.records) ?? []).join(",") }),
          submitLabel: "Guardar roles",
          successMessage: "Roles del usuario actualizados.",
          body: (_row, values) => ({ roleIds: splitIds(values.roleIds) }),
        },
        {
          label: "Activar",
          permission: Permissions.UsersManage,
          endpoint: (row) => `/users/${row.id}/activate`,
          isVisible: (row) => canModify(row) && row.isActive !== true,
          successMessage: "Usuario activado.",
        },
        {
          label: "Desactivar",
          permission: Permissions.UsersManage,
          endpoint: (row) => `/users/${row.id}/deactivate`,
          isVisible: (row) => canModify(row) && row.isActive === true,
          successMessage: "Usuario desactivado.",
          confirm: (row) => ({
            title: "Desactivar usuario",
            message: `¿Desactivar a «${String(row.fullName)}»? No podrá iniciar sesión hasta que se reactive.`,
            confirmLabel: "Desactivar",
            tone: "danger",
          }),
        },
        {
          label: "Bloquear",
          permission: Permissions.UsersManage,
          endpoint: (row) => `/users/${row.id}/lock`,
          isVisible: (row) => canModify(row) && row.isLocked !== true,
          successMessage: "Usuario bloqueado.",
          confirm: (row) => ({
            title: "Bloquear usuario",
            message: `¿Bloquear a «${String(row.fullName)}»? Sus sesiones no podrán renovarse.`,
            confirmLabel: "Bloquear",
            tone: "danger",
          }),
        },
        {
          label: "Desbloquear",
          permission: Permissions.UsersManage,
          endpoint: (row) => `/users/${row.id}/unlock`,
          isVisible: (row) => canModify(row) && row.isLocked === true,
          successMessage: "Usuario desbloqueado.",
        },
        {
          label: "Contraseña",
          permission: Permissions.UsersManage,
          endpoint: (row) => `/users/${row.id}/password`,
          isVisible: canModify,
          fields: [
            {
              name: "password",
              label: "Nueva contraseña",
              type: "password",
              required: true,
              minLength: 10,
              maxLength: 128,
              autoComplete: "new-password",
              helpText: PASSWORD_HELP,
            },
          ],
          submitLabel: "Cambiar contraseña",
          successMessage: "Contraseña actualizada.",
          body: (_row, values) => ({ password: values.password }),
        },
      ]}
    />
  );
}
