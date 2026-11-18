import { ResourceManager } from "../../../components/ResourceManager";
import { hasPermission, isSuperAdmin, Permissions } from "../../auth/permissions";

const activeOptions = [
  { label: "Activa", value: "true" },
  { label: "Inactiva", value: "false" },
];

export function OrganizationsPage() {
  const canManage = hasPermission(Permissions.OrganizationsManage);
  // El backend reserva crear y eliminar organizaciones al SUPER_ADMIN.
  const canAdministerTenants = canManage && isSuperAdmin();

  return (
    <ResourceManager
      title="Organizaciones"
      description="Administra las organizaciones disponibles en el entorno multiempresa. El código se genera automáticamente al crear cada organización."
      endpoint="/organizations"
      queryKey="organizations"
      canCreate={canAdministerTenants}
      canUpdate={() => canManage}
      canDelete={() => canAdministerTenants}
      getRowLabel={(row) => String(row.name ?? row.code ?? "organización")}
      emptyMessage="No hay organizaciones registradas."
      columns={[
        { key: "code", label: "Código" },
        { key: "name", label: "Nombre" },
        { key: "description", label: "Descripción" },
        { key: "isActive", label: "Activa" },
      ]}
      createFields={[
        { name: "name", label: "Nombre", required: true, maxLength: 200 },
        { name: "description", label: "Descripción", type: "textarea" },
      ]}
      updateFields={[
        { name: "name", label: "Nombre", required: true, maxLength: 200 },
        { name: "description", label: "Descripción", type: "textarea" },
        { name: "isActive", label: "Estado", type: "select", required: true, options: activeOptions },
      ]}
      mapCreate={(values) => ({ name: values.name, description: values.description || null })}
      mapUpdate={(values) => ({
        name: values.name,
        description: values.description || null,
        isActive: values.isActive === "true",
      })}
      deleteConfirm={(row) => ({
        title: "Eliminar organización",
        message: `¿Eliminar la organización «${String(row.name)}»? Solo es posible si no tiene registros relacionados.`,
        confirmLabel: "Eliminar",
        tone: "danger",
      })}
    />
  );
}
