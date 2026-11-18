import { expect, test } from '@playwright/test';
import { adminApi, getMe, uiLogin } from './support/session';

// Capturas reales para el README (API, base de datos y frontend reales). Solo se ejecuta con E2E_SCREENSHOTS=1.
const outputDir = process.env.E2E_SCREENSHOTS_DIR ?? 'screenshots';

test.describe.configure({ mode: 'serial' });

test.beforeAll(async () => {
  const api = await adminApi();
  try {
    const me = await getMe(api);
    for (const title of ['Auditoría de controles de acceso lógico', 'Revisión de continuidad del negocio', 'Evaluación de gestión de cambios TI']) {
      const response = await api.post('audits', {
        data: { organizationId: me.organizationId, title, objective: 'Evaluar el diseño y la eficacia operativa de los controles.', scope: 'Procesos de TI de la organización.' },
      });
      expect(response.status()).toBe(201);
    }
  } finally {
    await api.dispose();
  }
});

test('desktop screenshots', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto('/login');
  await expect(page.getByRole('heading', { name: 'Iniciar sesión' })).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.screenshot({ path: `${outputDir}/login-desktop.png` });

  await uiLogin(page);
  await page.waitForLoadState('networkidle');
  await page.screenshot({ path: `${outputDir}/dashboard.png` });

  await page.goto('/audits');
  await expect(page.getByRole('table')).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.screenshot({ path: `${outputDir}/audits.png` });
});

test('mobile screenshot', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await uiLogin(page);
  await page.goto('/audits');
  await expect(page.getByRole('table')).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.screenshot({ path: `${outputDir}/audits-mobile.png` });
});
