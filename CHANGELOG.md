# Changelog

Todos los cambios relevantes de AuditCore se documentan en este archivo.

El proyecto sigue versionado semántico (`MAJOR.MINOR.PATCH`).

## [1.0.0] - 2026-08-11

### Añadido

- Backend en .NET 10 con Clean Architecture.
- API REST con ASP.NET Core.
- Persistencia mediante Entity Framework Core y SQL Server.
- Autenticación JWT con access tokens de corta duración y refresh tokens rotativos.
- RBAC basado en roles, permisos y políticas de autorización.
- Aislamiento multiempresa por organización.
- Gestión de organizaciones, sucursales y departamentos.
- Gestión de usuarios, roles y permisos.
- Módulo de auditorías con ciclo de vida completo.
- Gestión de riesgos, nivel de exposición y tratamiento.
- Gestión de hallazgos y flujo de revisión, aceptación, resolución y cierre.
- Evidencias con metadata, persistencia en almacenamiento dedicado, SHA-256 y validaciones de seguridad.
- Planes de acción con responsable, fecha límite, progreso, cierre y seguimiento.
- Motor de auditoría con marcos, controles, preguntas, evaluaciones y respuestas.
- Dashboard ejecutivo con indicadores agregados.
- Reportes de auditoría en CSV, XLSX y PDF.
- Frontend React 19 + TypeScript 6 + Vite.
- Formularios con React Hook Form y Zod.
- Navegación protegida y manejo automático de sesión/refresh token.
- Integración frontend-backend para los módulos funcionales principales.
- Health checks, rate limiting y manejo global de errores mediante Problem Details.
- Auditoría automática de creación y modificación de entidades.
- Pruebas unitarias y de integración de backend.
- Suite E2E con Playwright.
- Flujo E2E integral: login → auditoría → riesgo → hallazgo → evidencia → plan de acción → cierre → Dashboard → reporte.
- Dockerfile multietapa para la API.
- Dockerfile multietapa para React/Nginx.
- Docker Compose para SQL Server, API y frontend.
- Volúmenes persistentes para base de datos y evidencias.
- Configuración de producción y plantilla `.env.example`.
- GitHub Actions para Backend CI, Frontend CI, E2E CI, Security CI y Container CI.
- Auditoría automatizada de dependencias NuGet y npm.

### Seguridad

- Validación de issuer, audience, expiración y firma de JWT.
- Rotación y revocación de refresh tokens.
- Autorización basada en claims de permisos.
- Restricción de acceso entre organizaciones.
- Rate limiting en autenticación.
- Validación de tipo/tamaño de evidencias.
- Protección contra path traversal en almacenamiento de archivos.
- Configuración de secretos mediante variables de entorno.
- Actualización de React Router a una versión corregida frente a la alerta de seguridad detectada durante el hardening de v1.0.0.

### Calidad

- 57 pruebas automatizadas de backend validadas.
- Frontend validado con lint y build de producción.
- Suite E2E ejecutada contra SQL Server, API y frontend reales.
- Validación de migraciones y modelo de Entity Framework Core.
- Construcción validada de imágenes Docker para backend y frontend.
- Matriz final de GitHub Actions completamente verde antes del cierre de v1.0.0.

### Documentación

- README actualizado con arquitectura, stack, seguridad, Docker, E2E y estado de CI.
- Documentación de ejecución local y mediante Docker Compose.
- Changelog inicial del producto.

---

## Antecedentes

AuditCore surge como reconstrucción y evolución profesional de un trabajo académico asociado a **Auditoría Informática (SOF-009)** del Instituto Tecnológico de Las Américas (ITLA), período **2017-C2**.
