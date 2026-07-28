import { useNavigate } from "react-router-dom";

export function LoginPage() {
  const navigate = useNavigate();

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    navigate("/dashboard");
  }

  return (
    <main className="login-page">
      <section className="login-brand">
        <div className="brand-symbol">✓</div>

        <p className="eyebrow">
          IT AUDIT & COMPLIANCE PLATFORM
        </p>

        <h1>
          AUDIT<span>CORE</span>
        </h1>

        <p className="brand-description">
          Gestiona auditorías, riesgos, hallazgos, evidencias y cumplimiento
          desde una sola plataforma empresarial.
        </p>
      </section>

      <section className="login-panel">
        <form className="login-form" onSubmit={handleSubmit}>
          <div>
            <p className="eyebrow">BIENVENIDO</p>
            <h2>Iniciar sesión</h2>
            <p>Accede al espacio de trabajo de tu organización.</p>
          </div>

          <label>
            Correo electrónico
            <input
              type="email"
              placeholder="usuario@empresa.com"
              required
            />
          </label>

          <label>
            Contraseña
            <input
              type="password"
              placeholder="••••••••"
              required
            />
          </label>

          <button type="submit">
            Iniciar sesión
          </button>
        </form>
      </section>
    </main>
  );
}