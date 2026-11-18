import { expect, test } from '@playwright/test';
import { openModule, uiLogin } from './support/session';

// Se ejecuta en los proyectos de escritorio y móvil (playwright.config.ts).
test('responsive smoke: login, dashboard and a functional module', async ({ page }) => {
  await uiLogin(page);
  await expect(page.getByRole('region', { name: 'Indicadores principales' })).toBeVisible();

  await openModule(page, 'Auditorías');
  await expect(page).toHaveURL(/\/audits$/);
  await expect(page.getByRole('heading', { name: 'Auditorías', level: 1 })).toBeVisible();
  await expect(page.getByRole('table')).toBeVisible();

  // Sin desbordamiento horizontal del documento: las tablas se desplazan dentro de su contenedor.
  const overflow = await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
  expect(overflow).toBeLessThanOrEqual(1);
});
