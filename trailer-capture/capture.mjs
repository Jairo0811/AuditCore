// Captura del tráiler: siembra datos FICTICIOS en una base de datos efímera de CI y graba la interfaz real.
import { chromium } from '../e2e/node_modules/@playwright/test/index.mjs';
import { mkdirSync, writeFileSync } from 'node:fs';
import { randomBytes } from 'node:crypto';

const API = process.env.API_URL ?? 'http://127.0.0.1:5047/api/';
const WEB = process.env.WEB_URL ?? 'http://127.0.0.1:4173';
const ADMIN_EMAIL = process.env.ADMIN_EMAIL;
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;
const OUT = 'out';
for (const dir of ['stills', 'clips', 'files']) mkdirSync(`${OUT}/${dir}`, { recursive: true });

let token = '';
async function api(method, path, body, { form } = {}) {
  const headers = { Authorization: `Bearer ${token}` };
  let payload;
  if (form) payload = form;
  else if (body !== undefined) { headers['Content-Type'] = 'application/json'; payload = JSON.stringify(body); }
  const res = await fetch(API + path, { method, headers, body: payload });
  if (!res.ok) throw new Error(`${method} ${path} -> ${res.status} ${await res.text()}`);
  const type = res.headers.get('content-type') ?? '';
  if (type.includes('application/json')) return res.json();
  return Buffer.from(await res.arrayBuffer());
}
const days = (n) => new Date(Date.now() + n * 86400000).toISOString();
const pwd = () => `Demo-${randomBytes(9).toString('base64url')}9a`;

