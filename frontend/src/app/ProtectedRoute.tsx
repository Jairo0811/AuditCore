import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useSession } from "../features/auth/useSession";

interface ProtectedRouteProps {
  children: ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const location = useLocation();
  const session = useSession();

  if (!session) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return children;
}
