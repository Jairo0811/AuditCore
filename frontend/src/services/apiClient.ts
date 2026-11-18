import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import {
  clearSession,
  getAccessToken,
  getRefreshToken,
  saveSession,
  type AuthResponse,
} from "../features/auth/session";

// En desarrollo, "/api" pasa por el proxy de Vite (vite.config.ts); en Docker/CI se define VITE_API_URL.
const apiBaseUrl = import.meta.env.VITE_API_URL || "/api";

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

// Cliente sin interceptores para renovar tokens sin recursión.
const refreshClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

interface RetryableRequest extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

let refreshPromise: Promise<string> | null = null;

// Endpoints de autenticación que nunca deben disparar un refresh automático (evita recursión).
const AUTH_ENDPOINT_PATTERN = /\/auth\/(login|refresh|logout)\/?$/;

/** Renueva el access token usando el refresh token almacenado. Comparte una única petición concurrente. */
export function refreshAccessToken() {
  refreshPromise ??= (async () => {
    const refreshToken = getRefreshToken();
    if (!refreshToken) throw new Error("No existe una sesión renovable.");
    const { data } = await refreshClient.post<AuthResponse>("/auth/refresh", { refreshToken });
    saveSession(data);
    return data.accessToken;
  })().finally(() => {
    refreshPromise = null;
  });

  return refreshPromise;
}

function endSessionAndRedirect() {
  clearSession();
  if (window.location.pathname !== "/login") {
    window.location.assign("/login");
  }
}

apiClient.interceptors.request.use((config) => {
  const accessToken = getAccessToken();

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const request = error.config as RetryableRequest | undefined;

    if (error.response?.status !== 401 || !request || AUTH_ENDPOINT_PATTERN.test(request.url ?? "")) {
      return Promise.reject(error);
    }

    if (request._retry || !getRefreshToken()) {
      endSessionAndRedirect();
      return Promise.reject(error);
    }

    request._retry = true;

    let accessToken: string;
    try {
      accessToken = await refreshAccessToken();
    } catch (refreshError) {
      endSessionAndRedirect();
      return Promise.reject(refreshError);
    }

    // Un error del reintento (403, 409…) se propaga normalmente; solo un refresh fallido cierra la sesión.
    request.headers.Authorization = `Bearer ${accessToken}`;
    return apiClient(request);
  },
);
