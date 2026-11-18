/**
 * Almacenamiento centralizado de la sesión (tokens + usuario) en localStorage.
 * Es la única pieza que conoce las claves de almacenamiento; auth.ts y apiClient.ts la reutilizan.
 */

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

interface AccessTokenClaims {
  sub?: string;
  exp?: number;
  organization_id?: string;
}

export interface Session {
  accessToken: string;
  refreshToken: string;
  user: AuthUser;
  userId: string | null;
  organizationId: string | null;
  accessTokenExpiresAt: number | null;
}

const sessionListeners = new Set<() => void>();
let sessionVersion = 0;
let storageListenerInstalled = false;

function notifySessionChange() {
  sessionVersion += 1;
  for (const listener of sessionListeners) listener();
}

/** Suscripción para useSyncExternalStore: cambios locales y de otras pestañas (evento storage). */
export function subscribeSession(listener: () => void) {
  if (!storageListenerInstalled && typeof window !== "undefined") {
    window.addEventListener("storage", (event) => {
      if (event.key === null || event.key.startsWith("auditcore.")) notifySessionChange();
    });
    storageListenerInstalled = true;
  }
  sessionListeners.add(listener);
  return () => {
    sessionListeners.delete(listener);
  };
}

export function getSessionVersion() {
  return sessionVersion;
}

function readStorage(key: string) {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

function decodeAccessToken(token: string): AccessTokenClaims | null {
  const parts = token.split(".");
  if (parts.length !== 3 || !parts[1]) return null;

  try {
    const base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
    const json = new TextDecoder().decode(Uint8Array.from(atob(padded), (char) => char.charCodeAt(0)));
    const claims = JSON.parse(json) as unknown;
    return typeof claims === "object" && claims !== null ? (claims as AccessTokenClaims) : null;
  } catch {
    return null;
  }
}

function parseUser(value: string | null): AuthUser | null {
  if (!value) return null;
  try {
    const user = JSON.parse(value) as Partial<AuthUser>;
    if (typeof user.email !== "string" || !Array.isArray(user.roles) || !Array.isArray(user.permissions)) return null;
    return {
      email: user.email,
      fullName: typeof user.fullName === "string" ? user.fullName : user.email,
      roles: user.roles.filter((role): role is string => typeof role === "string"),
      permissions: user.permissions.filter((permission): permission is string => typeof permission === "string"),
    };
  } catch {
    return null;
  }
}

/**
 * Devuelve la sesión almacenada si es estructuralmente válida. Un access token corrupto o un usuario
 * ilegible invalidan la sesión y la limpian. Un access token expirado se acepta mientras exista refresh
 * token: el interceptor HTTP lo renovará y, si falla, cerrará la sesión.
 */
export function getSession(): Session | null {
  const accessToken = readStorage(ACCESS_TOKEN_KEY);
  const refreshToken = readStorage(REFRESH_TOKEN_KEY);
  const rawUser = readStorage(USER_KEY);

  if (!accessToken && !refreshToken && !rawUser) return null;

  const claims = accessToken ? decodeAccessToken(accessToken) : null;
  const user = parseUser(rawUser);

  if (!accessToken || !refreshToken || !claims || !user) {
    // Se limpia sin notificar: getSession puede ejecutarse durante el render.
    removeSessionKeys();
    return null;
  }

  return {
    accessToken,
    refreshToken,
    user,
    userId: typeof claims.sub === "string" ? claims.sub : null,
    organizationId: typeof claims.organization_id === "string" ? claims.organization_id : null,
    accessTokenExpiresAt: typeof claims.exp === "number" ? claims.exp * 1000 : null,
  };
}

export function getAccessToken() {
  return readStorage(ACCESS_TOKEN_KEY);
}

export function getRefreshToken() {
  return readStorage(REFRESH_TOKEN_KEY);
}

export function getCurrentUser(): AuthUser | null {
  return getSession()?.user ?? null;
}

export function isAuthenticated() {
  return getSession() !== null;
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
  notifySessionChange();
}

/** Sincroniza roles y permisos con los que la API reporta para el token vigente (GET /auth/me). */
export function updateSessionAccess(roles: string[], permissions: string[]) {
  const current = getSession();
  if (!current) return;
  const unchanged =
    current.user.roles.join("|") === roles.join("|") &&
    current.user.permissions.join("|") === permissions.join("|");
  if (unchanged) return;
  localStorage.setItem(USER_KEY, JSON.stringify({ ...current.user, roles, permissions } satisfies AuthUser));
  notifySessionChange();
}

function removeSessionKeys() {
  try {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  } catch {
    // El almacenamiento puede no estar disponible (modo privado estricto); no hay nada que limpiar.
  }
}

export function clearSession() {
  removeSessionKeys();
  notifySessionChange();
}
