import { getSession } from "./session";

/**
 * Códigos de permiso publicados por la API (AuditCore.Application.Common.Security.PermissionCodes).
 * Centralizarlos evita strings dispersos; el backend sigue siendo la fuente de verdad de la autorización.
 */
export const Permissions = {
  OrganizationsView: "ORGANIZATIONS.VIEW",
  OrganizationsManage: "ORGANIZATIONS.MANAGE",
  UsersView: "USERS.VIEW",
  UsersManage: "USERS.MANAGE",
  RolesView: "ROLES.VIEW",
  RolesManage: "ROLES.MANAGE",
  AuditsView: "AUDITS.VIEW",
  AuditsCreate: "AUDITS.CREATE",
  AuditsUpdate: "AUDITS.UPDATE",
  AuditsExecute: "AUDITS.EXECUTE",
  AuditsClose: "AUDITS.CLOSE",
  FrameworksView: "FRAMEWORKS.VIEW",
  FrameworksManage: "FRAMEWORKS.MANAGE",
  EvidenceView: "EVIDENCE.VIEW",
  EvidenceManage: "EVIDENCE.MANAGE",
  FindingsView: "FINDINGS.VIEW",
  FindingsManage: "FINDINGS.MANAGE",
  RisksView: "RISKS.VIEW",
  RisksManage: "RISKS.MANAGE",
  ActionPlansView: "ACTION_PLANS.VIEW",
  ActionPlansManage: "ACTION_PLANS.MANAGE",
  ReportsView: "REPORTS.VIEW",
  ReportsExport: "REPORTS.EXPORT",
} as const;

export type PermissionCode = (typeof Permissions)[keyof typeof Permissions];

export const Roles = {
  SuperAdmin: "SUPER_ADMIN",
} as const;

/** Permisos que habilitan la consulta mínima de usuarios (GET /users/lookup). Refleja AuthorizationPolicies.UserLookup. */
export const userLookupPermissions: PermissionCode[] = [
  Permissions.UsersView,
  Permissions.AuditsUpdate,
  Permissions.AuditsExecute,
  Permissions.RisksManage,
  Permissions.FindingsManage,
  Permissions.ActionPlansManage,
];

function currentPermissions() {
  return getSession()?.user.permissions ?? [];
}

export function hasPermission(permission: PermissionCode) {
  return currentPermissions().includes(permission);
}

export function hasAnyPermission(...permissions: PermissionCode[]) {
  if (permissions.length === 0) return true;
  const granted = currentPermissions();
  return permissions.some((permission) => granted.includes(permission));
}

export function hasAllPermissions(...permissions: PermissionCode[]) {
  const granted = currentPermissions();
  return permissions.every((permission) => granted.includes(permission));
}

export function hasRole(role: string) {
  return getSession()?.user.roles.includes(role) ?? false;
}

export function isSuperAdmin() {
  return hasRole(Roles.SuperAdmin);
}
