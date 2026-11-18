import { ResourceManager } from "../../../components/ResourceManager";
import { hasPermission, Permissions } from "../../auth/permissions";
import { useOrganizationLookup } from "../../../hooks/useLookupOptions";

const activeOptions = [
  { label: "Activa", value: "true" },
  { label: "Inactiva", value: "false" },
];

export function BranchesPage() {
  const organizations = useOrganizationLookup();
  const canManage = hasPermission(Permissions.OrganizationsManage);

  return (
    <ResourceManager
      title="Sucursales"
      description="Administra las sedes y ubicaciones operativas de cada organización. El código se genera automáticamente."
      endpoint="/branches"
      queryKey="branches"
      canCreate={canManage}
      canUpdate={() => canManage}
      canDelete={() => canManage}
      getRowLabel={(row) => String(row.name ?? row.code ?? "sucursal")}
      emptyMessage="No hay sucursales registradas."
      columns={[
        { key: "code", label: "Código" },
        { key: "name", label: "Nombre" },
        { key: "organizationName", label: "Organización" },
        { key: "address", label: "Dirección" },
        { key: "isActive", label: "Activa" },
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
        { name: "name", label: "Nombre", required: true, maxLength: 200 },
        { name: "address", label: "Dirección", type: "textarea" },
      ]}
      mapCreate={(values) => ({
        organizationId: values.organizationId,
        name: values.name,
        address: values.address || null,
      })}
      updateFields={[
        { name: "name", label: "Nombre", required: true, maxLength: 200 },
        { name: "address", label: "Dirección", type: "textarea" },
        { name: "isActive", label: "Estado", type: "select", required: true, options: activeOptions },
      ]}
      mapUpdate={(values) => ({
        name: values.name,
        address: values.address || null,
        isActive: values.isActive === "true",
      })}
      deleteConfirm={(row) => ({
        title: "Eliminar sucursal",
        message: `¿Eliminar la sucursal «${String(row.name)}»? No es posible si tiene departamentos o usuarios asignados.`,
        confirmLabel: "Eliminar",
        tone: "danger",
      })}
    />
  );
}
