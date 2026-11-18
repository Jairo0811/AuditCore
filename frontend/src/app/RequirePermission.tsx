import type { ReactNode } from "react";
import { hasAnyPermission, type PermissionCode } from "../features/auth/permissions";
import { AccessDeniedPage } from "./AccessDeniedPage";

interface RequirePermissionProps {
  permissions: PermissionCode[];
  children: ReactNode;
}

/**
 * Protege una ruta funcional por permiso. Sin permiso se muestra "Acceso denegado" en la misma URL,
 * sin redirecciones (no puede producir bucles). El backend sigue aplicando la autorización real.
 */
export function RequirePermission({ permissions, children }: RequirePermissionProps) {
  return hasAnyPermission(...permissions) ? children : <AccessDeniedPage />;
}
