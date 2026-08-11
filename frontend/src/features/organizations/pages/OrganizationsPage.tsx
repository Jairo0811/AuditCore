import { ResourceManager } from "../../../components/ResourceManager";

export function OrganizationsPage() {
  return (
    <ResourceManager
      title="Organizaciones"
      description="Administra las organizaciones disponibles en el entorno multiempresa. El código se genera automáticamente al crear cada organización."
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
        { name: "description", label: "Descripción", type: "textarea" },
      ]}
      updateFields={[
        { name: "name", label: "Nombre", required: true },
        { name: "description", label: "Descripción", type: "textarea" },
        {
          name: "isActive",
          label: "Estado",
          type: "select",
          required: true,
          options: [
            { label: "Activa", value: "true" },
            { label: "Inactiva", value: "false" },
          ],
        },
      ]}
      mapUpdate={(values) => ({
        name: values.name,
        description: values.description || null,
        isActive: values.isActive === "true",
      })}
      allowDelete
    />
  );
}
