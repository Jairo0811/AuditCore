import type { ReactNode } from "react";
import type { PermissionCode } from "../../features/auth/permissions";
import type { ConfirmOptions } from "../ui/useConfirmDialog";

export type ResourceRecord = Record<string, unknown> & { id?: string };
export type FormValues = Record<string, string>;

export interface ResourceOption {
  label: string;
  value: string | number;
}

export interface ResourceField {
  name: string;
  label: string;
  type?: "text" | "email" | "number" | "textarea" | "datetime-local" | "select" | "multiselect" | "password";
  required?: boolean;
  options?: ResourceOption[] | ((values: FormValues) => ResourceOption[]);
  defaultValue?: string | number;
  placeholder?: string;
  helpText?: string;
  min?: number;
  max?: number;
  minLength?: number;
  maxLength?: number;
  autoComplete?: string;
  clearFieldsOnChange?: string[];
}

export interface ResourceColumn {
  key: string;
  label: string;
  render?: (value: unknown, row: ResourceRecord) => ReactNode;
}

export interface ResourceRowAction {
  label: string;
  endpoint: (row: ResourceRecord) => string;
  method?: "post" | "put" | "delete";
  body?: (row: ResourceRecord, values: FormValues) => unknown;
  /** Confirmación accesible previa a acciones sin formulario (cancelar, cerrar, etc.). */
  confirm?: ConfirmOptions | ((row: ResourceRecord) => ConfirmOptions);
  fields?: ResourceField[];
  /** Valores iniciales del formulario de la acción (p. ej. permisos o roles ya asignados). */
  getInitialValues?: (row: ResourceRecord) => FormValues;
  description?: string;
  submitLabel?: string;
  successMessage?: string;
  isVisible?: (row: ResourceRecord) => boolean;
  /** Permisos requeridos (basta con uno). Si el usuario no los tiene, la acción no se muestra. */
  permission?: PermissionCode | PermissionCode[];
}
