import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Plus, RefreshCw, Trash2 } from "lucide-react";
import { useState, type Dispatch, type SetStateAction } from "react";
import { hasAnyPermission } from "../features/auth/permissions";
import { getApiErrorMessage } from "../lib/apiErrors";
import { apiClient } from "../services/apiClient";
import { ResourceFieldInput } from "./resource/ResourceFieldInput";
import type { FormValues, ResourceColumn, ResourceField, ResourceRecord, ResourceRowAction } from "./resource/types";
import { FeedbackMessage, type Feedback } from "./ui/FeedbackMessage";
import { Modal } from "./ui/Modal";
import { useConfirmDialog, type ConfirmOptions } from "./ui/useConfirmDialog";

export type { FormValues, ResourceColumn, ResourceField, ResourceOption, ResourceRecord, ResourceRowAction } from "./resource/types";

interface ResourceManagerProps {
  title: string;
  description: string;
  endpoint: string;
  queryKey: string;
  columns: ResourceColumn[];
  createFields?: ResourceField[];
  updateFields?: ResourceField[];
  mapCreate?: (values: FormValues) => unknown;
  mapUpdate?: (values: FormValues, row: ResourceRecord) => unknown;
  rowActions?: ResourceRowAction[];
  /** El usuario puede crear registros (permiso ya evaluado por la página). Por defecto no se muestra "Nuevo". */
  canCreate?: boolean;
  /** El usuario puede editar este registro (permiso y estado). Por defecto no se permite editar. */
  canUpdate?: (row: ResourceRecord) => boolean;
  /** El usuario puede eliminar este registro. Por defecto no se permite eliminar. */
  canDelete?: (row: ResourceRecord) => boolean;
  deleteConfirm?: (row: ResourceRecord) => ConfirmOptions;
  /** Texto que identifica una fila en nombres accesibles y confirmaciones. Por defecto, la primera columna. */
  getRowLabel?: (row: ResourceRecord) => string;
  eyebrow?: string;
  headingLevel?: 1 | 2;
  emptyMessage?: string;
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
  if (Array.isArray(value)) return value.join(", ") || "—";
  if (typeof value === "object") return JSON.stringify(value);
  return String(value);
}

function initialValues(fields: ResourceField[]): FormValues {
  return Object.fromEntries(fields.map((field) => [field.name, String(field.defaultValue ?? "")]));
}

/** Valores de edición: los campos del formulario más los valores escalares de la fila como contexto (p. ej. organizationId para filtrar selectores dependientes). */
function valuesFromRow(fields: ResourceField[], row: ResourceRecord): FormValues {
  const context = Object.fromEntries(
    Object.entries(row)
      .filter(([, raw]) => typeof raw === "string" || typeof raw === "number" || typeof raw === "boolean")
      .map(([key, raw]) => [key, String(raw)]),
  );
  return {
    ...context,
    ...Object.fromEntries(fields.map((field) => {
      const raw = row[field.name];
      if (field.type === "datetime-local" && typeof raw === "string") return [field.name, toLocalDateTimeInput(raw)];
      if (field.type === "multiselect" && Array.isArray(raw)) return [field.name, raw.join(",")];
      return [field.name, raw === null || raw === undefined ? "" : String(raw)];
    })),
  };
}

/** Convierte una fecha ISO UTC al formato local que espera un input datetime-local. */
function toLocalDateTimeInput(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  const offset = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}

