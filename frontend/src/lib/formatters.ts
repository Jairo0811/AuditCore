/** Fecha corta localizada o "—" cuando no hay valor válido. */
export function formatDate(value: unknown) {
  if (typeof value !== "string" || !value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleDateString();
}

export function formatDateTime(value: unknown) {
  if (typeof value !== "string" || !value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleString();
}

export function splitIds(value: string | undefined) {
  return value ? value.split(",").filter(Boolean) : [];
}
