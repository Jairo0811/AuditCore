import { ResourceManager } from "../../../components/ResourceManager";

export function BranchesPage() {
  return (
    <ResourceManager
      title="Sucursales"
      description="Administra las sedes y ubicaciones operativas de cada organización."
      endpoint="/branches"
      queryKey="branches"
      columns={[
        { key: "code", label: "Código" },
        { key: "name", label: "Nombre" },
        { key: "organizationName", label: "Organización" },
        { key: "address", label: "Dirección" },
        { key: "isActive", label: "Activa" },
      ]}
      createFields={[
        { name: "organizationId", label: "ID de organización", required: true },
        { name: "name", label: "Nombre", required: true },
        { name: "code", label: "Código", required: true },
        { name: "address", label: "Dirección", type: "textarea" },
      ]}
      updateFields={[
        { name: "name", label: "Nombre", required: true },
        { name: "code", label: "Código", required: true },
        { name: "address", label: "Dirección", type: "textarea" },
        { name: "isActive", label: "Activa (true/false)", required: true },
      ]}
      mapUpdate={(values) => ({ ...values, address: values.address || null, isActive: values.isActive === "true" || values.isActive === "1" })}
      allowDelete
    />
  );
}
