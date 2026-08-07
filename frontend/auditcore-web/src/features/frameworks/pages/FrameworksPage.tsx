import { ResourceManager } from "../../../components/ResourceManager";

export function FrameworksPage() {
  return (
    <div>
      <ResourceManager
        title="Marcos de control"
        description="Configura marcos de referencia para estructurar la evaluación de controles."
        endpoint="/frameworks"
        queryKey="frameworks"
        columns={[
          { key: "code", label: "Código" },
          { key: "name", label: "Nombre" },
          { key: "version", label: "Versión" },
          { key: "description", label: "Descripción" },
          { key: "isActive", label: "Activo" },
        ]}
        createFields={[
          { name: "name", label: "Nombre", required: true },
          { name: "code", label: "Código", required: true },
          { name: "version", label: "Versión", required: true },
          { name: "description", label: "Descripción", type: "textarea" },
        ]}
        updateFields={[
          { name: "name", label: "Nombre", required: true },
          { name: "code", label: "Código", required: true },
          { name: "version", label: "Versión", required: true },
          { name: "description", label: "Descripción", type: "textarea" },
          { name: "isActive", label: "Activo (true/false)", required: true },
        ]}
        mapUpdate={(values) => ({ ...values, isActive: values.isActive === "true" || values.isActive === "1" })}
      />

      <ResourceManager
        title="Controles"
        description="Define controles, dominios y ponderaciones dentro de los marcos configurados."
        endpoint="/frameworks/controls"
        queryKey="framework-controls"
        columns={[
          { key: "code", label: "Código" },
          { key: "title", label: "Control" },
          { key: "domain", label: "Dominio" },
          { key: "weight", label: "Peso" },
          { key: "isActive", label: "Activo" },
        ]}
        createFields={[
          { name: "frameworkId", label: "ID de marco", required: true },
          { name: "code", label: "Código", required: true },
          { name: "title", label: "Título", required: true },
          { name: "domain", label: "Dominio", required: true },
          { name: "weight", label: "Peso", type: "number", required: true, defaultValue: 1 },
          { name: "description", label: "Descripción", type: "textarea" },
        ]}
        updateFields={[
          { name: "code", label: "Código", required: true },
          { name: "title", label: "Título", required: true },
          { name: "domain", label: "Dominio", required: true },
          { name: "weight", label: "Peso", type: "number", required: true },
          { name: "description", label: "Descripción", type: "textarea" },
          { name: "isActive", label: "Activo (true/false)", required: true },
        ]}
        mapUpdate={(values) => ({
          code: values.code,
          title: values.title,
          domain: values.domain,
          weight: Number(values.weight),
          description: values.description || null,
          isActive: values.isActive === "true" || values.isActive === "1",
        })}
      />
    </div>
  );
}
