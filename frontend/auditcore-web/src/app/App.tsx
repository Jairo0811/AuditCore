import { Navigate, Route, Routes } from "react-router-dom";
import { LoginPage } from "../features/auth/pages/LoginPage";
import { DashboardPage } from "../features/dashboard/pages/DashboardPage";

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/dashboard" element={<DashboardPage />} />

      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}