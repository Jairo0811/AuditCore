interface ModulePlaceholderPageProps {
  title: string;
  description: string;
}

export function ModulePlaceholderPage({ title, description }: ModulePlaceholderPageProps) {
  return (
    <main className="dashboard-page">
      <header className="dashboard-header">
        <div>
          <p className="eyebrow">AUDITCORE</p>
          <h1>{title}</h1>
          <p>{description}</p>
        </div>
      </header>

      <section className="module-empty-state">
        <strong>Módulo en integración</strong>
        <p>La navegación y seguridad ya están conectadas. El CRUD de este módulo se implementará en la siguiente iteración del frontend.</p>
      </section>
    </main>
  );
}
