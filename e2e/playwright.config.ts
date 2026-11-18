import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  // Las capturas del README solo se generan bajo demanda (E2E_SCREENSHOTS=1).
  testIgnore: process.env.E2E_SCREENSHOTS ? [] : [/screenshots\.spec\.ts$/],
  fullyParallel: false,
  forbidOnly: Boolean(process.env.CI),
  retries: process.env.CI ? 1 : 0,
  workers: 1,
  reporter: process.env.CI ? [['line'], ['html', { outputFolder: 'playwright-report', open: 'never' }]] : 'list',
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://127.0.0.1:4173',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      // Smoke responsive real en viewport móvil (Chromium); no duplica el ciclo completo.
      name: 'mobile-chromium',
      use: { ...devices['Pixel 7'] },
      testMatch: /responsive\.spec\.ts$/,
    },
  ],
});
