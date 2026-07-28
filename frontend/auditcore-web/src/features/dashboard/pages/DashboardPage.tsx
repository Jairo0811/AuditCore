export function DashboardPage() {
  return (
    <main className="dashboard-page">
      <header className="dashboard-header">
        <div>
          <p className="eyebrow">VISIÓN EJECUTIVA</p>
          <h1>Dashboard</h1>
          <p>
            Supervisa auditorías, riesgos, hallazgos y cumplimiento.
          </p>
        </div>

        <button type="button">
          Nueva auditoría
        </button>
      </header>

      <section className="stats-grid">
        <article className="stat-card">
          <span>Auditorías activas</span>
          <strong>12</strong>
          <small>+2 este mes</small>
        </article>

        <article className="stat-card">
          <span>Cumplimiento promedio</span>
          <strong>74%</strong>
          <small>+6% respecto al trimestre</small>
        </article>

        <article className="stat-card">
          <span>Riesgos altos</span>
          <strong>8</strong>
          <small>3 requieren atención</small>
        </article>

        <article className="stat-card">
          <span>Hallazgos críticos</span>
          <strong>4</strong>
          <small>2 vencidos</small>
        </article>
      </section>
    </main>
  );
}