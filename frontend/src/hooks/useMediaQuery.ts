import { useSyncExternalStore } from "react";

/** Estado reactivo de una media query CSS (p. ej. el breakpoint de navegación móvil). */
export function useMediaQuery(query: string) {
  return useSyncExternalStore(
    (onChange) => {
      const media = window.matchMedia(query);
      media.addEventListener("change", onChange);
      return () => media.removeEventListener("change", onChange);
    },
    () => window.matchMedia(query).matches,
    () => false,
  );
}
