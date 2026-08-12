import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../services/apiClient";

export interface LookupOption {
  label: string;
  value: string;
}

export function useLookupOptions<T extends { id: string }>(
  queryKey: string,
  endpoint: string,
  getLabel: (item: T) => string,
  enabled = true,
) {
  const query = useQuery({
    queryKey: [queryKey],
    enabled,
    queryFn: async () => (await apiClient.get<T[]>(endpoint)).data,
  });

  const records = query.data ?? [];
  const options: LookupOption[] = records.map((item) => ({
    label: getLabel(item),
    value: item.id,
  }));

  return {
    ...query,
    records,
    options,
  };
}
