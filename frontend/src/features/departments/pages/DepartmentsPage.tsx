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
  organizationName: string;
  code: string;
  name: string;
}

export function DepartmentsPage() {
  const organizations = useLookupOptions<OrganizationLookup>(
    "organizations",
    "/organizations",
    (item) => `${item.code} — ${item.name}`,
  );

  const branches = useLookupOptions<BranchLookup>(
    "branches",
    "/branches",
    (item) => `${item.organizationName} — ${item.code} — ${item.name}`,
  );

  return (
    <ResourceManager
      title="Departamentos"
      description="Administra unidades organizativas y su asociación opcional a sucursales. El código se genera automáticamente."
      endpoint="/departments"
      queryKey="departments"
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
        { name: "name", label: "Nombre", required: true },
        {
          name: "branchId",
          label: "Sucursal (opcional)",
          type: "select",
          options: (values) => branches.records
            .filter((branch) => !values.organizationId || branch.organizationId === values.organizationId)
            .map((branch) => ({ label: `${branch.code} — ${branch.name}`, value: branch.id })),
          placeholder: "Sin sucursal / selecciona una sucursal",
        },
      ]}
      updateFields={[
        { name: "name", label: "Nombre", required: true },
        {
          name: "branchId",
          label: "Sucursal (opcional)",
          type: "select",
          options: branches.options,
          placeholder: "Sin sucursal / selecciona una sucursal",
        },
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
        branchId: values.branchId || null,
        isActive: values.isActive === "true",
      })}
      allowDelete
    />
  );
}
