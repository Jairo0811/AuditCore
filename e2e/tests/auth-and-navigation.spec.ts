import { expect, test } from '@playwright/test';

const adminEmail = process.env.E2E_ADMIN_EMAIL ?? 'admin@auditcore.local';
const adminPassword = process.env.E2E_ADMIN_PASSWORD ?? 'AuditCore-E2E-2026!';

async function login(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('Correo electrónico').fill(adminEmail);
  await page.getByLabel('Contraseña').fill(adminPassword);
  await page.getByRole('button', { name: 'Iniciar sesión' }).click();
  await expect(page).toHaveURL(/\/dashboard$/);
  await expect(page.getByRole('heading', { name: /dashboard/i })).toBeVisible();
}

test.describe('Authentication', () => {
  test('redirects anonymous users to login', async ({ page }) => {
    await page.goto('/audits');
    await expect(page).toHaveURL(/\/login$/);
    await expect(page.getByRole('heading', { name: 'Iniciar sesión' })).toBeVisible();
  });

  test('validates malformed login input', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Correo electrónico').fill('correo-invalido');
    await page.getByLabel('Contraseña').fill('123');
    await page.getByRole('button', { name: 'Iniciar sesión' }).click();

    await expect(page.getByText('Ingresa un correo electrónico válido.')).toBeVisible();
    await expect(page.getByText('La contraseña debe tener al menos 8 caracteres.')).toBeVisible();
  });

  test('logs in and logs out using the real API', async ({ page }) => {
    await login(page);
    await page.getByRole('button', { name: 'Cerrar sesión' }).click();
    await expect(page).toHaveURL(/\/login$/);
  });
});

test.describe('Authenticated navigation', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  const modules = [
    ['Auditorías', '/audits'],
    ['Riesgos', '/risks'],
    ['Hallazgos', '/findings'],
    ['Evidencias', '/evidence'],
    ['Planes de acción', '/action-plans'],
    ['Marcos y controles', '/frameworks'],
    ['Reportes', '/reports'],
    ['Organizaciones', '/organizations'],
    ['Usuarios', '/users'],
    ['Roles', '/roles'],
  ] as const;

  for (const [label, path] of modules) {
    test(`opens ${label}`, async ({ page }) => {
      await page.getByRole('link', { name: label }).click();
      await expect(page).toHaveURL(new RegExp(`${path}$`));
      await expect(page.locator('main')).toBeVisible();
    });
  }
});