// ---------- Datos ficticios ----------
async function seed() {
  const login = await (await fetch(API + 'auth/login', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email: ADMIN_EMAIL, password: ADMIN_PASSWORD }) })).json();
  token = login.accessToken;
  const me = await api('GET', 'auth/me');
  const org = me.organizationId;
  await api('PUT', `organizations/${org}`, { name: 'Empresa Demo', description: 'Organización ficticia de demostración.', isActive: true });

  const branchHq = await api('POST', 'branches', { organizationId: org, name: 'Sede Central', address: 'Dirección de demostración' });
  await api('POST', 'branches', { organizationId: org, name: 'Centro de Datos', address: 'Dirección de demostración' });
  await api('POST', 'departments', { organizationId: org, name: 'Tecnología de la Información', branchId: branchHq.id });
  await api('POST', 'departments', { organizationId: org, name: 'Auditoría Interna', branchId: branchHq.id });
  await api('POST', 'departments', { organizationId: org, name: 'Seguridad de la Información', branchId: branchHq.id });

  const roles = await api('GET', 'roles');
  const role = (code) => roles.find((r) => r.code === code).id;
  const mkUser = (firstName, lastName, email, code) => api('POST', 'users', { organizationId: org, firstName, lastName, email, password: pwd(), roleIds: [role(code)] });
  const laura = await mkUser('Laura', 'Méndez', 'laura.mendez@empresademo.local', 'AUDIT_MANAGER');
  const carlos = await mkUser('Carlos', 'Rivas', 'carlos.rivas@empresademo.local', 'AUDITOR');
  const ana = await mkUser('Ana', 'Torres', 'ana.torres@empresademo.local', 'AUDITEE');
  await mkUser('Diego', 'Paredes', 'diego.paredes@empresademo.local', 'VIEWER');

  const audit = (title, objective, scope) => api('POST', 'audits', { organizationId: org, title, objective, scope });
  const aSec = await audit('Auditoría de Seguridad de la Información', 'Evaluar el diseño y la eficacia de los controles de seguridad.', 'Sistemas críticos y políticas de seguridad.');
  const aAcc = await audit('Auditoría de Gestión de Accesos', 'Verificar la gestión del ciclo de vida de accesos y privilegios.', 'Directorio, aplicaciones críticas y cuentas privilegiadas.');
  const aBcp = await audit('Auditoría de Continuidad del Negocio', 'Revisar los planes de continuidad y recuperación.', 'Servicios críticos y centro de datos.');
  const aChg = await audit('Auditoría de Gestión de Cambios TI', 'Evaluar la aprobación y trazabilidad de cambios.', 'Cambios en producción.');
  const aBak = await audit('Auditoría de Respaldo y Recuperación', 'Verificar respaldos y pruebas de restauración.', 'Bases de datos y servidores de archivos.');
  const plan = (a, lead, s, e) => api('PUT', `audits/${a.id}/plan`, { leadAuditorUserId: lead.id, startDateUtc: days(s), endDateUtc: days(e) });
  await plan(aSec, laura, -20, 25); await api('PUT', `audits/${aSec.id}/start`);
  await plan(aAcc, carlos, -12, 30); await api('PUT', `audits/${aAcc.id}/start`);
  await plan(aBcp, laura, 10, 45);
  await plan(aChg, carlos, -60, -5); await api('PUT', `audits/${aChg.id}/start`); await api('PUT', `audits/${aChg.id}/complete`); await api('PUT', `audits/${aChg.id}/close`);

  const risk = (a, title, description, probability, impact, owner, treatment) => api('POST', 'risks', { auditId: a.id, title, description, probability, impact, treatment, ownerUserId: owner.id });
  const rAccess = await risk(aAcc, 'Acceso no autorizado a sistemas críticos', 'Usuarios con privilegios superiores a los requeridos por su función.', 4, 5, laura, 'Revisión trimestral de accesos y principio de mínimo privilegio.');
  const rPriv = await risk(aAcc, 'Cuentas privilegiadas sin revisión periódica', 'Las cuentas administrativas no se revisan con la frecuencia definida.', 4, 4, carlos, 'Inventario y certificación de cuentas privilegiadas.');
  const rAvail = await risk(aSec, 'Pérdida de disponibilidad de servicios críticos', 'Dependencia de componentes sin redundancia.', 3, 5, laura, 'Plan de redundancia y monitoreo.');
  const rChange = await risk(aChg, 'Cambios no autorizados en producción', 'Cambios aplicados sin aprobación formal.', 3, 4, carlos, 'Flujo de aprobación de cambios.');
  const rBackup = await risk(aSec, 'Respaldos sin pruebas de restauración', 'No se verifica periódicamente la restauración.', 3, 3, ana, 'Pruebas semestrales de restauración.');
  await api('PUT', `risks/${rAccess.id}/start-treatment`);
  await api('PUT', `risks/${rChange.id}/mitigate`);
  await api('PUT', `risks/${rBackup.id}/accept`);

  const finding = (a, r, title, condition, criteria, cause, effect, recommendation, severity, owner, due) => api('POST', 'findings', {
    auditId: a.id, riskId: r?.id ?? null, title, condition, criteria, cause, effect, recommendation, severity, responsibleUserId: owner.id, dueDateUtc: days(due),
  });
  const fPriv = await finding(aAcc, rAccess, 'Controles de acceso privilegiado insuficientes', 'Se identificaron cuentas administrativas compartidas.', 'Política de control de accesos: cuentas nominativas.', 'Ausencia de herramienta de gestión de privilegios.', 'Falta de trazabilidad de acciones administrativas.', 'Implementar cuentas nominativas y registro de sesiones privilegiadas.', 3, laura, 30);
  const fReview = await finding(aAcc, rPriv, 'Revisión de permisos desactualizada', 'La última certificación de permisos excede el período definido.', 'Procedimiento de revisión trimestral de accesos.', 'Proceso manual sin responsables asignados.', 'Permisos excesivos acumulados.', 'Formalizar la revisión trimestral con responsables.', 2, carlos, 45);
  const fInactive = await finding(aAcc, rAccess, 'Cuentas inactivas sin deshabilitar', 'Existen cuentas sin uso en los últimos 90 días habilitadas.', 'Política de gestión de cuentas.', 'Falta de integración con el proceso de bajas.', 'Riesgo de uso indebido de credenciales.', 'Automatizar la desactivación de cuentas inactivas.', 3, ana, 20);
  const fChange = await finding(aChg, rChange, 'Registro de cambios incompleto', 'Algunos cambios no documentan la aprobación.', 'Procedimiento de gestión de cambios.', 'Aprobaciones verbales.', 'Pérdida de trazabilidad.', 'Exigir aprobación registrada antes de cada cambio.', 2, carlos, 15);
  await api('PUT', `findings/${fPriv.id}/review`);
  await api('PUT', `findings/${fInactive.id}/review`); await api('PUT', `findings/${fInactive.id}/accept`);
  await api('PUT', `findings/${fChange.id}/review`); await api('PUT', `findings/${fChange.id}/accept`); await api('PUT', `findings/${fChange.id}/resolve`);

  const actionPlan = (f, title, description, owner, due) => api('POST', 'action-plans', { findingId: f.id, title, description, responsibleUserId: owner.id, dueDateUtc: days(due) });
  const pReview = await actionPlan(fReview, 'Plan de revisión de permisos', 'Certificación trimestral de permisos por responsables de área.', carlos, 40);
  const pPriv = await actionPlan(fPriv, 'Implementar gestión de cuentas privilegiadas', 'Cuentas nominativas y registro de sesiones.', laura, 30);
  const pInactive = await actionPlan(fInactive, 'Depuración de cuentas inactivas', 'Deshabilitar cuentas sin uso y automatizar el proceso.', ana, 20);
  await actionPlan(fChange, 'Formalizar aprobaciones de cambios', 'Registro obligatorio de aprobaciones en la herramienta de cambios.', carlos, 25);
  await api('PUT', `action-plans/${pReview.id}/progress`, { progressPercent: 40 });
  await api('PUT', `action-plans/${pPriv.id}/progress`, { progressPercent: 65 });
  await api('PUT', `action-plans/${pInactive.id}/complete`, { notes: 'Cuentas inactivas deshabilitadas y proceso automatizado.' });

  const fw = await api('POST', 'frameworks', { name: 'Marco de Control Interno de TI (Demo)', code: 'MCI-DEMO', version: '1.0', description: 'Marco ficticio de demostración.' });
  const control = (code, title, domain, weight) => api('POST', 'frameworks/controls', { frameworkId: fw.id, code, title, domain, weight, description: 'Control de demostración.' });
  const c1 = await control('CTL-AC-01', 'Revisión periódica de accesos', 'Gestión de accesos', 3);
  const c2 = await control('CTL-AC-02', 'Gestión de cuentas privilegiadas', 'Gestión de accesos', 3);
  const c3 = await control('CTL-AC-03', 'Desactivación de cuentas inactivas', 'Gestión de accesos', 2);
  const c4 = await control('CTL-CM-01', 'Aprobación formal de cambios', 'Gestión de cambios', 2);
  await control('CTL-BK-01', 'Pruebas de restauración de respaldos', 'Continuidad', 2);
  const q = (c, text, order) => api('POST', 'frameworks/questions', { controlId: c.id, text, weight: 1, order, isRequired: true });
  await q(c1, '¿Se revisan los accesos de los usuarios al menos trimestralmente?', 1);
  await q(c1, '¿Las revisiones quedan documentadas y aprobadas por el responsable del área?', 2);
  await q(c1, '¿Se retiran los permisos no justificados tras cada revisión?', 3);
  const evaluate = (c, score, status, notes) => api('PUT', `frameworks/evaluations/${aAcc.id}/${c.id}`, { score, status, notes, evaluatedByUserId: laura.id });
  await evaluate(c1, 60, 3, 'Revisión realizada sin evidencia de aprobación.');
  await evaluate(c2, 40, 2, 'Cuentas administrativas compartidas.');
  await evaluate(c3, 85, 4, 'Proceso automatizado operativo.');
  await evaluate(c4, 70, 3, 'Aprobaciones parcialmente registradas.');

  const csv = 'usuario,sistema,perfil,estado_revision\nusuario.demo01,ERP,Consulta,Aprobado\nusuario.demo02,ERP,Administrador,Pendiente\nusuario.demo03,Directorio,Operador,Aprobado\n';
  const form = new FormData();
  form.append('auditId', aAcc.id); form.append('findingId', fReview.id); form.append('description', 'Matriz de revisión de permisos (demostración).');
  form.append('file', new Blob([csv], { type: 'text/csv' }), 'matriz-revision-permisos.csv');
  await api('POST', 'evidence', undefined, { form });

  writeFileSync(`${OUT}/files/acta-revision-accesos.txt`, 'Acta de revisión de accesos — documento ficticio de demostración.\nOrganización: Empresa Demo\nResultado: revisión completada con observaciones.\n');
  writeFileSync(`${OUT}/files/demo-ids.json`, JSON.stringify({ aAcc: aAcc.code, aBak: aBak.code, rAvail: rAvail.code, fPriv: fPriv.code, pReview: pReview.title }, null, 2));
  return { ids: { aAcc, aBak, rAvail, fPriv, fReview, pReview, c1 } };
}

