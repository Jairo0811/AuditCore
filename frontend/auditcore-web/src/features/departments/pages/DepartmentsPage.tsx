import { ResourceManager } from "../../../components/ResourceManager";

export function DepartmentsPage() {
  return (
    <ResourceManager
      title="Departamentos"
      description="Administra unidades organizativas y su asociación opcional a sucursales."
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
        { name: "organizationId", label: "ID de organización", required: true },
        { name: "name", label: "Nombre", required: true },
        { name: "code", label: "Código", required: true },
        { name: "branchId", label: "ID de sucursal" },
      ]}
      updateFields={[
        { name: "name", label: "Nombre", required: true },
        { name: "code", label: "Código", required: true },
        { name: "branchId", label: "ID de sucursal" },
        { name: "isActive", label: "Activo (true/false)", required: true },
      ]}
      mapUpdate={(values) => ({ ...values, branchId: values.branchId || null, isActive: values.isActive === "true" || values.isActive === "1" })}
      allowDelete
    />
  );
}
