import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Plus, RefreshCw, Trash2, X } from "lucide-react";
import { useMemo, useState } from "react";
import { apiClient } from "../services/apiClient";

export type ResourceRecord = Record<string, unknown> & { id?: string };

export interface ResourceOption {
  label: string;
  value: string | number;
}

export interface ResourceField {
  name: string;
  label: string;
  type?: "text" | "number" | "textarea" | "datetime-local" | "select" | "multiselect" | "password";
  required?: boolean;
  options?: ResourceOption[] | ((values: Record<string, string>) => ResourceOption[]);
  defaultValue?: string | number;
  placeholder?: string;
  min?: number;
  max?: number;
  clearFieldsOnChange?: string[];
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
  body?: (row: ResourceRecord, values: Record<string, string>) => unknown;
  confirm?: string;
  fields?: ResourceField[];
  submitLabel?: string;
  isVisible?: (row: ResourceRecord) => boolean;
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
  canEdit?: (row: ResourceRecord) => boolean;
}

interface ProblemDetails {
  title?: string;
  detail?: string;
}

function normalizeValue(field: ResourceField, value: string) {
  if (field.type === "number") return value === "" ? null : Number(value);
  if (field.type === "datetime-local") return value ? new Date(value).toISOString() : null;
  if (field.type === "multiselect") return value ? value.split(",").filter(Boolean) : [];
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
  if (!response) return error instanceof Error && error.message ? error.message : fallback;

  if (typeof response.data === "string" && response.data.trim()) return response.data;

  const problem = response.data as ProblemDetails | undefined;
  return problem?.detail || problem?.title || fallback;
}

function initialValues(fields: ResourceField[]) {
  return Object.fromEntries(fields.map((field) => [field.name, String(field.defaultValue ?? "")]));
}

