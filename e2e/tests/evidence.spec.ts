import { expect, test } from '@playwright/test';
import { adminApi, getMe, uiLogin, uniqueSuffix } from './support/session';

test('evidence upload, download and delete through the UI, then 404 everywhere', async ({ page }) => {
  const api = await adminApi();
  const suffix = uniqueSuffix();
  try {
    const me = await getMe(api);
    const auditResponse = await api.post('audits', {
      data: { organizationId: me.organizationId, title: `Auditoría evidencias ${suffix}`, objective: 'Validar el ciclo de evidencias.' },
    });
    expect(auditResponse.status()).toBe(201);
    const audit = (await auditResponse.json()) as { id: string; code: string; title: string };

    const fileName = `evidencia-e2e-${suffix}.txt`;
    const content = `AuditCore v1.2.0 evidence ${suffix}`;

    await uiLogin(page);
    await page.goto('/evidence');
    await page.getByRole('combobox', { name: 'Auditoría', exact: true }).selectOption({ label: `${audit.code} — ${audit.title}` });
    await page.getByLabel('Descripción').fill('Evidencia cargada desde la interfaz.');
    await page.getByLabel('Archivo').setInputFiles({ name: fileName, mimeType: 'text/plain', buffer: Buffer.from(content, 'utf-8') });
    await page.getByRole('button', { name: 'Subir' }).click();
    await expect(page.getByRole('status').filter({ hasText: `Evidencia «${fileName}» subida correctamente.` })).toBeVisible();
    await expect(page.getByRole('cell', { name: fileName, exact: true })).toBeVisible();

    const list = (await (await api.get(`evidence?auditId=${audit.id}`)).json()) as { id: string; fileName: string; uploadedByUserId: string }[];
    const evidence = list.find((item) => item.fileName === fileName);
    expect(evidence).toBeTruthy();
    // La autoría corresponde al usuario autenticado que realizó la carga.
    expect(evidence!.uploadedByUserId).toBe(me.userId);

    const download = page.waitForEvent('download');
    await page.getByRole('button', { name: `Descargar ${fileName}` }).click();
    expect((await download).suggestedFilename()).toBe(fileName);

    await page.getByRole('button', { name: `Eliminar ${fileName}` }).click();
    const confirm = page.getByRole('alertdialog');
    await expect(confirm).toBeVisible();
    await expect(confirm.getByRole('button', { name: 'Volver' })).toBeFocused();
    await confirm.getByRole('button', { name: 'Eliminar' }).click();
    await expect(page.getByRole('status').filter({ hasText: `Evidencia «${fileName}» eliminada.` })).toBeVisible();
    await expect(page.getByRole('cell', { name: fileName, exact: true })).toHaveCount(0);

    expect((await api.get(`evidence/${evidence!.id}`)).status()).toBe(404);
    expect((await api.get(`evidence/${evidence!.id}/download`)).status()).toBe(404);
    expect((await api.delete(`evidence/${evidence!.id}`)).status()).toBe(404);
    const afterDelete = (await (await api.get('evidence')).json()) as { id: string }[];
    expect(afterDelete.some((item) => item.id === evidence!.id)).toBe(false);
  } finally {
    await api.dispose();
  }
});

test('evidence API rejects files whose content does not match the extension', async () => {
  const api = await adminApi();
  try {
    const me = await getMe(api);
    const audit = (await (await api.post('audits', { data: { organizationId: me.organizationId, title: `Auditoría archivos ${uniqueSuffix()}` } })).json()) as { id: string };
    const response = await api.post('evidence', {
      multipart: { auditId: audit.id, file: { name: 'falso.pdf', mimeType: 'application/pdf', buffer: Buffer.from('no es un pdf') } },
    });
    expect(response.status()).toBeGreaterThanOrEqual(400);
    expect(response.status()).toBeLessThan(500);
  } finally {
    await api.dispose();
  }
});
