import { apiClient } from "../../services/apiClient";

const ACCESS_TOKEN_KEY = "auditcore.accessToken";
const REFRESH_TOKEN_KEY = "auditcore.refreshToken";
const USER_KEY = "auditcore.user";

export interface AuthUser {
  email: string;
  fullName: string;
  roles: string[];
  permissions: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  email: string;
  fullName: string;
  roles: string[];
  permissions: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export function getAccessToken() {
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function getRefreshToken() {
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function getCurrentUser(): AuthUser | null {
  const value = localStorage.getItem(USER_KEY);

  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value) as AuthUser;
  } catch {
    clearSession();
    return null;
  }
}

export function isAuthenticated() {
  return Boolean(getAccessToken() && getRefreshToken());
}

export function saveSession(response: AuthResponse) {
  localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
  localStorage.setItem(
    USER_KEY,
    JSON.stringify({
      email: response.email,
      fullName: response.fullName,
      roles: response.roles,
      permissions: response.permissions,
    } satisfies AuthUser),
  );
}

export function clearSession() {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export async function login(request: LoginRequest) {
  const { data } = await apiClient.post<AuthResponse>("/auth/login", request);
  saveSession(data);
  return data;
}

export async function refreshSession() {
  const refreshToken = getRefreshToken();

  if (!refreshToken) {
    throw new Error("No existe una sesión renovable.");
  }

  const { data } = await apiClient.post<AuthResponse>("/auth/refresh", {
    refreshToken,
  });

  saveSession(data);
  return data;
}

export async function logout() {
  const refreshToken = getRefreshToken();

  try {
    if (refreshToken) {
      await apiClient.post("/auth/logout", { refreshToken });
    }
  } finally {
    clearSession();
  }
}
