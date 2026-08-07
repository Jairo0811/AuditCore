import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "./AppShell";
import { ProtectedRoute } from "./ProtectedRoute";
import { LoginPage } from "../features/auth/pages/LoginPage";
import { DashboardPage } from "../features/dashboard/pages/DashboardPage";
import { AuditsPage } from "../features/audits/pages/AuditsPage";
import { RisksPage } from "../features/risks/pages/RisksPage";
import { FindingsPage } from "../features/findings/pages/FindingsPage";
import { EvidencePage } from "../features/evidence/pages/EvidencePage";
import { ActionPlansPage } from "../features/actionPlans/pages/ActionPlansPage";
import { FrameworksPage } from "../features/frameworks/pages/FrameworksPage";
import { OrganizationsPage } from "../features/organizations/pages/OrganizationsPage";
import { UsersPage } from "../features/users/pages/UsersPage";
import { RolesPage } from "../features/roles/pages/RolesPage";
import { ReportsPage } from "../features/reports/pages/ReportsPage";
import { isAuthenticated } from "../features/auth/auth";

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route
        element={(
          <ProtectedRoute>
            <AppShell />
          </ProtectedRoute>
        )}
      >
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/audits" element={<AuditsPage />} />
        <Route path="/risks" element={<RisksPage />} />
        <Route path="/findings" element={<FindingsPage />} />
        <Route path="/evidence" element={<EvidencePage />} />
        <Route path="/action-plans" element={<ActionPlansPage />} />
        <Route path="/frameworks" element={<FrameworksPage />} />
        <Route path="/organizations" element={<OrganizationsPage />} />
        <Route path="/users" element={<UsersPage />} />
        <Route path="/roles" element={<RolesPage />} />
        <Route path="/reports" element={<ReportsPage />} />
      </Route>

      <Route path="/" element={<Navigate to={isAuthenticated() ? "/dashboard" : "/login"} replace />} />
      <Route path="*" element={<Navigate to={isAuthenticated() ? "/dashboard" : "/login"} replace />} />
    </Routes>
  );
}
