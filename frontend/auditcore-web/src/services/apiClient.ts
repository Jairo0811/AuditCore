import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";

const ACCESS_TOKEN_KEY = "auditcore.accessToken";
const REFRESH_TOKEN_KEY = "auditcore.refreshToken";
const USER_KEY = "auditcore.user";

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5047/api",
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

const refreshClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5047/api",
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

interface RetryableRequest extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

let refreshPromise: Promise<string> | null = null;

apiClient.interceptors.request.use((config) => {
  const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const request = error.config as RetryableRequest | undefined;

    if (error.response?.status !== 401 || !request || request._retry || request.url?.includes("/auth/")) {
      return Promise.reject(error);
    }

    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);

    if (!refreshToken) {
      clearStoredSession();
      return Promise.reject(error);
    }

    request._retry = true;

    try {
      refreshPromise ??= renewAccessToken(refreshToken);
      const accessToken = await refreshPromise;
      request.headers.Authorization = `Bearer ${accessToken}`;
      return await apiClient(request);
    } catch (refreshError) {
      clearStoredSession();
      window.location.assign("/login");
      return Promise.reject(refreshError);
    } finally {
      refreshPromise = null;
    }
  },
);

async function renewAccessToken(refreshToken: string) {
  const { data } = await refreshClient.post<{
    accessToken: string;
    refreshToken: string;
    email: string;
    fullName: string;
    roles: string[];
    permissions: string[];
  }>("/auth/refresh", { refreshToken });

  localStorage.setItem(ACCESS_TOKEN_KEY, data.accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
  localStorage.setItem(
    USER_KEY,
    JSON.stringify({
      email: data.email,
      fullName: data.fullName,
      roles: data.roles,
      permissions: data.permissions,
    }),
  );

  return data.accessToken;
}

function clearStoredSession() {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}