function isActionAllowed(action: ResourceRowAction) {
  if (!action.permission) return true;
  const permissions = Array.isArray(action.permission) ? action.permission : [action.permission];
  return hasAnyPermission(...permissions);
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
  rowActions = [],
  canCreate = false,
  canUpdate = () => false,
  canDelete,
  deleteConfirm,
  getRowLabel,
  eyebrow = "GESTIÓN",
  headingLevel = 1,
  emptyMessage = "No hay registros para mostrar.",
}: ResourceManagerProps) {
  const queryClient = useQueryClient();
  const { confirm, dialog: confirmDialog } = useConfirmDialog();
  const [mode, setMode] = useState<"closed" | "create" | "edit">("closed");
  const [selected, setSelected] = useState<ResourceRecord | null>(null);
  const [values, setValues] = useState<FormValues>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<Feedback | null>(null);
  const [activeAction, setActiveAction] = useState<ResourceRowAction | null>(null);
  const [actionValues, setActionValues] = useState<FormValues>({});
  const [actionError, setActionError] = useState<string | null>(null);

  const Heading = headingLevel === 1 ? "h1" : "h2";
  const allowedActions = rowActions.filter(isActionAllowed);
  const showCreate = canCreate && createFields.length > 0;
  const rowLabel = (row: ResourceRecord) =>
    getRowLabel?.(row) ?? formatCell(row[columns[0]?.key ?? "id"]);

  const resourceQuery = useQuery({
    queryKey: [queryKey],
    queryFn: async () => (await apiClient.get<ResourceRecord[]>(endpoint)).data,
  });

  const rows = resourceQuery.data ?? [];
  const activeFields = mode === "edit" ? updateFields : createFields;

  const saveMutation = useMutation({
    mutationFn: async () => {
      const fields = mode === "edit" ? updateFields : createFields;
      const normalized = Object.fromEntries(
        fields.map((field) => [field.name, normalizeValue(field, values[field.name] ?? "")]),
      );

      if (mode === "edit" && selected?.id) {
        await apiClient.put(`${endpoint}/${selected.id}`, mapUpdate ? mapUpdate(values, selected) : normalized);
        return "Cambios guardados correctamente.";
      }

      await apiClient.post(endpoint, mapCreate ? mapCreate(values) : normalized);
      return "Registro creado correctamente.";
    },
    onSuccess: async (message) => {
      closeForm();
      setFeedback({ tone: "success", message });
      await queryClient.invalidateQueries({ queryKey: [queryKey] });
    },
    onError: (error) =>
      setFormError(getApiErrorMessage(error, "No fue posible guardar los cambios. Revisa los datos e inténtalo nuevamente.")),
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`${endpoint}/${id}`);
    },
    onSuccess: async () => {
      setFeedback({ tone: "success", message: "Registro eliminado correctamente." });
      await queryClient.invalidateQueries({ queryKey: [queryKey] });
    },
    onError: (error) =>
      setFeedback({ tone: "error", message: getApiErrorMessage(error, "No fue posible eliminar el registro.") }),
  });

  const actionMutation = useMutation({
    mutationFn: async ({ row, action, submittedValues }: {
      row: ResourceRecord;
      action: ResourceRowAction;
      submittedValues: FormValues;
    }) => {
      await apiClient.request({
        url: action.endpoint(row),
        method: action.method ?? "put",
        data: action.body?.(row, submittedValues),
      });
      return action.successMessage ?? `${action.label}: acción completada correctamente.`;
    },
    onSuccess: async (message) => {
      closeAction();
      setFeedback({ tone: "success", message });
      await queryClient.invalidateQueries({ queryKey: [queryKey] });
    },
    onError: (error, variables) => {
      const message = getApiErrorMessage(error, "No fue posible completar la acción solicitada.");
      if (variables.action.fields?.length) setActionError(message);
      else setFeedback({ tone: "error", message });
    },
  });

  const busy = saveMutation.isPending || deleteMutation.isPending || actionMutation.isPending;

  function updateFieldValue(field: ResourceField, value: string, setter: Dispatch<SetStateAction<FormValues>>) {
    setter((current) => {
      const next = { ...current, [field.name]: value };
      for (const dependentField of field.clearFieldsOnChange ?? []) next[dependentField] = "";
      return next;
    });
  }

  function closeForm() {
    setMode("closed");
    setSelected(null);
    setFormError(null);
  }

  function closeAction() {
    setActiveAction(null);
    setActionValues({});
    setActionError(null);
    setSelected(null);
  }

  function openCreate() {
    setSelected(null);
    setValues(initialValues(createFields));
    setFormError(null);
    setFeedback(null);
    setMode("create");
  }

  function openEdit(row: ResourceRecord) {
    setSelected(row);
    setValues(valuesFromRow(updateFields, row));
    setFormError(null);
    setFeedback(null);
    setMode("edit");
  }

  async function triggerAction(row: ResourceRecord, action: ResourceRowAction) {
    setFeedback(null);

    if (action.fields?.length) {
      setSelected(row);
      setActionValues({ ...initialValues(action.fields), ...(action.getInitialValues?.(row) ?? {}) });
      setActionError(null);
      setActiveAction(action);
      return;
    }

    if (action.confirm) {
      const options = typeof action.confirm === "function" ? action.confirm(row) : action.confirm;
      if (!(await confirm(options))) return;
    }

    actionMutation.mutate({ row, action, submittedValues: {} });
  }

  async function requestDelete(row: ResourceRecord) {
    if (!row.id) return;
    setFeedback(null);
    const confirmed = await confirm(
      deleteConfirm?.(row) ?? {
        title: "Eliminar registro",
        message: `¿Eliminar «${rowLabel(row)}»? Esta acción no se puede deshacer.`,
        confirmLabel: "Eliminar",
        tone: "danger",
      },
    );
    if (confirmed) deleteMutation.mutate(row.id);
  }

  // La columna de acciones solo existe si al menos una fila ofrece alguna acción permitida.
  const rowHasActions = (row: ResourceRecord) =>
    Boolean(row.id) && (
      (updateFields.length > 0 && canUpdate(row)) ||
      (canDelete?.(row) ?? false) ||
      allowedActions.some((action) => action.isVisible?.(row) ?? true)
    );
  const renderActionsColumn = rows.some(rowHasActions);

  return (
    <section className="module-page" aria-labelledby={`${queryKey}-title`}>
      <header className="module-header">
        <div>
          <p className="eyebrow">{eyebrow}</p>
          <Heading id={`${queryKey}-title`}>{title}</Heading>
          <p>{description}</p>
        </div>
        <div className="module-header-actions">
          <button
            type="button"
            className="secondary-button"
            disabled={resourceQuery.isFetching}
            onClick={() => resourceQuery.refetch()}
          >
            <RefreshCw size={16} aria-hidden="true" /> {resourceQuery.isFetching ? "Actualizando..." : "Actualizar"}
          </button>
          {showCreate && (
            <button type="button" onClick={openCreate}>
              <Plus size={16} aria-hidden="true" /> Nuevo
            </button>
          )}
        </div>
      </header>

      <FeedbackMessage feedback={feedback} onDismiss={() => setFeedback(null)} />

      {resourceQuery.isPending && (
        <div className="panel-state" role="status">Cargando información...</div>
      )}

      {resourceQuery.isError && (
        <div className="panel-state error-message" role="alert">
          <p>{getApiErrorMessage(resourceQuery.error, "No fue posible cargar el módulo.")}</p>
          <button type="button" className="secondary-button" onClick={() => resourceQuery.refetch()}>Reintentar</button>
        </div>
      )}

      {resourceQuery.isSuccess && (
        <div className="data-panel" aria-busy={resourceQuery.isFetching || busy}>
          <div className="table-scroll">
            <table className="data-table">
              <caption className="sr-only">{title}</caption>
              <thead>
                <tr>
                  {columns.map((column) => <th key={column.key} scope="col">{column.label}</th>)}
                  {renderActionsColumn && <th scope="col">Acciones</th>}
                </tr>
              </thead>
              <tbody>
                {rows.map((row, index) => {
                  const label = rowLabel(row);
                  const visibleActions = allowedActions.filter((action) => action.isVisible?.(row) ?? true);
                  const editable = updateFields.length > 0 && Boolean(row.id) && canUpdate(row);
                  const deletable = Boolean(row.id) && (canDelete?.(row) ?? false);

                  return (
                    <tr key={row.id ?? index}>
                      {columns.map((column) => (
                        <td key={column.key}>{column.render ? column.render(row[column.key], row) : formatCell(row[column.key])}</td>
                      ))}
                      {renderActionsColumn && (
                        <td>
                          {editable || deletable || visibleActions.length > 0 ? (
                            <div className="row-actions">
                              {editable && (
                                <button
                                  type="button"
                                  className="icon-button"
                                  aria-label={`Editar ${label}`}
                                  title="Editar"
                                  disabled={busy}
                                  onClick={() => openEdit(row)}
                                >
                                  <Pencil size={15} aria-hidden="true" />
                                </button>
                              )}
                              {visibleActions.map((action) => (
                                <button
                                  key={action.label}
                                  type="button"
                                  className="text-action"
                                  aria-label={`${action.label} ${label}`}
                                  disabled={busy}
                                  onClick={() => void triggerAction(row, action)}
                                >
                                  {action.label}
                                </button>
                              ))}
                              {deletable && (
                                <button
                                  type="button"
                                  className="icon-button danger-button"
                                  aria-label={`Eliminar ${label}`}
                                  title="Eliminar"
                                  disabled={busy}
                                  onClick={() => void requestDelete(row)}
                                >
                                  <Trash2 size={15} aria-hidden="true" />
                                </button>
                              )}
                            </div>
                          ) : (
                            <span className="muted-text">Sin acciones disponibles</span>
                          )}
                        </td>
                      )}
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
          {rows.length === 0 && <p className="empty-table">{emptyMessage}</p>}
        </div>
      )}

      {mode !== "closed" && (
        <Modal
          eyebrow={mode === "edit" ? "EDITAR" : "NUEVO REGISTRO"}
          title={mode === "edit" && selected ? `${title}: ${rowLabel(selected)}` : title}
          onClose={closeForm}
        >
          <form
            className="resource-form"
            onSubmit={(event) => {
              event.preventDefault();
              setFormError(null);
              saveMutation.mutate();
            }}
          >
            {activeFields.map((field) => (
              <ResourceFieldInput
                key={field.name}
                field={field}
                values={values}
                onChange={(changed, value) => updateFieldValue(changed, value, setValues)}
              />
            ))}
            {formError && <div className="form-error" role="alert">{formError}</div>}
            <div className="form-actions">
              <button type="button" className="secondary-button" onClick={closeForm}>Cancelar</button>
              <button type="submit" disabled={saveMutation.isPending}>
                {saveMutation.isPending ? "Guardando..." : "Guardar"}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {activeAction && selected && (
        <Modal
          eyebrow="ACCIÓN"
          title={`${activeAction.label}: ${rowLabel(selected)}`}
          description={activeAction.description}
          onClose={closeAction}
        >
          <form
            className="resource-form"
            onSubmit={(event) => {
              event.preventDefault();
              setActionError(null);
              actionMutation.mutate({ row: selected, action: activeAction, submittedValues: actionValues });
            }}
          >
            {(activeAction.fields ?? []).map((field) => (
              <ResourceFieldInput
                key={field.name}
                field={field}
                values={actionValues}
                onChange={(changed, value) => updateFieldValue(changed, value, setActionValues)}
              />
            ))}
            {actionError && <div className="form-error" role="alert">{actionError}</div>}
            <div className="form-actions">
              <button type="button" className="secondary-button" onClick={closeAction}>Cancelar</button>
              <button type="submit" disabled={actionMutation.isPending}>
                {actionMutation.isPending ? "Procesando..." : (activeAction.submitLabel ?? "Confirmar")}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {confirmDialog}
    </section>
  );
}
