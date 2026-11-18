import axios from "axios";

interface ProblemDetails {
  title?: string;
  detail?: string;
  message?: string;
  errors?: Record<string, string[]>;
}

function messageFromStatus(status: number | undefined, fallback: string) {
  switch (status) {
    case 401:
      return "Tu sesión expiró. Inicia sesión nuevamente.";
    case 403:
      return "No tienes permisos para realizar esta acción.";
    case 404:
      return "El recurso solicitado ya no existe.";
    case 413:
      return "El archivo supera el tamaño máximo permitido (20 MB).";
    case 429:
      return "Demasiadas solicitudes. Espera un momento e inténtalo nuevamente.";
    default:
      return fallback;
  }
}

function messageFromProblem(data: unknown): string | null {
  if (typeof data === "string") return data.trim() || null;
  if (typeof data !== "object" || data === null) return null;

  const problem = data as ProblemDetails;
  const validation = problem.errors ? Object.values(problem.errors).flat().find(Boolean) : undefined;
  return problem.detail || validation || problem.message || problem.title || null;
}

/** Mensaje legible para un error de la API (Problem Details, validación o estado HTTP). */
export function getApiErrorMessage(error: unknown, fallback: string) {
  if (axios.isAxiosError(error)) {
    if (!error.response) return "No fue posible conectar con AuditCore. Verifica tu conexión e inténtalo nuevamente.";
    const status = error.response.status;
    if (status === 403 || status === 401) return messageFromStatus(status, fallback);
    return messageFromProblem(error.response.data) ?? messageFromStatus(status, fallback);
  }

  return error instanceof Error && error.message ? error.message : fallback;
}

/**
 * Igual que getApiErrorMessage, pero también interpreta respuestas binarias (responseType: "blob"),
 * cuyo cuerpo de error llega como Blob en lugar de JSON.
 */
export async function readApiErrorMessage(error: unknown, fallback: string) {
  if (axios.isAxiosError(error) && error.response?.data instanceof Blob) {
    const status = error.response.status;
    if (status === 403 || status === 401) return messageFromStatus(status, fallback);
    try {
      const text = await error.response.data.text();
      const parsed = text ? (JSON.parse(text) as unknown) : null;
      return messageFromProblem(parsed) ?? messageFromStatus(status, fallback);
    } catch {
      return messageFromStatus(status, fallback);
    }
  }

  return getApiErrorMessage(error, fallback);
}
