export function labelFromMap(value: unknown, labels: Record<number, string>) {
  const numericValue = typeof value === "number" ? value : Number(value);
  return Number.isFinite(numericValue) ? (labels[numericValue] ?? String(value ?? "—")) : String(value ?? "—");
}

export const auditStatusLabels: Record<number, string> = {
  1: "Borrador",
  2: "Planificada",
  3: "En curso",
  4: "Completada",
  5: "Cerrada",
  6: "Cancelada",
};

export const riskLevelLabels: Record<number, string> = {
  1: "Bajo",
  2: "Medio",
  3: "Alto",
  4: "Crítico",
};

export const riskStatusLabels: Record<number, string> = {
  1: "Identificado",
  2: "En tratamiento",
  3: "Aceptado",
  4: "Mitigado",
  5: "Cerrado",
};

export const findingSeverityLabels: Record<number, string> = {
  1: "Baja",
  2: "Media",
  3: "Alta",
  4: "Crítica",
};

export const findingStatusLabels: Record<number, string> = {
  1: "Abierto",
  2: "En revisión",
  3: "Aceptado",
  4: "Resuelto",
  5: "Cerrado",
};

export const actionPlanStatusLabels: Record<number, string> = {
  1: "Pendiente",
  2: "En progreso",
  3: "Completado",
  4: "Vencido",
  5: "Cancelado",
};
