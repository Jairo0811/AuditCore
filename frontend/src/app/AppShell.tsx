import { useQueryClient } from "@tanstack/react-query";
import { LogOut, Menu, X } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import { logout, verifySession } from "../features/auth/auth";
import { useSession } from "../features/auth/useSession";
import { useMediaQuery } from "../hooks/useMediaQuery";
import { getLandingPath, visibleNavigationGroups } from "./navigation";

const brandMark = "/assets/brand/auditcore-mark.png";

export function AppShell() {
  // El menú móvil se asocia a la ruta en la que se abrió: navegar lo cierra sin efectos adicionales.
  const [menuOpenPath, setMenuOpenPath] = useState<string | null>(null);
  const [loggingOut, setLoggingOut] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const menuOpen = menuOpenPath === location.pathname;
  const queryClient = useQueryClient();
  const session = useSession();
  const menuButtonRef = useRef<HTMLButtonElement>(null);
  const closeButtonRef = useRef<HTMLButtonElement>(null);
  // Mismo breakpoint que global.css: por debajo, la barra lateral es un panel deslizante.
  const isMobileNavigation = useMediaQuery("(max-width: 900px)");
  const sidebarHidden = isMobileNavigation && !menuOpen;
  const user = session?.user;
  const groups = visibleNavigationGroups();
  const landingPath = getLandingPath();

  // Comprueba la sesión almacenada contra la API (renueva o cierra la sesión si los tokens ya no son válidos).
  useEffect(() => {
    void verifySession();
  }, []);

  useEffect(() => {
    if (!menuOpen) return undefined;
    closeButtonRef.current?.focus();
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setMenuOpenPath(null);
        menuButtonRef.current?.focus();
      }
    };
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [menuOpen]);

  function closeMenu() {
    setMenuOpenPath(null);
    menuButtonRef.current?.focus();
  }

  async function handleLogout() {
    setLoggingOut(true);
    try {
      await logout();
    } finally {
      // Evita que datos de la sesión anterior queden en caché para el siguiente usuario de la pestaña.
      queryClient.clear();
      navigate("/login", { replace: true });
      setLoggingOut(false);
    }
  }

  const displayName = user?.fullName || user?.email || "Usuario";

  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">Saltar al contenido principal</a>
      <aside
        id="app-sidebar"
        className={`sidebar ${menuOpen ? "sidebar-open" : ""}`}
        aria-label="Menú de AuditCore"
        inert={sidebarHidden}
      >
        <div className="sidebar-header">
          <NavLink to={landingPath} className="sidebar-brand" aria-label="AuditCore - Ir al inicio">
            <img className="sidebar-brand-mark" src={brandMark} alt="" aria-hidden="true" />
            <span aria-hidden="true">AUDIT<strong>CORE</strong></span>
          </NavLink>
          <button ref={closeButtonRef} className="sidebar-close" type="button" aria-label="Cerrar menú de navegación" onClick={closeMenu}>
            <X size={20} aria-hidden="true" />
          </button>
        </div>

        <nav className="sidebar-nav" aria-label="Navegación principal">
          {groups.map((group, groupIndex) => (
            <div className="sidebar-nav-group" key={group.label ?? `primary-${groupIndex}`}>
              {group.label && <p className="sidebar-nav-label">{group.label}</p>}
              {group.items.map(({ to, label, icon: Icon }) => (
                <NavLink key={to} to={to} className={({ isActive }) => `sidebar-link ${isActive ? "active" : ""}`}>
                  <Icon size={18} aria-hidden="true" /><span>{label}</span>
                </NavLink>
              ))}
            </div>
          ))}
        </nav>

        <div className="sidebar-user">
          <div className="avatar" aria-hidden="true">{displayName.charAt(0).toUpperCase()}</div>
          <div>
            <strong>{displayName}</strong>
            <small>{user?.roles?.[0] ?? "AuditCore"}</small>
          </div>
        </div>
        <button className="logout-button" type="button" disabled={loggingOut} onClick={handleLogout}>
          <LogOut size={17} aria-hidden="true" />{loggingOut ? "Cerrando..." : "Cerrar sesión"}
        </button>
      </aside>

      {menuOpen && <div className="sidebar-backdrop" aria-hidden="true" onClick={closeMenu} />}

      <div className="app-content">
        <header className="mobile-topbar">
          <button
            ref={menuButtonRef}
            type="button"
            aria-label="Abrir menú de navegación"
            aria-expanded={menuOpen}
            aria-controls="app-sidebar"
            onClick={() => setMenuOpenPath(location.pathname)}
          >
            <Menu size={22} aria-hidden="true" />
          </button>
          <div className="mobile-brand">
            <img src={brandMark} alt="" aria-hidden="true" />
            <span>AUDIT<strong>CORE</strong></span>
          </div>
        </header>
        <main id="main-content" tabIndex={-1}><Outlet /></main>
      </div>
    </div>
  );
}
