import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Plus, RefreshCw, Trash2, X } from "lucide-react";
import { useMemo, useState } from "react";
import { apiClient } from "../services/apiClient";

export type ResourceRecord = Record<string, unknown> & { id?: string };

export interface ResourceField {
  name: string;
  label: string;
  type?: "text" | "number" | "textarea" | "datetime-local" | "select";
  required?: boolean;
  options?: Array<{ label: string; value: string | number }>;
  defaultValue?: string | number;
}

export interface ResourceColumn {
  key: string;
  label: string;
  render?: (value: unknown, row: ResourceRecord) => React.ReactNode;
}

export interface ResourceRowAction {
  label: string;
  endpoint: (row: ResourceRecord) => string;
  method?: "post" | "put" | "delete";
  body?: (row: ResourceRecord) => unknown;
  confirm?: string;
}

interface ResourceManagerProps {
  title: string;
  description: string;
  endpoint: string;
  queryKey: string;
  columns: ResourceColumn[];
  createFields?: ResourceField[];
  updateFields?: ResourceField[];
  mapCreate?: (values: Record<string, string>) => unknown;
  mapUpdate?: (values: Record<string, string>, row: ResourceRecord) => unknown;
  allowDelete?: boolean;
  rowActions?: ResourceRowAction[];
}

interface ProblemDetails {
  title?: string;
  detail?: string;
}

function normalizeValue(field: ResourceField, value: string) {
  if (field.type === "number") return value === "" ? null : Number(value);
  if (field.type === "datetime-local") return value ? new Date(value).toISOString() : null;
  return value === "" ? null : value;
}

function formatCell(value: unknown) {
  if (value === null || value === undefined || value === "") return "—";
  if (typeof value === "boolean") return value ? "Sí" : "No";
  if (typeof value === "object") return JSON.stringify(value);
  return String(value);
}

function getApiErrorMessage(error: unknown, fallback: string) {
  if (typeof error !== "object" || error === null) return fallback;

  const response = (error as { response?: { data?: unknown } }).response;
  if (!response) return fallback;

  if (typeof response.data === "string" && response.data.trim()) {
    return response.data;
  }

  const problem = response.data as ProblemDetails | undefined;
  return problem?.detail || problem?.title || fallback;
}