function resolveOptions(field: ResourceField, values: Record<string, string>) {
  if (!field.options) return [];
  return typeof field.options === "function" ? field.options(values) : field.options;
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
  canEdit = () => true,
}: ResourceManagerProps) {
  const queryClient = useQueryClient();
  const [mode, setMode] = useState<"closed" | "create" | "edit">("closed");
  const [selected, setSelected] = useState<ResourceRecord | null>(null);
  const [values, setValues] = useState<Record<string, string>>({});
  const [error, setError] = useState<string | null>(null);
  const [pageError, setPageError] = useState<string | null>(null);
  const [activeAction, setActiveAction] = useState<ResourceRowAction | null>(null);
  const [actionValues, setActionValues] = useState<Record<string, string>>({});
  const [actionError, setActionError] = useState<string | null>(null);

  const resourceQuery = useQuery({
    queryKey: [queryKey],
    queryFn: async () => {
      const { data } = await apiClient.get<ResourceRecord[]>(endpoint);
      return data;
    },
  });

  const activeFields = mode === "edit" ? updateFields : createFields;
  const initialCreateValues = useMemo(() => initialValues(createFields), [createFields]);

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
    mutationFn: async ({ row, action, submittedValues }: {
      row: ResourceRecord;
      action: ResourceRowAction;
      submittedValues: Record<string, string>;
    }) => {
      if (action.confirm && !window.confirm(action.confirm)) return;
      setPageError(null);
      setActionError(null);
      const method = action.method ?? "put";
      await apiClient.request({
        url: action.endpoint(row),
        method,
        data: action.body?.(row, submittedValues),
      });
    },
    onSuccess: async () => {
      setActiveAction(null);
      setActionValues({});
      setSelected(null);
      await queryClient.invalidateQueries({ queryKey: [queryKey] });
    },
    onError: (mutationError) => {
      const message = getApiErrorMessage(mutationError, "No fue posible completar la acción solicitada.");
      if (activeAction) setActionError(message);
      else setPageError(message);
    },
  });

  function updateFieldValue(
    field: ResourceField,
    value: string,
    setter: React.Dispatch<React.SetStateAction<Record<string, string>>>,
  ) {
    setter((current) => {
      const next = { ...current, [field.name]: value };
      for (const dependentField of field.clearFieldsOnChange ?? []) next[dependentField] = "";
      return next;
    });
  }

  function openCreate() {
    setSelected(null);
    setValues(initialCreateValues);
    setMode("create");
    setError(null);
    setPageError(null);
  }

  function openEdit(row: ResourceRecord) {
    setSelected(row);
    setValues(
      Object.fromEntries(
        updateFields.map((field) => {
          const raw = row[field.name];
          if (field.type === "datetime-local" && typeof raw === "string") return [field.name, raw.slice(0, 16)];
          if (field.type === "multiselect" && Array.isArray(raw)) return [field.name, raw.join(",")];
          return [field.name, raw === null || raw === undefined ? "" : String(raw)];
        }),
      ),
    );
    setMode("edit");
    setError(null);
    setPageError(null);
  }

  function triggerAction(row: ResourceRecord, action: ResourceRowAction) {
    if (action.fields?.length) {
      setSelected(row);
      setActiveAction(action);
      setActionValues(initialValues(action.fields));
      setActionError(null);
      return;
    }

    actionMutation.mutate({ row, action, submittedValues: {} });
  }

  function renderField(
    field: ResourceField,
    currentValues: Record<string, string>,
    setter: React.Dispatch<React.SetStateAction<Record<string, string>>>,
  ) {
    const options = resolveOptions(field, currentValues);

    return (
      <label key={field.name}>
        {field.label}
        {field.type === "textarea" ? (
          <textarea
            required={field.required}
            value={currentValues[field.name] ?? ""}
            placeholder={field.placeholder}
            onChange={(event) => updateFieldValue(field, event.target.value, setter)}
          />
        ) : field.type === "select" ? (
          <select
            required={field.required}
            value={currentValues[field.name] ?? ""}
            onChange={(event) => updateFieldValue(field, event.target.value, setter)}
          >
            <option value="">{field.placeholder ?? "Selecciona una opción"}</option>
            {options.map((option) => (
              <option key={String(option.value)} value={option.value}>{option.label}</option>
            ))}
          </select>
        ) : field.type === "multiselect" ? (
          <select
            multiple
            required={field.required}
            value={(currentValues[field.name] ?? "").split(",").filter(Boolean)}
            onChange={(event) => {
              const selectedValues = Array.from(event.currentTarget.selectedOptions).map((option) => option.value);
              updateFieldValue(field, selectedValues.join(","), setter);
            }}
          >
            {options.map((option) => (
              <option key={String(option.value)} value={option.value}>{option.label}</option>
            ))}
          </select>
        ) : (
          <input
            type={field.type ?? "text"}
            required={field.required}
            min={field.min}
            max={field.max}
            placeholder={field.placeholder}
            value={currentValues[field.name] ?? ""}
            onChange={(event) => updateFieldValue(field, event.target.value, setter)}
          />
        )}
      </label>
    );
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

      {pageError && <div className="panel-state error-message" role="alert">{pageError}</div>}
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
                          {updateFields.length > 0 && row.id && canEdit(row) && (
                            <button type="button" className="icon-button" title="Editar" onClick={() => openEdit(row)}>
                              <Pencil size={15} />
                            </button>
                          )}
                          {rowActions
                            .filter((action) => action.isVisible?.(row) ?? true)
                            .map((action) => (
                              <button
                                key={action.label}
                                type="button"
                                className="text-action"
                                disabled={actionMutation.isPending}
                                onClick={() => triggerAction(row, action)}
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

            <form className="resource-form" onSubmit={(event) => { event.preventDefault(); saveMutation.mutate(); }}>
              {activeFields.map((field) => renderField(field, values, setValues))}
              {error && <div className="form-error">{error}</div>}
              <div className="form-actions">
                <button type="button" className="secondary-button" onClick={() => setMode("closed")}>Cancelar</button>
                <button type="submit" disabled={saveMutation.isPending}>{saveMutation.isPending ? "Guardando..." : "Guardar"}</button>
              </div>
            </form>
          </section>
        </div>
      )}

      {activeAction && selected && (
        <div className="modal-backdrop" role="presentation">
          <section className="modal-card" role="dialog" aria-modal="true" aria-label={activeAction.label}>
            <header>
              <div>
                <p className="eyebrow">ACCIÓN</p>
                <h2>{activeAction.label}</h2>
              </div>
              <button type="button" className="icon-button" onClick={() => setActiveAction(null)}><X size={18} /></button>
            </header>
            <form
              className="resource-form"
              onSubmit={(event) => {
                event.preventDefault();
                actionMutation.mutate({ row: selected, action: activeAction, submittedValues: actionValues });
              }}
            >
              {(activeAction.fields ?? []).map((field) => renderField(field, actionValues, setActionValues))}
              {actionError && <div className="form-error">{actionError}</div>}
              <div className="form-actions">
                <button type="button" className="secondary-button" onClick={() => setActiveAction(null)}>Cancelar</button>
                <button type="submit" disabled={actionMutation.isPending}>
                  {actionMutation.isPending ? "Procesando..." : (activeAction.submitLabel ?? "Confirmar")}
                </button>
              </div>
            </form>
          </section>
        </div>
      )}
    </main>
  );
}
