import { expect, test, type APIRequestContext } from '@playwright/test';
import { adminApi, apiAs, createUserWithRole, getMe, uiLogin, type CreatedUser } from './support/session';

// Matriz semilla del rol VIEWER (DefaultRolePermissions): solo lectura.
const viewerModules = ['Dashboard', 'Auditorías', 'Riesgos', 'Hallazgos', 'Evidencias', 'Planes de acción', 'Marcos y controles', 'Evaluaciones', 'Organizaciones', 'Sucursales', 'Departamentos'];
const hiddenForViewer = ['Usuarios', 'Roles', 'Reportes'];

test.describe.configure({ mode: 'serial' });

let admin: APIRequestContext;
let viewer: CreatedUser;

test.beforeAll(async () => {
  admin = await adminApi();
  viewer = await createUserWithRole(admin, 'VIEWER');
});

test.afterAll(async () => {
  await admin?.dispose();
});

test('VIEWER only sees the modules it can consult and no mutation controls', async ({ page }) => {
  await uiLogin(page, viewer.email, viewer.password);
  const nav = page.getByRole('navigation', { name: 'Navegación principal' });

  for (const label of viewerModules) {
    await expect(nav.getByRole('link', { name: label, exact: true }), label).toBeVisible();
  }
  for (const label of hiddenForViewer) {
    await expect(nav.getByRole('link', { name: label, exact: true }), label).toHaveCount(0);
  }

  for (const path of ['/audits', '/risks', '/findings', '/action-plans', '/frameworks', '/organizations', '/branches', '/departments']) {
    await page.goto(path);
    await expect(page.getByRole('heading', { level: 1 }), path).toBeVisible();
    await expect(page.getByRole('button', { name: 'Nuevo' }), path).toHaveCount(0);
    await expect(
      page.getByRole('button', { name: /^(Editar|Eliminar|Planificar|Iniciar|Completar|Tratar|Aceptar|Mitigar|Resolver|Cerrar(?! sesión)|Cancelar|Progreso|Enviar a revisión)\b/ }),
      path,
    ).toHaveCount(0);
  }

  await page.goto('/evidence');
  await expect(page.getByRole('heading', { name: 'Evidencias', level: 1 })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Subir evidencia' })).toHaveCount(0);
  await expect(page.getByRole('button', { name: /^Eliminar / })).toHaveCount(0);

  await page.goto('/assessments');
  await expect(page.getByRole('button', { name: 'Nueva pregunta' })).toHaveCount(0);
});

test('VIEWER opening a forbidden route sees an accessible access-denied page (no loop)', async ({ page }) => {
  await uiLogin(page, viewer.email, viewer.password);
  for (const path of ['/users', '/roles', '/reports']) {
    await page.goto(path);
    await expect(page).toHaveURL(new RegExp(`${path}$`));
    await expect(page.getByRole('heading', { name: 'Acceso denegado', level: 1 })).toBeVisible();
  }
  await page.getByRole('link', { name: 'Ir a una sección disponible' }).click();
  await expect(page).toHaveURL(/\/dashboard$/);
});

test('backend rejects VIEWER mutations with 403 regardless of the UI', async () => {
  const api = await apiAs(viewer.email, viewer.password);
  try {
    const me = await getMe(api);
    expect(me.roles).toEqual(['VIEWER']);

    expect((await api.get('audits')).status()).toBe(200);
    expect((await api.get('evidence')).status()).toBe(200);
    expect((await api.get('users')).status()).toBe(403);
    expect((await api.get('roles')).status()).toBe(403);
    expect((await api.get('reports/audits/export?format=Csv')).status()).toBe(403);

    expect((await api.post('audits', { data: { organizationId: me.organizationId, title: 'Intento VIEWER' } })).status()).toBe(403);
    expect((await api.post('organizations', { data: { name: 'Intento VIEWER' } })).status()).toBe(403);
    expect((await api.put(`users/${viewer.id}/roles`, { data: { roleIds: [] } })).status()).toBe(403);
    expect((await api.post('evidence', {
      multipart: { auditId: me.organizationId, file: { name: 'x.txt', mimeType: 'text/plain', buffer: Buffer.from('x') } },
    })).status()).toBe(403);
  } finally {
    await api.dispose();
  }
});

test('role permission matrix is preloaded and saving unchanged keeps it intact', async ({ page }) => {
  const before = ((await (await admin.get('roles')).json()) as { code: string; permissions: string[] }[])
    .find((role) => role.code === 'VIEWER')!;
  expect(before.permissions.length).toBeGreaterThan(0);

  await uiLogin(page);
  await page.goto('/roles');
  await page.getByRole('button', { name: 'Permisos VIEWER' }).click();
  const dialog = page.getByRole('dialog');
  await expect(dialog).toBeVisible();
  await expect(dialog.getByRole('checkbox', { checked: true })).toHaveCount(before.permissions.length);
  for (const code of before.permissions) {
    await expect(dialog.getByRole('checkbox', { name: new RegExp(`^${code.replace('.', '\\.')} —`) })).toBeChecked();
  }

  await dialog.getByRole('button', { name: 'Guardar permisos' }).click();
  await expect(dialog).toHaveCount(0);
  await expect(page.getByRole('status').filter({ hasText: 'Permisos del rol actualizados.' })).toBeVisible();

  const after = ((await (await admin.get('roles')).json()) as { code: string; permissions: string[] }[])
    .find((role) => role.code === 'VIEWER')!;
  expect([...after.permissions].sort()).toEqual([...before.permissions].sort());
});

test('user roles can be managed after creation with current roles preloaded', async ({ page }) => {
  await uiLogin(page);
  await page.goto('/users');
  await page.getByRole('button', { name: `Roles ${viewer.fullName}` }).click();
  const dialog = page.getByRole('dialog');
  await expect(dialog.getByRole('checkbox', { name: /^VIEWER —/ })).toBeChecked();
  await expect(dialog.getByRole('checkbox', { checked: true })).toHaveCount(1);

  await dialog.getByRole('checkbox', { name: /^AUDITOR —/ }).check();
  await dialog.getByRole('button', { name: 'Guardar roles' }).click();
  await expect(dialog).toHaveCount(0);

  const updated = (await (await admin.get(`users/${viewer.id}`)).json()) as { roles: string[] };
  expect([...updated.roles].sort()).toEqual(['AUDITOR', 'VIEWER']);

  // Se restaura el rol original para no alterar las pruebas posteriores.
  await page.getByRole('button', { name: `Roles ${viewer.fullName}` }).click();
  await page.getByRole('dialog').getByRole('checkbox', { name: /^AUDITOR —/ }).uncheck();
  await page.getByRole('dialog').getByRole('button', { name: 'Guardar roles' }).click();
  await expect(page.getByRole('dialog')).toHaveCount(0);
  const restored = (await (await admin.get(`users/${viewer.id}`)).json()) as { roles: string[] };
  expect(restored.roles).toEqual(['VIEWER']);
});
