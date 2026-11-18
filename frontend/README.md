# AuditCore — Frontend

SPA de AuditCore v1.2.0 construida con React 19, TypeScript 6 y Vite. Consume exclusivamente la API HTTP de AuditCore.

## Stack

- React 19 + TypeScript 6 + Vite
- React Router (rutas protegidas por sesión y por permiso; módulos cargados bajo demanda)
- TanStack Query (estado remoto) y Axios (`src/services/apiClient.ts`, con renovación automática del token)
- React Hook Form + Zod (login)
- lucide-react (iconografía)

## Estructura

```text
src/
├── app/          # App, rutas, AppShell, navegación por permisos y acceso denegado
├── components/   # ResourceManager, campos de formulario, Modal, confirmaciones y feedback
├── features/     # Pantallas por módulo y autenticación (sesión, permisos)
├── hooks/        # Catálogos para selectores y media queries
├── lib/          # Errores de API, descargas, formatos y etiquetas de dominio
├── services/     # Cliente HTTP
└── styles/       # Tema oscuro de AuditCore
```

## Autorización en la interfaz

Los códigos de permiso de la API están centralizados en `src/features/auth/permissions.ts` (`hasPermission`, `hasAnyPermission`, `hasRole`). La navegación, las rutas y las acciones se muestran únicamente cuando la sesión posee el permiso correspondiente; una ruta abierta manualmente sin permiso muestra «Acceso denegado». El backend sigue siendo la única fuente de verdad de la autorización.

## Comandos

```bash
npm ci
npm run dev      # http://localhost:5173 (proxy de /api hacia http://127.0.0.1:5047)
npm run lint
npm run build
```

En Docker y CI la URL de la API se define con `VITE_API_URL` en tiempo de compilación.
