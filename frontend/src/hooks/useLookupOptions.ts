import { useQuery } from "@tanstack/react-query";
import { hasAnyPermission, Permissions, userLookupPermissions, type PermissionCode } from "../features/auth/permissions";
import { apiClient } from "../services/apiClient";

export interface LookupOption {
  label: string;
  value: string;
}

/**
 * Carga un catálogo para selectores. Si se indican permisos y el usuario no posee ninguno,
 * la consulta no se ejecuta (evita 403 previsibles) y el catálogo queda vacío.
 */
export function useLookupOptions<T extends { id: string }>(
  queryKey: string,
  endpoint: string,
  getLabel: (item: T) => string,
  requiredPermissions: PermissionCode[] = [],
) {
  const allowed = hasAnyPermission(...requiredPermissions);
  const query = useQuery({
    queryKey: [queryKey],
    enabled: allowed,
    queryFn: async () => (await apiClient.get<T[]>(endpoint)).data,
  });

  const records = query.data ?? [];
  const options: LookupOption[] = records.map((item) => ({
    label: getLabel(item),
    value: item.id,
  }));

  return {
    ...query,
    allowed,
    records,
    options,
  };
}

export interface UserLookup {
  id: string;
  organizationId: string;
  fullName: string;
  email: string;
}

/** Usuarios seleccionables como responsables/evaluadores (GET /users/lookup, política USERS.LOOKUP). */
export function useUserLookup() {
  return useLookupOptions<UserLookup>(
    "user-lookup",
    "/users/lookup",
    (item) => `${item.fullName} — ${item.email}`,
    userLookupPermissions,
  );
}

export interface OrganizationLookup {
  id: string;
  code: string;
  name: string;
}

export function useOrganizationLookup() {
  return useLookupOptions<OrganizationLookup>(
    "organizations",
    "/organizations",
    (item) => `${item.code} — ${item.name}`,
    [Permissions.OrganizationsView],
  );
}
