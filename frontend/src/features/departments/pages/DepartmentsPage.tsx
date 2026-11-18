import { ResourceManager, type FormValues } from "../../../components/ResourceManager";
import { hasPermission, Permissions } from "../../auth/permissions";
import { useLookupOptions, useOrganizationLookup } from "../../../hooks/useLookupOptions";

interface BranchLookup {
  id: string;
  organizationId: string;
  organizationName: string;
  code: string;
  name: string;
}

const activeOptions = [
  { label: "Activo", value: "true" },
  { label: "Inactivo", value: "false" },
];

export function DepartmentsPage() {
  const organizations = useOrganizationLookup();
  const branches = useLookupOptions<BranchLookup>(
    "branches",
    "/branches",
    (item) => `${item.organizationName} — ${item.code} — ${item.name}`,
    [Permissions.OrganizationsView],
  );
  const canManage = hasPermission(Permissions.OrganizationsManage);

  const branchOptions = (values: FormValues) => branches.records
    .filter((branch) => !values.organizationId || branch.organizationId === values.organizationId)
    .map((branch) => ({ label: `${branch.code} — ${branch.name}`, value: branch.id }));

  return (
    <ResourceManager
      title="Departamentos"
      description="Administra unidades organizativas y su asociación opcional a sucursales. El código se genera automáticamente."
      endpoint="/departments"
      queryKey="departments"
      canCreate={canManage}
      canUpdate={() => canManage}
      canDelete={() => canManage}
      getRowLabel={(row) => String(row.name ?? row.code ?? "departamento")}
      emptyMessage="No hay departamentos registrados."
      columns={[
        { key: "code", label: "Código" },
        { key: "name", label: "Nombre" },
        { key: "organizationName", label: "Organización" },
        { key: "branchName", label: "Sucursal" },
        { key: "isActive", label: "Activo" },
      ]}
      createFields={[
        {
          name: "organizationId",
          label: "Organización",
          type: "select",
          required: true,
          options: organizations.options,
          clearFieldsOnChange: ["branchId"],
          placeholder: organizations.isLoading ? "Cargando organizaciones..." : "Selecciona una organización",
        },
        { name: "name", label: "Nombre", required: true, maxLength: 200 },
        {
          name: "branchId",
          label: "Sucursal (opcional)",
          type: "select",
          options: branchOptions,
          placeholder: "Sin sucursal / selecciona una sucursal",
        },
      ]}
      mapCreate={(values) => ({
        organizationId: values.organizationId,
        name: values.name,
        branchId: values.branchId || null,
      })}
      updateFields={[
        { name: "name", label: "Nombre", required: true, maxLength: 200 },
        {
          name: "branchId",
          label: "Sucursal (opcional)",
          type: "select",
          options: branchOptions,
          placeholder: "Sin sucursal / selecciona una sucursal",
        },
        { name: "isActive", label: "Estado", type: "select", required: true, options: activeOptions },
      ]}
      mapUpdate={(values) => ({
        name: values.name,
        branchId: values.branchId || null,
        isActive: values.isActive === "true",
      })}
      deleteConfirm={(row) => ({
        title: "Eliminar departamento",
        message: `¿Eliminar el departamento «${String(row.name)}»? No es posible si tiene usuarios asignados.`,
        confirmLabel: "Eliminar",
        tone: "danger",
      })}
    />
  );
}
