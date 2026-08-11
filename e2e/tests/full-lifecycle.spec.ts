import { expect, request as playwrightRequest, test } from '@playwright/test';

const apiBaseUrl = process.env.E2E_API_URL ?? 'http://127.0.0.1:5047/api';
const adminEmail = process.env.E2E_ADMIN_EMAIL ?? 'admin@auditcore.local';
const adminPassword = process.env.E2E_ADMIN_PASSWORD ?? 'AuditCore-E2E-2026!';

type AuthResponse = {
  accessToken: string;
};

type EntityRef = {
  id: string;
};

async function createAuthenticatedApi() {
  const anonymous = await playwrightRequest.newContext({ baseURL: apiBaseUrl });

  const loginResponse = await anonymous.post('/auth/login', {
    data: {
      email: adminEmail,
      password: adminPassword,
    },
  });

  expect(loginResponse.status()).toBe(200);
  const auth = (await loginResponse.json()) as AuthResponse;
  expect(auth.accessToken).toBeTruthy();
  await anonymous.dispose();

  return playwrightRequest.newContext({
    baseURL: apiBaseUrl,
    extraHTTPHeaders: {
      Authorization: `Bearer ${auth.accessToken}`,
    },
  });
}

test('executes the complete audit lifecycle against the real API and database', async () => {
  const api = await createAuthenticatedApi();
  const suffix = `${Date.now()}-${Math.floor(Math.random() * 10_000)}`;

  try {
    const organizationsResponse = await api.get('/organizations');
    expect(organizationsResponse.status()).toBe(200);
    const organizations = (await organizationsResponse.json()) as EntityRef[];
    expect(organizations.length).toBeGreaterThan(0);
    const organizationId = organizations[0].id;

    const usersResponse = await api.get(`/users?organizationId=${organizationId}`);
    expect(usersResponse.status()).toBe(200);
    const users = (await usersResponse.json()) as EntityRef[];
    expect(users.length).toBeGreaterThan(0);
    const adminUserId = users[0].id;

    const auditResponse = await api.post('/audits', {
      data: {
        organizationId,
        code: `AUD-E2E-${suffix}`,
        title: `Auditoría E2E ${suffix}`,
        objective: 'Validar el ciclo completo de AuditCore mediante Playwright.',
        scope: 'Flujo automatizado de QA de la versión 1.0.0.',
      },
    });
    expect(auditResponse.status()).toBe(201);
    const audit = (await auditResponse.json()) as EntityRef;

    const riskResponse = await api.post('/risks', {
      data: {
        auditId: audit.id,
        code: `RSK-E2E-${suffix}`,
        title: 'Riesgo E2E',
        description: 'Riesgo creado por la validación end-to-end.',
        probability: 4,
        impact: 4,
        treatment: 'Mitigar mediante controles y seguimiento automatizado.',
        ownerUserId: adminUserId,
      },
    });
    expect(riskResponse.status()).toBe(201);
    const risk = (await riskResponse.json()) as EntityRef;

    const findingResponse = await api.post('/findings', {
      data: {
        auditId: audit.id,
        riskId: risk.id,
        code: `FND-E2E-${suffix}`,
        title: 'Hallazgo E2E',
        condition: 'Se detectó una condición simulada durante la prueba E2E.',
        criteria: 'El control esperado debe estar implementado y documentado.',
        cause: 'Escenario automatizado de validación.',
        effect: 'Exposición controlada para verificar el flujo del producto.',
        recommendation: 'Aplicar el plan de acción definido por la prueba.',
        severity: 3,
        responsibleUserId: adminUserId,
        dueDateUtc: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
      },
    });
    expect(findingResponse.status()).toBe(201);
    const finding = (await findingResponse.json()) as EntityRef;

    const evidenceText = `AuditCore E2E evidence ${suffix}`;
    const evidenceResponse = await api.post('/evidence', {
      multipart: {
        auditId: audit.id,
        findingId: finding.id,
        description: 'Evidencia generada automáticamente por Playwright.',
        uploadedByUserId: adminUserId,
        file: {
          name: `auditcore-e2e-${suffix}.txt`,
          mimeType: 'text/plain',
          buffer: Buffer.from(evidenceText, 'utf-8'),
        },
      },
    });
    expect(evidenceResponse.status()).toBe(201);
    const evidence = (await evidenceResponse.json()) as EntityRef;

    const downloadResponse = await api.get(`/evidence/${evidence.id}/download`);
    expect(downloadResponse.status()).toBe(200);
    expect(await downloadResponse.text()).toBe(evidenceText);

    const actionPlanResponse = await api.post('/action-plans', {
      data: {
        findingId: finding.id,
        title: `Plan de acción E2E ${suffix}`,
        description: 'Plan creado para validar seguimiento, progreso y cierre.',
        responsibleUserId: adminUserId,
        dueDateUtc: new Date(Date.now() + 10 * 24 * 60 * 60 * 1000).toISOString(),
      },
    });
    expect(actionPlanResponse.status()).toBe(201);
    const actionPlan = (await actionPlanResponse.json()) as EntityRef;

    expect(
      (await api.put(`/action-plans/${actionPlan.id}/progress`, {
        data: { progressPercent: 50 },
      })).status(),
    ).toBe(204);

    expect(
      (await api.put(`/action-plans/${actionPlan.id}/complete`, {
        data: { notes: 'Completado satisfactoriamente por la prueba E2E.' },
      })).status(),
    ).toBe(204);

    expect((await api.put(`/risks/${risk.id}/start-treatment`)).status()).toBe(204);
    expect((await api.put(`/risks/${risk.id}/mitigate`)).status()).toBe(204);
    expect((await api.put(`/risks/${risk.id}/close`)).status()).toBe(204);

    expect((await api.put(`/findings/${finding.id}/review`)).status()).toBe(204);
    expect((await api.put(`/findings/${finding.id}/accept`)).status()).toBe(204);
    expect((await api.put(`/findings/${finding.id}/resolve`)).status()).toBe(204);
    expect((await api.put(`/findings/${finding.id}/close`)).status()).toBe(204);

    const startDateUtc = new Date(Date.now() + 60 * 60 * 1000).toISOString();
    const endDateUtc = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString();

    const planAuditResponse = await api.put(`/audits/${audit.id}/plan`, {
      data: {
        leadAuditorUserId: adminUserId,
        startDateUtc,
        endDateUtc,
      },
    });
    expect(planAuditResponse.status()).toBe(200);

    expect((await api.put(`/audits/${audit.id}/start`)).status()).toBe(204);
    expect((await api.put(`/audits/${audit.id}/complete`)).status()).toBe(204);
    expect((await api.put(`/audits/${audit.id}/close`)).status()).toBe(204);

    const auditDetailsResponse = await api.get(`/audits/${audit.id}`);
    expect(auditDetailsResponse.status()).toBe(200);

    const dashboardResponse = await api.get(`/reports/dashboard?organizationId=${organizationId}`);
    expect(dashboardResponse.status()).toBe(200);

    const exportResponse = await api.get(
      `/reports/audits/export?organizationId=${organizationId}&format=Csv`,
    );
    expect(exportResponse.status()).toBe(200);
    expect((await exportResponse.body()).byteLength).toBeGreaterThan(0);

    expect((await api.delete(`/evidence/${evidence.id}`)).status()).toBe(204);
  } finally {
    await api.dispose();
  }
});