export function ResourceManager({
  title,
  description,
  endpoint,
  queryKey,
  columns,
  createFields = [],
  updateFields = createFields,
  mapCreate,
  mapUpdate,
  allowDelete = false,
  rowActions = [],
}: ResourceManagerProps) {
  const queryClient = useQueryClient();
  const [mode, setMode] = useState<"closed" | "create" | "edit">("closed");
  const [selected, setSelected] = useState<ResourceRecord | null>(null);
  const [values, setValues] = useState<Record<string, string>>({});
  const [error, setError] = useState<string | null>(null);
  const [pageError, setPageError] = useState<string | null>(null);

  const resourceQuery = useQuery({
    queryKey: [queryKey],
    queryFn: async () => {
      const { data } = await apiClient.get<ResourceRecord[]>(endpoint);
      return data;
    },
  });

  const activeFields = mode === "edit" ? updateFields : createFields;

  const initialCreateValues = useMemo(
    () => Object.fromEntries(createFields.map((field) => [field.name, String(field.defaultValue ?? "")])),
    [createFields],
  );

  const saveMutation = useMutation({
    mutationFn: async () => {
      setError(null);
      setPageError(null);
      const fields = mode === "edit" ? updateFields : createFields;
      const normalized = Object.fromEntries(
        fields.map((field) => [field.name, normalizeValue(field, values[field.name] ?? "")]),
      );

      if (mode === "edit" && selected?.id) {
        const payload = mapUpdate ? mapUpdate(values, selected) : normalized;
        await apiClient.put(`${endpoint}/${selected.id}`, payload);
      } else {
        const payload = mapCreate ? mapCreate(values) : normalized;
        await apiClient.post(endpoint, payload);
      }
    },
    onSuccess: async () => {
      setMode("closed");
      setSelected(null);
      setValues(initialCreateValues);
      await queryClient.invalidateQueries({ queryKey: [queryKey] });
    },
    onError: (mutationError) =>
      setError(getApiErrorMessage(mutationError, "No fue posible guardar los cambios. Revisa los datos y permisos.")),
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => {
      setPageError(null);
      await apiClient.delete(`${endpoint}/${id}`);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [queryKey] });
    },
    onError: (mutationError) =>
      setPageError(getApiErrorMessage(mutationError, "No fue posible eliminar el registro.")),
  });

  const actionMutation = useMutation({
    mutationFn: async ({ row, action }: { row: ResourceRecord; action: ResourceRowAction }) => {
      if (action.confirm && !window.confirm(action.confirm)) return;
      setPageError(null);
      const method = action.method ?? "put";
      await apiClient.request({
        url: action.endpoint(row),
        method,
        data: action.body?.(row),
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [queryKey] });
    },
    onError: (mutationError) =>
      setPageError(getApiErrorMessage(mutationError, "No fue posible completar la acción solicitada.")),
  });

  function openCreate() {
    setSelected(null);
    setValues(initialCreateValues);
    setMode("create");
    setError(null);
  }

  function openEdit(row: ResourceRecord) {
    setSelected(row);
    setValues(
      Object.fromEntries(
        updateFields.map((field) => {
          const raw = row[field.name];
          if (field.type === "datetime-local" && typeof raw === "string") return [field.name, raw.slice(0, 16)];
          return [field.name, raw === null || raw === undefined ? "" : String(raw)];
        }),
      ),
    );
    setMode("edit");
    setError(null);
  }

  return (
    <main className="module-page">
      <header className="module-header">
        <div>
          <p className="eyebrow">GESTIÓN</p>
          <h1>{title}</h1>
          <p>{description}</p>
        </div>
        <div className="module-header-actions">
          <button type="button" className="secondary-button" onClick={() => resourceQuery.refetch()}>
            <RefreshCw size={16} /> Actualizar
          </button>
          {createFields.length > 0 && (
            <button type="button" onClick={openCreate}>
              <Plus size={16} /> Nuevo
            </button>
          )}
        </div>
      </header>

      {pageError && (
        <div className="panel-state error-message" role="alert">
          {pageError}
        </div>
      )}

      {resourceQuery.isLoading && <div className="panel-state">Cargando información...</div>}
      {resourceQuery.isError && <div className="panel-state error-message">No fue posible cargar el módulo.</div>}

      {!resourceQuery.isLoading && !resourceQuery.isError && (
        <section className="data-panel">
          <div className="table-scroll">
            <table className="data-table">
              <thead>
                <tr>
                  {columns.map((column) => <th key={column.key}>{column.label}</th>)}
                  {(updateFields.length > 0 || allowDelete || rowActions.length > 0) && <th>Acciones</th>}
                </tr>
              </thead>
              <tbody>
                {(resourceQuery.data ?? []).map((row, index) => (
                  <tr key={row.id ?? index}>
                    {columns.map((column) => (
                      <td key={column.key}>{column.render ? column.render(row[column.key], row) : formatCell(row[column.key])}</td>
                    ))}
                    {(updateFields.length > 0 || allowDelete || rowActions.length > 0) && (
                      <td>
                        <div className="row-actions">
                          {updateFields.length > 0 && row.id && (
                            <button type="button" className="icon-button" title="Editar" onClick={() => openEdit(row)}>
                              <Pencil size={15} />
                            </button>
                          )}
                          {rowActions.map((action) => (
                            <button
                              key={action.label}
                              type="button"
                              className="text-action"
                              disabled={actionMutation.isPending}
                              onClick={() => actionMutation.mutate({ row, action })}
                            >
                              {action.label}
                            </button>
                          ))}
                          {allowDelete && row.id && (
                            <button
                              type="button"
                              className="icon-button danger-button"
                              title="Eliminar"
                              disabled={deleteMutation.isPending}
                              onClick={() => window.confirm("¿Eliminar este registro? Esta acción no se puede deshacer.") && deleteMutation.mutate(row.id!)}
                            >
                              <Trash2 size={15} />
                            </button>
                          )}
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {(resourceQuery.data?.length ?? 0) === 0 && <div className="empty-table">No hay registros para mostrar.</div>}
        </section>
      )}

      {mode !== "closed" && (
        <div className="modal-backdrop" role="presentation">
          <section className="modal-card" role="dialog" aria-modal="true" aria-label={`${mode === "edit" ? "Editar" : "Crear"} ${title}`}>
            <header>
              <div>
                <p className="eyebrow">{mode === "edit" ? "EDITAR" : "NUEVO REGISTRO"}</p>
                <h2>{title}</h2>
              </div>
              <button type="button" className="icon-button" onClick={() => setMode("closed")}><X size={18} /></button>
            </header>

            <form
              className="resource-form"
              onSubmit={(event) => {
                event.preventDefault();
                saveMutation.mutate();
              }}
            >
              {activeFields.map((field) => (
                <label key={field.name}>
                  {field.label}
                  {field.type === "textarea" ? (
                    <textarea
                      required={field.required}
                      value={values[field.name] ?? ""}
                      onChange={(event) => setValues((current) => ({ ...current, [field.name]: event.target.value }))}
                    />
                  ) : field.type === "select" ? (
                    <select
                      required={field.required}
                      value={values[field.name] ?? ""}
                      onChange={(event) => setValues((current) => ({ ...current, [field.name]: event.target.value }))}
                    >
                      <option value="">Selecciona una opción</option>
                      {field.options?.map((option) => <option key={String(option.value)} value={option.value}>{option.label}</option>)}
                    </select>
                  ) : (
                    <input
                      type={field.type ?? "text"}
                      required={field.required}
                      value={values[field.name] ?? ""}
                      onChange={(event) => setValues((current) => ({ ...current, [field.name]: event.target.value }))}
                    />
                  )}
                </label>
              ))}

              {error && <div className="form-error">{error}</div>}

              <div className="form-actions">
                <button type="button" className="secondary-button" onClick={() => setMode("closed")}>Cancelar</button>
                <button type="submit" disabled={saveMutation.isPending}>{saveMutation.isPending ? "Guardando..." : "Guardar"}</button>
              </div>
            </form>
          </section>
        </div>
      )}
    </main>
  );
}
