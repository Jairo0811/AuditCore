import { expect, test } from '@playwright/test';
import { uiLogin } from './support/session';

const authenticatedRoutes = [
  '/dashboard', '/audits', '/risks', '/findings', '/evidence', '/action-plans', '/frameworks',
  '/assessments', '/reports', '/organizations', '/branches', '/departments', '/users', '/roles',
];

test.describe('Semantics and keyboard accessibility', () => {
  test.beforeEach(async ({ page }) => {
    await uiLogin(page);
  });

  test('every authenticated screen exposes exactly one main landmark and one h1', async ({ page }) => {
    const consoleErrors: string[] = [];
    page.on('pageerror', (error) => consoleErrors.push(error.message));
    page.on('console', (message) => {
      if (message.type() === 'error') consoleErrors.push(message.text());
    });

    for (const route of authenticatedRoutes) {
      await page.goto(route);
      await expect(page.getByRole('heading', { level: 1 }), route).toHaveCount(1);
      await expect(page.locator('main'), route).toHaveCount(1);
      await expect(page.getByRole('main'), route).toHaveCount(1);
    }

    expect(consoleErrors).toEqual([]);
  });

  test('modal receives focus, traps Tab, closes with Escape and restores focus', async ({ page }) => {
    await page.goto('/organizations');
    const trigger = page.getByRole('button', { name: 'Nuevo' });
    await trigger.click();

    const dialog = page.getByRole('dialog');
    await expect(dialog).toBeVisible();
    await expect(dialog).toHaveAttribute('aria-modal', 'true');
    await expect(dialog).toHaveAccessibleName(/Organizaciones/);
    expect(await dialog.evaluate((element) => element.contains(document.activeElement))).toBe(true);

    for (let index = 0; index < 10; index += 1) await page.keyboard.press('Tab');
    expect(await dialog.evaluate((element) => element.contains(document.activeElement))).toBe(true);
    await page.keyboard.press('Shift+Tab');
    expect(await dialog.evaluate((element) => element.contains(document.activeElement))).toBe(true);

    await page.keyboard.press('Escape');
    await expect(dialog).toHaveCount(0);
    await expect(trigger).toBeFocused();
  });

  test('icon-only actions have accessible names and tables use column headers', async ({ page }) => {
    await page.goto('/organizations');
    await expect(page.getByRole('button', { name: /^Editar / }).first()).toBeVisible();
    const headers = page.locator('table thead th');
    expect(await headers.count()).toBeGreaterThan(0);
    for (const scope of await headers.evaluateAll((cells) => cells.map((cell) => cell.getAttribute('scope')))) {
      expect(scope).toBe('col');
    }
  });
});

test.describe('Mobile navigation', () => {
  test.use({ viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true });

  test('menu opens, moves focus inside, closes with Escape and after navigating', async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('.login-mobile-brand')).toBeVisible();
    await uiLogin(page);

    const nav = page.getByRole('navigation', { name: 'Navegación principal' });
    const openButton = page.getByRole('button', { name: 'Abrir menú de navegación' });
    await expect(nav.getByRole('link', { name: 'Riesgos' })).not.toBeInViewport();

    await openButton.click();
    await expect(openButton).toHaveAttribute('aria-expanded', 'true');
    await expect(page.getByRole('button', { name: 'Cerrar menú de navegación' })).toBeFocused();
    await expect(nav.getByRole('link', { name: 'Riesgos' })).toBeInViewport();

    await page.keyboard.press('Escape');
    await expect(openButton).toHaveAttribute('aria-expanded', 'false');
    await expect(openButton).toBeFocused();

    await openButton.click();
    await nav.getByRole('link', { name: 'Riesgos' }).click();
    await expect(page).toHaveURL(/\/risks$/);
    await expect(openButton).toHaveAttribute('aria-expanded', 'false');
    await expect(page.getByRole('heading', { name: 'Riesgos', level: 1 })).toBeVisible();
  });
});
