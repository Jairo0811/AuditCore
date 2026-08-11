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
import { useState } from "react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { getCurrentUser, logout } from "../features/auth/auth";

type NavigationItem = {
  to: string;
  label: string;
  icon: typeof BarChart3;
};

type NavigationGroup = {
  label?: string;
  items: NavigationItem[];
};

const navigationGroups: NavigationGroup[] = [
  {
    items: [{ to: "/dashboard", label: "Dashboard", icon: BarChart3 }],
  },
  {
    label: "AUDITORÍA",
    items: [
      { to: "/audits", label: "Auditorías", icon: ClipboardCheck },
      { to: "/risks", label: "Riesgos", icon: ShieldAlert },
      { to: "/findings", label: "Hallazgos", icon: FileSearch },
      { to: "/evidence", label: "Evidencias", icon: FileArchive },
      { to: "/action-plans", label: "Planes de acción", icon: ListChecks },
    ],
  },
  {
    label: "CUMPLIMIENTO",
    items: [
      { to: "/frameworks", label: "Marcos y controles", icon: ShieldCheck },
      { to: "/assessments", label: "Evaluaciones", icon: Network },
      { to: "/reports", label: "Reportes", icon: FileText },
    ],
  },
  {
    label: "ADMINISTRACIÓN",
    items: [
      { to: "/organizations", label: "Organizaciones", icon: Building2 },
      { to: "/branches", label: "Sucursales", icon: GitBranch },
      { to: "/departments", label: "Departamentos", icon: Network },
      { to: "/users", label: "Usuarios", icon: Users },
      { to: "/roles", label: "Roles", icon: ShieldCheck },
    ],
  },
];

const brandMark = "/assets/brand/auditcore-mark.png";

export function AppShell() {
  const [menuOpen, setMenuOpen] = useState(false);
  const [loggingOut, setLoggingOut] = useState(false);
  const navigate = useNavigate();
  const user = getCurrentUser();

  async function handleLogout() {
    setLoggingOut(true);

    try {
      await logout();
    } finally {
      navigate("/login", { replace: true });
      setLoggingOut(false);
    }
  }

  return (
    <div className="app-shell">
      <aside className={`sidebar ${menuOpen ? "sidebar-open" : ""}`}>
        <div className="sidebar-header">
          <NavLink to="/dashboard" className="sidebar-brand" onClick={() => setMenuOpen(false)}>
            <img className="sidebar-brand-mark" src={brandMark} alt="" aria-hidden="true" />
            <span>AUDIT<strong>CORE</strong></span>
          </NavLink>

          <button className="sidebar-close" type="button" aria-label="Cerrar menú" onClick={() => setMenuOpen(false)}>
            <X size={20} />
          </button>
        </div>

        <nav className="sidebar-nav" aria-label="Navegación principal">
          {navigationGroups.map((group, groupIndex) => (
            <div className="sidebar-nav-group" key={group.label ?? `primary-${groupIndex}`}>
              {group.label && <p className="sidebar-nav-label">{group.label}</p>}
              {group.items.map(({ to, label, icon: Icon }) => (
                <NavLink
                  key={to}
                  to={to}
                  onClick={() => setMenuOpen(false)}
                  className={({ isActive }) => `sidebar-link ${isActive ? "active" : ""}`}
                >
                  <Icon size={18} />
                  <span>{label}</span>
                </NavLink>
              ))}
            </div>
          ))}
        </nav>

        <div className="sidebar-user">
          <div className="avatar" aria-hidden="true">
            {(user?.fullName || user?.email || "A").charAt(0).toUpperCase()}
          </div>
          <div>
            <strong>{user?.fullName ?? "Usuario"}</strong>
            <small>{user?.roles?.[0] ?? "AuditCore"}</small>
          </div>
        </div>

        <button className="logout-button" type="button" disabled={loggingOut} onClick={handleLogout}>
          <LogOut size={17} />
          {loggingOut ? "Cerrando..." : "Cerrar sesión"}
        </button>
      </aside>

      {menuOpen && <button className="sidebar-backdrop" aria-label="Cerrar menú" onClick={() => setMenuOpen(false)} />}

      <div className="app-content">
        <header className="mobile-topbar">
          <button type="button" aria-label="Abrir menú" onClick={() => setMenuOpen(true)}>
            <Menu size={22} />
          </button>
          <div className="mobile-brand">
            <img src={brandMark} alt="" aria-hidden="true" />
            <span>AUDIT<strong>CORE</strong></span>
          </div>
        </header>
        <Outlet />
      </div>
    </div>
  );
}
