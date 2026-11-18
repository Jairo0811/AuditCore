import { expect, test } from '@playwright/test';
import { uiLogin } from './support/session';

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

  test('rejects wrong credentials without leaving the login screen', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Correo electrónico').fill('nadie@auditcore.local');
    await page.getByLabel('Contraseña').fill('Contrasena-Incorrecta-1');
    await page.getByRole('button', { name: 'Iniciar sesión' }).click();
    await expect(page.getByRole('alert')).toContainText('Correo o contraseña incorrectos.');
    await expect(page).toHaveURL(/\/login$/);
  });

  test('logs in and logs out using the real API', async ({ page }) => {
    await uiLogin(page);
    await page.getByRole('button', { name: 'Cerrar sesión' }).click();
    await expect(page).toHaveURL(/\/login$/);
  });

  test('visiting /login with a valid session goes to the app instead of an empty screen', async ({ page }) => {
    await uiLogin(page);
    await page.goto('/login');
    await expect(page).toHaveURL(/\/dashboard$/);
    await expect(page.getByRole('heading', { name: 'Dashboard', level: 1 })).toBeVisible();
  });

  test('a corrupted stored session is discarded and sends the user to login', async ({ page }) => {
    await uiLogin(page);
    await page.evaluate(() => localStorage.setItem('auditcore.accessToken', 'not-a-jwt'));
    await page.goto('/audits');
    await expect(page).toHaveURL(/\/login$/);
    await expect(page.getByRole('heading', { name: 'Iniciar sesión' })).toBeVisible();
  });
});

test.describe('Authenticated navigation', () => {
  test.beforeEach(async ({ page }) => {
    await uiLogin(page);
  });

  const modules = [
    ['Auditorías', '/audits'],
    ['Riesgos', '/risks'],
    ['Hallazgos', '/findings'],
    ['Evidencias', '/evidence'],
    ['Planes de acción', '/action-plans'],
    ['Marcos y controles', '/frameworks'],
    ['Evaluaciones', '/assessments'],
    ['Reportes', '/reports'],
    ['Organizaciones', '/organizations'],
    ['Sucursales', '/branches'],
    ['Departamentos', '/departments'],
    ['Usuarios', '/users'],
    ['Roles', '/roles'],
  ] as const;

  for (const [label, path] of modules) {
    test(`opens ${label}`, async ({ page }) => {
      await page.getByRole('link', { name: label, exact: true }).click();
      await expect(page).toHaveURL(new RegExp(`${path}$`));
      await expect(page.getByRole('heading', { name: label, level: 1 })).toBeVisible();
      await expect(page.getByRole('alert')).toHaveCount(0);
    });
  }
});
