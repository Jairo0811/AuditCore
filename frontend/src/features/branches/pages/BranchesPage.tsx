import { ResourceManager } from "../../../components/ResourceManager";
import { useLookupOptions } from "../../../hooks/useLookupOptions";

interface OrganizationLookup {
  id: string;
  code: string;
  name: string;
}

export function BranchesPage() {
  const organizations = useLookupOptions<OrganizationLookup>(
    "organizations",
    "/organizations",
    (item) => `${item.code} — ${item.name}`,
  );

  return (
    <ResourceManager
      title="Sucursales"
      description="Administra las sedes y ubicaciones operativas de cada organización. El código se genera automáticamente."
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
        {
          name: "organizationId",
          label: "Organización",
          type: "select",
          required: true,
          options: organizations.options,
          placeholder: organizations.isLoading ? "Cargando organizaciones..." : "Selecciona una organización",
        },
        { name: "name", label: "Nombre", required: true },
        { name: "address", label: "Dirección", type: "textarea" },
      ]}
      updateFields={[
        { name: "name", label: "Nombre", required: true },
        { name: "address", label: "Dirección", type: "textarea" },
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
        address: values.address || null,
        isActive: values.isActive === "true",
      })}
      allowDelete
    />
  );
}
