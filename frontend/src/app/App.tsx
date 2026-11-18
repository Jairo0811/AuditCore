import { lazy, Suspense, type ReactElement } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import { AccessDeniedPage } from "./AccessDeniedPage";
import { AppShell } from "./AppShell";
import { ACCESS_DENIED_PATH, getLandingPath, permissionsForPath } from "./navigation";
import { ProtectedRoute } from "./ProtectedRoute";
import { RequirePermission } from "./RequirePermission";
import { LoginPage } from "../features/auth/pages/LoginPage";
import { useSession } from "../features/auth/useSession";

// Las pantallas de módulo se cargan bajo demanda para reducir el bundle inicial.
const DashboardPage = lazy(() => import("../features/dashboard/pages/DashboardPage").then((module) => ({ default: module.DashboardPage })));
const AuditsPage = lazy(() => import("../features/audits/pages/AuditsPage").then((module) => ({ default: module.AuditsPage })));
const RisksPage = lazy(() => import("../features/risks/pages/RisksPage").then((module) => ({ default: module.RisksPage })));
const FindingsPage = lazy(() => import("../features/findings/pages/FindingsPage").then((module) => ({ default: module.FindingsPage })));
const EvidencePage = lazy(() => import("../features/evidence/pages/EvidencePage").then((module) => ({ default: module.EvidencePage })));
const ActionPlansPage = lazy(() => import("../features/actionPlans/pages/ActionPlansPage").then((module) => ({ default: module.ActionPlansPage })));
const FrameworksPage = lazy(() => import("../features/frameworks/pages/FrameworksPage").then((module) => ({ default: module.FrameworksPage })));
const AssessmentsPage = lazy(() => import("../features/assessments/pages/AssessmentsPage").then((module) => ({ default: module.AssessmentsPage })));
const OrganizationsPage = lazy(() => import("../features/organizations/pages/OrganizationsPage").then((module) => ({ default: module.OrganizationsPage })));
const BranchesPage = lazy(() => import("../features/branches/pages/BranchesPage").then((module) => ({ default: module.BranchesPage })));
const DepartmentsPage = lazy(() => import("../features/departments/pages/DepartmentsPage").then((module) => ({ default: module.DepartmentsPage })));
const UsersPage = lazy(() => import("../features/users/pages/UsersPage").then((module) => ({ default: module.UsersPage })));
const RolesPage = lazy(() => import("../features/roles/pages/RolesPage").then((module) => ({ default: module.RolesPage })));
const ReportsPage = lazy(() => import("../features/reports/pages/ReportsPage").then((module) => ({ default: module.ReportsPage })));

const modules: Array<[path: string, element: ReactElement]> = [
  ["/dashboard", <DashboardPage />],
  ["/audits", <AuditsPage />],
  ["/risks", <RisksPage />],
  ["/findings", <FindingsPage />],
  ["/evidence", <EvidencePage />],
  ["/action-plans", <ActionPlansPage />],
  ["/frameworks", <FrameworksPage />],
  ["/assessments", <AssessmentsPage />],
  ["/organizations", <OrganizationsPage />],
  ["/branches", <BranchesPage />],
  ["/departments", <DepartmentsPage />],
  ["/users", <UsersPage />],
  ["/roles", <RolesPage />],
  ["/reports", <ReportsPage />],
];

function HomeRedirect() {
  const session = useSession();
  return <Navigate to={session ? getLandingPath() : "/login"} replace />;
}

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
        {modules.map(([path, element]) => (
          <Route
            key={path}
            path={path}
            element={(
              <RequirePermission permissions={permissionsForPath(path)}>
                <Suspense fallback={<p className="panel-state" role="status">Cargando módulo...</p>}>{element}</Suspense>
              </RequirePermission>
            )}
          />
        ))}
        <Route path={ACCESS_DENIED_PATH} element={<AccessDeniedPage />} />
      </Route>

      <Route path="*" element={<HomeRedirect />} />
    </Routes>
  );
}