// ---------- Grabación ----------
async function clip(page, name, action, { maxWidth = 1920, maxHeight = 1080 } = {}) {
  const dir = `${OUT}/clips/${name}`; mkdirSync(dir, { recursive: true });
  const cdp = await page.context().newCDPSession(page);
  const frames = []; let n = 0;
  cdp.on('Page.screencastFrame', async ({ data, metadata, sessionId }) => {
    const file = `${String(n++).padStart(5, '0')}.jpg`;
    writeFileSync(`${dir}/${file}`, Buffer.from(data, 'base64'));
    frames.push({ file, t: metadata.timestamp });
    try { await cdp.send('Page.screencastFrameAck', { sessionId }); } catch { /* detached */ }
  });
  const start = Date.now() / 1000;
  await cdp.send('Page.startScreencast', { format: 'jpeg', quality: 92, maxWidth, maxHeight, everyNthFrame: 1 });
  await page.waitForTimeout(400);
  await action();
  await page.waitForTimeout(500);
  await cdp.send('Page.stopScreencast');
  writeFileSync(`${dir}/frames.json`, JSON.stringify({ start, end: Date.now() / 1000, frames }));
  await cdp.detach();
}
const still = (page, name) => page.screenshot({ path: `${OUT}/stills/${name}.png` });
async function smoothScroll(page, total, steps = 30) { for (let i = 0; i < steps; i++) { await page.mouse.wheel(0, total / steps); await page.waitForTimeout(40); } }

