import { useQueryClient } from "@tanstack/react-query";
import { RefreshCw } from "lucide-react";
import { ResourceManager } from "../../../components/ResourceManager";
import { useLookupOptions } from "../../../hooks/useLookupOptions";

interface FrameworkLookup {
  id: string;
  code: string;
  name: string;
  version: string;
}

export function FrameworksPage() {
  const queryClient = useQueryClient();
  const frameworks = useLookupOptions<FrameworkLookup>(
    "frameworks",
    "/frameworks",
    (item) => `${item.code} — ${item.name} (${item.version})`,
  );

  const statusOptions = [
    { label: "Activo", value: "true" },
    { label: "Inactivo", value: "false" },
  ];

  async function refreshWorkspace() {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["frameworks"] }),
      queryClient.invalidateQueries({ queryKey: ["framework-controls"] }),
    ]);
  }

  return (
    <main className="module-page frameworks-page">
      <header className="module-header frameworks-page-header">
        <div>
          <p className="eyebrow">CUMPLIMIENTO</p>
          <h1>Marcos y controles</h1>
          <p>Administra marcos de referencia y controles para estructurar el cumplimiento.</p>
        </div>
        <div className="module-header-actions">
          <button type="button" className="secondary-button" onClick={refreshWorkspace}>
            <RefreshCw size={16} /> Actualizar
          </button>
        </div>
      </header>

      <section className="frameworks-workspace" aria-label="Gestión de marcos y controles">
        <ResourceManager
          title="Marcos de control"
          description="Configura marcos de referencia para la evaluación de controles."
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
            { name: "isActive", label: "Estado", type: "select", required: true, options: statusOptions },
          ]}
          mapUpdate={(values) => ({ ...values, isActive: values.isActive === "true" })}
        />

        <ResourceManager
          title="Controles"
          description="Define controles, dominios y ponderaciones dentro de los marcos."
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
            {
              name: "frameworkId",
              label: "Marco de control",
              type: "select",
              required: true,
              options: frameworks.options,
              placeholder: frameworks.isLoading ? "Cargando marcos..." : "Selecciona un marco",
            },
            { name: "code", label: "Código", required: true },
            { name: "title", label: "Título", required: true },
            { name: "domain", label: "Dominio", required: true },
            { name: "weight", label: "Peso", type: "number", min: 0, required: true, defaultValue: 1 },
            { name: "description", label: "Descripción", type: "textarea" },
          ]}
          updateFields={[
            { name: "code", label: "Código", required: true },
            { name: "title", label: "Título", required: true },
            { name: "domain", label: "Dominio", required: true },
            { name: "weight", label: "Peso", type: "number", min: 0, required: true },
            { name: "description", label: "Descripción", type: "textarea" },
            { name: "isActive", label: "Estado", type: "select", required: true, options: statusOptions },
          ]}
          mapUpdate={(values) => ({
            code: values.code,
            title: values.title,
            domain: values.domain,
            weight: Number(values.weight),
            description: values.description || null,
            isActive: values.isActive === "true",
          })}
        />
      </section>
    </main>
  );
}
