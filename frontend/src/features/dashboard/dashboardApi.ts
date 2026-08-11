import { apiClient } from "../../services/apiClient";

export interface DashboardDto {
  totalAudits: number;
  openAudits: number;
  closedAudits: number;
  totalRisks: number;
  criticalRisks: number;
  totalFindings: number;
  openFindings: number;
  overdueActionPlans: number;
  averageComplianceScore: number;
}

export async function getDashboard() {
  const { data } = await apiClient.get<DashboardDto>("/reports/dashboard");
  return data;
}
