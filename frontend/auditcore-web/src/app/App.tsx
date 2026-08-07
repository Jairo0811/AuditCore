import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "./AppShell";
import { ProtectedRoute } from "./ProtectedRoute";
import { LoginPage } from "../features/auth/pages/LoginPage";
import { DashboardPage } from "../features/dashboard/pages/DashboardPage";
import { ModulePlaceholderPage } from "../components/ModulePlaceholderPage";
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
        <Route path="/audits" element={<ModulePlaceholderPage title="Auditorías" description="Planificación, ejecución y seguimiento de auditorías." />} />
        <Route path="/risks" element={<ModulePlaceholderPage title="Riesgos" description="Registro, evaluación y tratamiento de riesgos." />} />
        <Route path="/findings" element={<ModulePlaceholderPage title="Hallazgos" description="Gestión de hallazgos, responsables y seguimiento." />} />
      </Route>

      <Route path="/" element={<Navigate to={isAuthenticated() ? "/dashboard" : "/login"} replace />} />
      <Route path="*" element={<Navigate to={isAuthenticated() ? "/dashboard" : "/login"} replace />} />
    </Routes>
  );
}