async function record({ ids }) {
  const browser = await chromium.launch();
  const ctx = await browser.newContext({ viewport: { width: 1600, height: 900 }, deviceScaleFactor: 2, locale: 'es-DO', timezoneId: 'America/Santo_Domingo' });
  const page = await ctx.newPage();
  await page.goto(`${WEB}/login`); await page.waitForLoadState('networkidle'); await page.waitForTimeout(600);
  await still(page, '01-login');
  await clip(page, 'login', async () => {
    await page.getByLabel('Correo electrónico').click();
    await page.keyboard.type(ADMIN_EMAIL, { delay: 55 });
    await page.getByLabel('Contraseña').click();
    await page.keyboard.type(ADMIN_PASSWORD, { delay: 45 });
    await page.waitForTimeout(300);
    await page.getByRole('button', { name: 'Iniciar sesión' }).click();
    await page.waitForURL(/dashboard/); await page.waitForLoadState('networkidle'); await page.waitForTimeout(2200);
  });
  await still(page, '02-dashboard');

  const visit = async (path, name, extra) => { await page.goto(`${WEB}${path}`); await page.waitForLoadState('networkidle'); await page.getByRole('heading', { level: 1 }).waitFor(); await page.waitForTimeout(700); await still(page, name); if (extra) await extra(); };
  await visit('/audits', '03-audits');
  await clip(page, 'audit-plan', async () => {
    await page.getByRole('button', { name: `Planificar ${ids.aBak.code}`, exact: true }).first().click();
    const d = page.getByRole('dialog'); await d.waitFor(); await page.waitForTimeout(700);
    await d.getByLabel('Auditor líder').selectOption({ index: 1 }); await page.waitForTimeout(500);
    await still(page, '03b-audit-plan-modal');
    await page.waitForTimeout(900);
    await page.keyboard.press('Escape'); await page.waitForTimeout(800);
  });
  await visit('/risks', '04-risks');
  await clip(page, 'risk-treat', async () => {
    await page.getByRole('button', { name: `Tratar ${ids.rAvail.code}`, exact: true }).first().click();
    await page.getByRole('status').filter({ hasText: /tratamiento/i }).waitFor(); await page.waitForTimeout(1500);
  });
  await still(page, '04b-risks-after');
  await visit('/findings', '05-findings');
  await clip(page, 'finding-edit', async () => {
    await page.getByRole('button', { name: `Editar ${ids.fPriv.code}`, exact: true }).first().click();
    await page.getByRole('dialog').waitFor(); await page.waitForTimeout(1800);
    await still(page, '05b-finding-modal');
    await page.keyboard.press('Escape'); await page.waitForTimeout(600);
  });
  await visit('/evidence', '06-evidence');
  await clip(page, 'evidence-upload', async () => {
    await page.getByRole('combobox', { name: 'Auditoría', exact: true }).selectOption({ index: 2 }); await page.waitForTimeout(500);
    await page.getByLabel('Descripción').click(); await page.keyboard.type('Acta de revisión de accesos', { delay: 40 });
    await page.getByLabel('Archivo').setInputFiles(`${OUT}/files/acta-revision-accesos.txt`); await page.waitForTimeout(500);
    await page.getByRole('button', { name: 'Subir' }).click();
    await page.getByRole('status').filter({ hasText: 'subida correctamente' }).waitFor(); await page.waitForTimeout(1800);
  });
  await still(page, '06b-evidence-after');
  await visit('/action-plans', '07-action-plans');
  await clip(page, 'action-plan-progress', async () => {
    await page.getByRole('button', { name: `Progreso ${ids.pReview.title}`, exact: true }).first().click();
    const d = page.getByRole('dialog'); await d.waitFor(); await page.waitForTimeout(500);
    const input = d.getByLabel('Nuevo progreso (%)'); await input.fill(''); await input.pressSequentially('75', { delay: 120 }); await page.waitForTimeout(500);
    await d.getByRole('button', { name: 'Actualizar progreso' }).click();
    await page.getByRole('status').filter({ hasText: 'Progreso actualizado' }).waitFor(); await page.waitForTimeout(1600);
  });
  await still(page, '07b-action-plans-after');
  await visit('/frameworks', '08-frameworks');
  await visit('/assessments', '09-assessments');
  await clip(page, 'assessment', async () => {
    await page.getByRole('combobox', { name: 'Control' }).selectOption({ label: 'CTL-AC-01 — Revisión periódica de accesos' });
    await page.getByRole('table').first().waitFor(); await page.waitForTimeout(900);
    await page.getByRole('combobox', { name: 'Auditoría', exact: true }).selectOption({ label: `${ids.aAcc.code} — ${ids.aAcc.title}` });
    await page.getByRole('cell', { name: 'CTL-AC-02', exact: true }).waitFor(); await page.waitForTimeout(800);
    await still(page, '09b-assessments-filled');
    await page.getByRole('button', { name: 'Evaluar CTL-AC-01', exact: true }).first().click(); await page.getByRole('dialog').waitFor(); await page.waitForTimeout(1600);
    await still(page, '09c-evaluation-modal');
    await page.keyboard.press('Escape'); await page.waitForTimeout(500);
  });
  await visit('/reports', '10-reports');
  await clip(page, 'report-pdf', async () => {
    const download = page.waitForEvent('download');
    await page.getByRole('button', { name: 'Descargar PDF' }).click();
    const file = await download; await file.saveAs(`${OUT}/files/reporte-auditorias.pdf`);
    await page.getByText(/Reporte PDF generado/).waitFor(); await page.waitForTimeout(1500);
  });
  await visit('/organizations', '11-organizations');
  await visit('/branches', '11b-branches');
  await visit('/departments', '11c-departments');
  await visit('/users', '12-users');
  await visit('/roles', '13-roles');
  await clip(page, 'roles-permissions', async () => {
    await page.getByRole('button', { name: 'Permisos AUDITOR', exact: true }).click(); await page.getByRole('dialog').waitFor(); await page.waitForTimeout(1800);
    await still(page, '13b-roles-permissions');
    await page.keyboard.press('Escape'); await page.waitForTimeout(400);
  });
  await page.goto(`${WEB}/dashboard`); await page.waitForLoadState('networkidle'); await page.waitForTimeout(500);
  await clip(page, 'dashboard', async () => { await page.waitForTimeout(1500); await smoothScroll(page, 500); await page.waitForTimeout(800); await smoothScroll(page, -500); await page.waitForTimeout(600); });
  await still(page, '02b-dashboard-final');
  await page.goto(`${WEB}/audits`); await page.waitForLoadState('networkidle');
  await clip(page, 'audits-browse', async () => { await page.waitForTimeout(800); await smoothScroll(page, 400, 25); await page.waitForTimeout(900); });
  await ctx.close();

  // Vista responsive (navegador móvil, no aplicación nativa).
  const mctx = await browser.newContext({ viewport: { width: 390, height: 844 }, deviceScaleFactor: 3, isMobile: true, hasTouch: true, locale: 'es-DO', timezoneId: 'America/Santo_Domingo' });
  const m = await mctx.newPage();
  await m.goto(`${WEB}/login`); await m.waitForLoadState('networkidle'); await m.waitForTimeout(500);
  await still(m, '20-mobile-login');
  await m.getByLabel('Correo electrónico').fill(ADMIN_EMAIL); await m.getByLabel('Contraseña').fill(ADMIN_PASSWORD);
  await m.getByRole('button', { name: 'Iniciar sesión' }).click(); await m.waitForURL(/dashboard/); await m.waitForLoadState('networkidle'); await m.waitForTimeout(800);
  await still(m, '21-mobile-dashboard');
  await clip(m, 'mobile-nav', async () => {
    await m.waitForTimeout(600);
    await m.getByRole('button', { name: 'Abrir menú de navegación' }).click(); await m.waitForTimeout(1100);
    await still(m, '22-mobile-menu');
    await m.getByRole('navigation', { name: 'Navegación principal' }).getByRole('link', { name: 'Hallazgos' }).click();
    await m.waitForURL(/findings/); await m.waitForLoadState('networkidle'); await m.waitForTimeout(1300);
    await smoothScroll(m, 350, 20); await m.waitForTimeout(800);
  }, { maxWidth: 1170, maxHeight: 2532 });
  await still(m, '23-mobile-findings');
  for (const [path, name] of [['/audits', '24-mobile-audits'], ['/risks', '25-mobile-risks'], ['/evidence', '26-mobile-evidence']]) {
    await m.goto(`${WEB}${path}`); await m.waitForLoadState('networkidle'); await m.getByRole('heading', { level: 1 }).waitFor(); await m.waitForTimeout(600); await still(m, name);
  }
  await mctx.close();
  await browser.close();
}

const data = await seed();
await record(data);
console.log('capture complete');
