import { apiClient, refreshAccessToken } from "../../services/apiClient";
import { clearSession, getRefreshToken, saveSession, updateSessionAccess, type AuthResponse } from "./session";

export {
  clearSession,
  getAccessToken,
  getCurrentUser,
  getRefreshToken,
  getSession,
  isAuthenticated,
  saveSession,
} from "./session";
export type { AuthResponse, AuthUser, Session } from "./session";

export interface LoginRequest {
  email: string;
  password: string;
}

interface MeResponse {
  roles: string[];
  permissions: string[];
}

export async function login(request: LoginRequest) {
  const { data } = await apiClient.post<AuthResponse>("/auth/login", request);
  saveSession(data);
  return data;
}

export function refreshSession() {
  return refreshAccessToken();
}

/**
 * Valida la sesión almacenada contra la API. Un access token expirado se renueva mediante el interceptor;
 * si el refresh falla, el interceptor cierra la sesión y redirige al login. Los roles y permisos locales
 * se alinean con los del token vigente.
 */
export async function verifySession() {
  try {
    const { data } = await apiClient.get<MeResponse>("/auth/me");
    if (Array.isArray(data.roles) && Array.isArray(data.permissions)) {
      updateSessionAccess(data.roles, data.permissions);
    }
  } catch {
    // 401 sin refresh posible ya fue tratado por el interceptor; un error de red no invalida la sesión local.
  }
}

export async function logout() {
  const refreshToken = getRefreshToken();

  try {
    if (refreshToken) {
      await apiClient.post("/auth/logout", { refreshToken });
    }
  } catch {
    // El cierre local de sesión no debe depender de que la API responda.
  } finally {
    clearSession();
  }
}
