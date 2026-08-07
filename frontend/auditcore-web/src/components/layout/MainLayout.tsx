import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";

interface MainLayoutProps {
  children: ReactNode;
}

const navigationItems = [
  { label: "Dashboard", path: "/dashboard" },
  { label: "Organizaciones", path: "/organizations" },
  { label: "Auditorías", path: "/audits" },
  { label: "Hallazgos", path: "/findings" },
  { label: "Riesgos", path: "/risks" },
  { label: "Reportes", path: "/reports" },
];

export function MainLayout({ children }: MainLayoutProps) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <span className="sidebar-brand-mark">AC</span>
          <span>AuditCore</span>
        </div>

        <nav className="sidebar-navigation" aria-label="Navegación principal">
          {navigationItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                isActive ? "sidebar-link sidebar-link-active" : "sidebar-link"
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="app-content">
        <header className="app-header">
          <div>
            <strong>AuditCore</strong>
            <span>Audita. Evalúa. Protege.</span>
          </div>
        </header>

        <main className="page-content">{children}</main>
      </div>
    </div>
  );
}
