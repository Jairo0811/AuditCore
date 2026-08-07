import { ResourceManager } from "../../../components/ResourceManager";

export function OrganizationsPage() {
  return (
    <ResourceManager
      title="Organizaciones"
      description="Administra las organizaciones disponibles en el entorno multiempresa."
      endpoint="/organizations"
      queryKey="organizations"
      columns={[
        { key: "code", label: "Código" },
        { key: "name", label: "Nombre" },
        { key: "description", label: "Descripción" },
        { key: "isActive", label: "Activa" },
      ]}
      createFields={[
        { name: "name", label: "Nombre", required: true },
        { name: "code", label: "Código", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
      ]}
      updateFields={[
        { name: "name", label: "Nombre", required: true },
        { name: "code", label: "Código", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        { name: "isActive", label: "Activa (true/false)", required: true },
      ]}
      mapUpdate={(values) => ({
        name: values.name,
        code: values.code,
        description: values.description || null,
        isActive: values.isActive === "true" || values.isActive === "1",
      })}
      allowDelete
    />
  );
}
