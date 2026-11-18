import { useSyncExternalStore } from "react";
import { getSession, getSessionVersion, subscribeSession } from "./session";

/** Sesión actual con re-render automático cuando cambia (login, refresh, logout u otra pestaña). */
export function useSession() {
  useSyncExternalStore(subscribeSession, getSessionVersion, getSessionVersion);
  return getSession();
}
