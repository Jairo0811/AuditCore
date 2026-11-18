import { ShieldOff } from "lucide-react";
import { Link } from "react-router-dom";
import { ACCESS_DENIED_PATH, getLandingPath } from "./navigation";

export function AccessDeniedPage() {
  const landing = getLandingPath();
  const hasAlternative = landing !== ACCESS_DENIED_PATH;

  return (
    <section className="module-page access-denied" aria-labelledby="access-denied-title">
      <div className="access-denied-card">
        <ShieldOff size={36} aria-hidden="true" />
        <p className="eyebrow">ACCESO RESTRINGIDO</p>
        <h1 id="access-denied-title">Acceso denegado</h1>
        <p>
          {hasAlternative
            ? "Tu rol no tiene permisos para consultar esta sección. Si necesitas acceso, solicítalo al administrador de tu organización."
            : "Tu cuenta no tiene permisos asignados para ningún módulo de AuditCore. Solicita acceso al administrador de tu organización."}
        </p>
        {hasAlternative && <Link className="primary-link" to={landing}>Ir a una sección disponible</Link>}
      </div>
    </section>
  );
}
