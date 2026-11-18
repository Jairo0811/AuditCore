import { expect, request as playwrightRequest, type APIRequestContext, type Page } from '@playwright/test';

export const apiBaseUrl = process.env.E2E_API_URL ?? 'http://127.0.0.1:5047/api/';
export const adminEmail = process.env.E2E_ADMIN_EMAIL ?? 'admin@auditcore.local';
export const adminPassword = process.env.E2E_ADMIN_PASSWORD ?? 'AuditCore-E2E-2026!';

export interface Me {
  userId: string;
  organizationId: string;
  roles: string[];
  permissions: string[];
}

export function uniqueSuffix() {
  return `${Date.now()}-${Math.floor(Math.random() * 10_000)}`;
}

/** Contexto de API autenticado con un usuario real (login contra la API y la base de datos reales). */
export async function apiAs(email: string, password: string): Promise<APIRequestContext> {
  const anonymous = await playwrightRequest.newContext({ baseURL: apiBaseUrl });
  const response = await anonymous.post('auth/login', { data: { email, password } });
  expect(response.status(), `login ${email}`).toBe(200);
  const { accessToken } = (await response.json()) as { accessToken: string };
  await anonymous.dispose();
  return playwrightRequest.newContext({
    baseURL: apiBaseUrl,
    extraHTTPHeaders: { Authorization: `Bearer ${accessToken}` },
  });
}

export function adminApi() {
  return apiAs(adminEmail, adminPassword);
}

export async function getMe(api: APIRequestContext): Promise<Me> {
  const response = await api.get('auth/me');
  expect(response.status()).toBe(200);
  return (await response.json()) as Me;
}

/** Inicia sesión en la interfaz y espera la pantalla inicial permitida. */
export async function uiLogin(page: Page, email = adminEmail, password = adminPassword) {
  await page.goto('/login');
  await page.getByLabel('Correo electrónico').fill(email);
  await page.getByLabel('Contraseña').fill(password);
  await page.getByRole('button', { name: 'Iniciar sesión' }).click();
  await expect(page).toHaveURL(/\/dashboard$/);
  await expect(page.getByRole('heading', { name: 'Dashboard', level: 1 })).toBeVisible();
}

/** Abre un módulo desde la navegación principal (en móvil abre antes el menú lateral). */
export async function openModule(page: Page, label: string) {
  const menuButton = page.getByRole('button', { name: 'Abrir menú de navegación' });
  if (await menuButton.isVisible()) await menuButton.click();
  await page.getByRole('navigation', { name: 'Navegación principal' }).getByRole('link', { name: label, exact: true }).click();
}

export interface CreatedUser {
  id: string;
  email: string;
  password: string;
  fullName: string;
}

/** Crea un usuario de prueba con un rol semilla (p. ej. VIEWER) en la organización del administrador. */
export async function createUserWithRole(api: APIRequestContext, roleCode: string): Promise<CreatedUser> {
  const me = await getMe(api);
  const rolesResponse = await api.get('roles');
  expect(rolesResponse.status()).toBe(200);
  const roles = (await rolesResponse.json()) as { id: string; code: string }[];
  const role = roles.find((item) => item.code === roleCode);
  expect(role, `rol ${roleCode}`).toBeTruthy();

  const suffix = uniqueSuffix();
  const user = {
    email: `e2e.${roleCode.toLowerCase()}.${suffix}@auditcore.local`,
    password: `E2e-${roleCode}-Pass-${suffix}`,
    firstName: 'E2E',
    lastName: `${roleCode} ${suffix}`,
  };
  const response = await api.post('users', {
    data: { organizationId: me.organizationId, firstName: user.firstName, lastName: user.lastName, email: user.email, password: user.password, roleIds: [role!.id] },
  });
  expect(response.status()).toBe(201);
  const created = (await response.json()) as { id: string; fullName: string };
  return { id: created.id, email: user.email, password: user.password, fullName: created.fullName };
}
