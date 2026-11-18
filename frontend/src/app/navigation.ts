import {
  BarChart3,
  Building2,
  ClipboardCheck,
  FileArchive,
  FileSearch,
  FileText,
  GitBranch,
  ListChecks,
  Network,
  ShieldAlert,
  ShieldCheck,
  Users,
  type LucideIcon,
} from "lucide-react";
import { hasAnyPermission, Permissions, type PermissionCode } from "../features/auth/permissions";

export interface NavigationItem {
  to: string;
  label: string;
  icon: LucideIcon;
  /** Basta con uno de estos permisos para consultar el módulo (refleja la política GET del endpoint). */
  permissions: PermissionCode[];
}

export interface NavigationGroup {
  label?: string;
  items: NavigationItem[];
}

export const navigationGroups: NavigationGroup[] = [
  { items: [{ to: "/dashboard", label: "Dashboard", icon: BarChart3, permissions: [Permissions.ReportsView] }] },
  {
    label: "AUDITORÍA",
    items: [
      { to: "/audits", label: "Auditorías", icon: ClipboardCheck, permissions: [Permissions.AuditsView] },
      { to: "/risks", label: "Riesgos", icon: ShieldAlert, permissions: [Permissions.RisksView] },
      { to: "/findings", label: "Hallazgos", icon: FileSearch, permissions: [Permissions.FindingsView] },
      { to: "/evidence", label: "Evidencias", icon: FileArchive, permissions: [Permissions.EvidenceView] },
      { to: "/action-plans", label: "Planes de acción", icon: ListChecks, permissions: [Permissions.ActionPlansView] },
    ],
  },
  {
    label: "CUMPLIMIENTO",
    items: [
      { to: "/frameworks", label: "Marcos y controles", icon: ShieldCheck, permissions: [Permissions.FrameworksView] },
      { to: "/assessments", label: "Evaluaciones", icon: Network, permissions: [Permissions.FrameworksView] },
      // La única función del módulo es exportar; sin REPORTS.EXPORT sería una pantalla sin acciones.
      { to: "/reports", label: "Reportes", icon: FileText, permissions: [Permissions.ReportsExport] },
    ],
  },
  {
    label: "ADMINISTRACIÓN",
    items: [
      { to: "/organizations", label: "Organizaciones", icon: Building2, permissions: [Permissions.OrganizationsView] },
      { to: "/branches", label: "Sucursales", icon: GitBranch, permissions: [Permissions.OrganizationsView] },
      { to: "/departments", label: "Departamentos", icon: Network, permissions: [Permissions.OrganizationsView] },
      { to: "/users", label: "Usuarios", icon: Users, permissions: [Permissions.UsersView] },
      { to: "/roles", label: "Roles", icon: ShieldCheck, permissions: [Permissions.RolesView] },
    ],
  },
];

const allItems = navigationGroups.flatMap((group) => group.items);

export const ACCESS_DENIED_PATH = "/access-denied";

export function canAccess(item: NavigationItem) {
  return hasAnyPermission(...item.permissions);
}

export function permissionsForPath(path: string): PermissionCode[] {
  return allItems.find((item) => item.to === path)?.permissions ?? [];
}

/** Grupos de navegación filtrados a los módulos que el usuario puede consultar. */
export function visibleNavigationGroups(): NavigationGroup[] {
  return navigationGroups
    .map((group) => ({ ...group, items: group.items.filter(canAccess) }))
    .filter((group) => group.items.length > 0);
}

/** Primera pantalla permitida; si el usuario no puede consultar ningún módulo, la página de acceso denegado. */
export function getLandingPath() {
  return allItems.find(canAccess)?.to ?? ACCESS_DENIED_PATH;
}
