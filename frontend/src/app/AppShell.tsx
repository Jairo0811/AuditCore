import {
  BarChart3,
  Building2,
  ClipboardCheck,
  FileArchive,
  FileSearch,
  FileText,
  GitBranch,
  ListChecks,
  LogOut,
  Menu,
  Network,
  ShieldAlert,
  ShieldCheck,
  Users,
  X,
} from "lucide-react";
import { useEffect, useState } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { getCurrentUser, logout } from "../features/auth/auth";

type NavigationItem = { to: string; label: string; icon: typeof BarChart3 };
type NavigationGroup = { label?: string; items: NavigationItem[] };

const navigationGroups: NavigationGroup[] = [
  { items: [{ to: "/dashboard", label: "Dashboard", icon: BarChart3 }] },
  { label: "AUDITORÍA", items: [
    { to: "/audits", label: "Auditorías", icon: ClipboardCheck },
    { to: "/risks", label: "Riesgos", icon: ShieldAlert },
    { to: "/findings", label: "Hallazgos", icon: FileSearch },
    { to: "/evidence", label: "Evidencias", icon: FileArchive },
    { to: "/action-plans", label: "Planes de acción", icon: ListChecks },
  ]},
  { label: "CUMPLIMIENTO", items: [
    { to: "/frameworks", label: "Marcos y controles", icon: ShieldCheck },
    { to: "/assessments", label: "Evaluaciones", icon: Network },
    { to: "/reports", label: "Reportes", icon: FileText },
  ]},
  { label: "ADMINISTRACIÓN", items: [
    { to: "/organizations", label: "Organizaciones", icon: Building2 },
    { to: "/branches", label: "Sucursales", icon: GitBranch },
    { to: "/departments", label: "Departamentos", icon: Network },
    { to: "/users", label: "Usuarios", icon: Users },
    { to: "/roles", label: "Roles", icon: ShieldCheck },
  ]},
];

const brandMark = "/assets/brand/auditcore-mark.png";

export function AppShell() {
  const [menuOpen, setMenuOpen] = useState(false);
  const [loggingOut, setLoggingOut] = useState(false);
  const navigate = useNavigate();
  const user = getCurrentUser();

  useEffect(() => {
    if (!menuOpen) return;
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") setMenuOpen(false);
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [menuOpen]);

  async function handleLogout() {
    setLoggingOut(true);
    try { await logout(); }
    finally {
      navigate("/login", { replace: true });
      setLoggingOut(false);
    }
  }

  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">Saltar al contenido principal</a>
      <aside className={`sidebar ${menuOpen ? "sidebar-open" : ""}`} aria-label="Menú de AuditCore">
        <div className="sidebar-header">
          <NavLink to="/dashboard" className="sidebar-brand" aria-label="AuditCore - Ir al dashboard" onClick={() => setMenuOpen(false)}>
            <img className="sidebar-brand-mark" src={brandMark} alt="" aria-hidden="true" />
            <span>AUDIT<strong>CORE</strong></span>
          </NavLink>
          <button className="sidebar-close" type="button" aria-label="Cerrar menú de navegación" onClick={() => setMenuOpen(false)}><X size={20} aria-hidden="true" /></button>
        </div>

        <nav className="sidebar-nav" aria-label="Navegación principal">
          {navigationGroups.map((group, groupIndex) => (
            <div className="sidebar-nav-group" key={group.label ?? `primary-${groupIndex}`}>
              {group.label && <p className="sidebar-nav-label">{group.label}</p>}
              {group.items.map(({ to, label, icon: Icon }) => (
                <NavLink key={to} to={to} onClick={() => setMenuOpen(false)} className={({ isActive }) => `sidebar-link ${isActive ? "active" : ""}`}>
                  <Icon size={18} aria-hidden="true" /><span>{label}</span>
                </NavLink>
              ))}
            </div>
          ))}
        </nav>

        <div className="sidebar-user" aria-label={`Sesión de ${user?.fullName ?? user?.email ?? "usuario"}`}>
          <div className="avatar" aria-hidden="true">{(user?.fullName || user?.email || "A").charAt(0).toUpperCase()}</div>
          <div><strong>{user?.fullName ?? "Usuario"}</strong><small>{user?.roles?.[0] ?? "AuditCore"}</small></div>
        </div>
        <button className="logout-button" type="button" disabled={loggingOut} onClick={handleLogout}><LogOut size={17} aria-hidden="true" />{loggingOut ? "Cerrando..." : "Cerrar sesión"}</button>
      </aside>

      {menuOpen && <button className="sidebar-backdrop" type="button" aria-label="Cerrar menú de navegación" onClick={() => setMenuOpen(false)} />}

      <div className="app-content">
        <header className="mobile-topbar">
          <button type="button" aria-label="Abrir menú de navegación" aria-expanded={menuOpen} onClick={() => setMenuOpen(true)}><Menu size={22} aria-hidden="true" /></button>
          <div className="mobile-brand" aria-label="AuditCore"><img src={brandMark} alt="" aria-hidden="true" /><span>AUDIT<strong>CORE</strong></span></div>
        </header>
        <main id="main-content" tabIndex={-1}><Outlet /></main>
      </div>
    </div>
  );
}
