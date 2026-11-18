# Changelog

Todos los cambios relevantes de AuditCore se documentan en este archivo.

El proyecto sigue versionado semántico (`MAJOR.MINOR.PATCH`).

## [1.2.0] - 2026-11-18

Versión final de portafolio: pase de hardening, corrección y pulido sin ampliar el alcance funcional. Con esta versión AuditCore queda congelada.

### Fixed

- Eliminar una evidencia la desactiva y la marca como eliminada (`Deactivate()` + `SoftDelete()`); después del `DELETE` ya no aparece en el listado y el detalle, la descarga y un segundo `DELETE` responden 404. Antes la metadata seguía visible apuntando a un archivo ya borrado.
- Al abrir «Permisos» de un rol, la selección ahora refleja los permisos ya asignados (los códigos se convierten a IDs). Antes comenzaba vacía y guardar reemplazaba la matriz completa.
- Los roles de un usuario pueden administrarse después de crearlo mediante la acción «Roles», con los roles actuales precargados (`PUT /users/{id}/roles`).
- Eliminar una sucursal o un departamento con usuarios asignados (o una sucursal con departamentos) devuelve un conflicto de negocio claro (409) en lugar de un error SQL genérico.
- Las acciones de fila solo se ofrecen en los estados que el dominio acepta (por ejemplo, «Planificar» solo en borrador; transiciones de riesgos, hallazgos y planes de acción según su estado), evitando errores previsibles.
- Las pantallas autenticadas ya no anidan elementos `<main>`; existe un único landmark principal y un único `h1` por pantalla.
- Entrar a `/login` con una sesión válida redirige a la aplicación en lugar de mostrar una pantalla vacía; un token corrupto descarta la sesión y envía al login sin bucles.
- `GET /auth/me` participa en la renovación automática del token (antes cualquier ruta `/auth/` quedaba excluida del refresh).
- Compilación de las pruebas de aislamiento multiempresa (conflicto entre el tipo `Evidence` y el espacio de nombres de pruebas).

### Improved

- RBAC reflejado en la interfaz: códigos de permiso centralizados (`hasPermission`, `hasAnyPermission`, `hasRole`), navegación filtrada por permisos de consulta, rutas protegidas con página accesible de «Acceso denegado» y botones de crear, editar, eliminar, cambiar estado, subir o exportar visibles solo con el permiso correspondiente. El backend sigue siendo la fuente de verdad.
- Los catálogos de los selectores solo se consultan cuando el usuario tiene permiso; los responsables y evaluadores se cargan desde `GET /users/lookup`. Sin `ORGANIZATIONS.VIEW`, auditorías y usuarios se crean en la organización de la sesión.
- Un administrador de organización no ve acciones sobre cuentas `SUPER_ADMIN` ni puede ofrecer ese rol; crear y eliminar organizaciones solo se muestra al `SUPER_ADMIN`.
- `ResourceManager`: capacidades declarativas (`canCreate`, `canUpdate`, `canDelete`, permiso por acción, valores iniciales de acciones), estados de carga, error con reintento y vacío, y mensajes de resultado. Se extrajeron `ResourceFieldInput` y los tipos compartidos.
- Evidencias y Reportes: estados explícitos de carga, subida, descarga, eliminación y error (sin `alert()`), nombre de archivo del servidor vía `Content-Disposition` y liberación de object URLs.
- La sesión se valida contra `/auth/me` al entrar, se sincroniza entre pestañas y la caché de consultas se limpia al cerrar sesión.
- Login móvil con la identidad compacta de AuditCore visible cuando el panel visual se oculta.
- Carga diferida por módulo: el bundle inicial bajó de 518 kB a 445 kB y desaparece la advertencia de tamaño de Vite.
- Recursos de marca optimizados sin cambiar su apariencia: logo del login en WebP (1,9 MB → 0,3 MB), marca de la barra lateral (1,9 MB → 30 kB), favicon (1,7 MB → 50 kB), logo de reportes PDF (1,9 MB → 0,4 MB) y logo del README (1,7 MB → 0,18 MB).

### Accessibility

- Componente `Modal` común: `role="dialog"`/`alertdialog`, `aria-modal`, título y descripción asociados, foco inicial, focus trap, cierre con Escape, fondo inerte, bloqueo de scroll y restauración del foco. Aplicado a formularios y acciones de `ResourceManager`, preguntas y evaluaciones.
- Confirmaciones destructivas (eliminar, cancelar, cerrar, desactivar, bloquear) con un diálogo accesible cuyo foco inicial está en la opción segura; se elimina `window.confirm`.
- Tablas con `scope="col"` y `caption`; botones de solo icono con nombre accesible; iconos decorativos con `aria-hidden`; `aria-busy` en cargas; errores con `role="alert"` y resultados en una región `aria-live`.
- Menú móvil: el foco entra al panel al abrirlo, vuelve al botón al cerrarlo y el panel cerrado queda fuera del orden de tabulación (`inert`).

### Security

- La autoría de una evidencia (`UploadedByUserId`) se toma del usuario autenticado; un `uploadedByUserId` enviado por el cliente se ignora.
- Validación de evidencias por extensión permitida, coherencia con el MIME declarado y firma del contenido; ruta de almacenamiento resuelta con `Path.GetRelativePath` (una ruta como `<raíz>-otra-cosa` ya no puede considerarse descendiente).
- Un actor restringido a su organización no puede modificar cuentas `SUPER_ADMIN` (perfil, contraseña, roles, activación o bloqueo) ni asignar ese rol.
- Política mínima de contraseñas aplicada por el backend al crear usuarios y cambiar contraseñas: 10 a 128 caracteres con al menos una letra y un número.
- `DbUpdateException` se traduce a 409 solo para violaciones de integridad conocidas (únicos y claves foráneas) con un mensaje seguro; el resto sigue siendo 500 y nunca se expone el mensaje de SQL Server.
- La API expone `Content-Disposition` por CORS para que el frontend use el nombre de archivo del servidor.
- Dependencias corregidas frente a avisos de severidad alta: axios 1.21.1 (incluye follow-redirects 1.16.3), zod 4.6.7, @playwright/test 1.55.1 y System.Security.Cryptography.Pkcs 10.0.13 (transitiva de SQL Server, antes 9.0.4).
- Nginx mantiene las cabeceras de seguridad también en los recursos estáticos.

### Tests

- Backend: 178 pruebas (37 de dominio, 14 de aplicación y 127 de integración). Nuevas pruebas de ciclo de vida de evidencias, aislamiento multiempresa (IDOR/BOLA), autorización de endpoints críticos, protección de `SUPER_ADMIN`, eliminación de sucursales y departamentos con usuarios, política de contraseñas, permisos de roles, refresh token y mapeo de errores.
- E2E: nuevas pruebas de un único `main` por pantalla, foco y cierre de modales, navegación móvil, visibilidad y 403 para `VIEWER`, precarga de permisos de roles, gestión de roles de usuarios, ciclo de evidencias desde la interfaz (404 posterior y autoría) y smoke responsive en escritorio y móvil.
- E2E reproducible con `e2e/package-lock.json` y `npm ci`; el ciclo completo ya no envía campos que la API no utiliza (`code`, `uploadedByUserId`).
- Container CI levanta el stack completo con Docker Compose y verifica salud, login, dashboard, escritura y persistencia de evidencias.

### Documentation

- README y CHANGELOG actualizados al estado final de v1.2.0.

### Cleanup

- Eliminados `WeatherForecast`, `httpClient.ts` (cliente Axios duplicado sin consumidores), `MainLayout.tsx`, `ModulePlaceholderPage.tsx`, recursos de plantilla de Vite (`vite.svg`, `hero.png`, `favicon.svg`, `icons.svg`) y la dependencia `recharts`, sin usos.
- Versión 1.2.0 alineada en frontend, E2E y ensamblados del backend (`Directory.Build.props`).
- Workflows sin disparadores de ramas obsoletas; versión de `dotnet-ef` alineada; el job E2E se fija a `ubuntu-24.04`, compatible con Playwright 1.55.

## [1.1.0] - 2026-08-12

### Añadido

- Asociación opcional de usuarios con sucursal y departamento dentro de su organización.
- Selectores dependientes Organización → Sucursal → Departamento en administración de usuarios.
- Matriz RBAC predeterminada para todos los roles del sistema.
- Seeding aditivo de permisos por rol.
- Nuevas validaciones de consistencia entre organización, sucursal y departamento.
- Navegación responsive reforzada para escritorio, tablet y móvil.
- Menú móvil accesible con cierre mediante `Escape`.
- Enlace “Saltar al contenido principal”.
- Estructura semántica con `main`, navegación etiquetada y mejoras ARIA.
- Objetivos táctiles de al menos 44 px.
- Soporte para `prefers-reduced-motion` y `forced-colors`.
- Ajustes de contraste, legibilidad y foco visible.

### Mejorado

- Estados y severidades presentados con nombres legibles en la interfaz.
- Acciones disponibles según el estado real de auditorías y planes de acción.
- Módulo de Marcos y controles reorganizado para evitar separación excesiva entre secciones.
- Evaluaciones con mejor flujo de preguntas y registro de cumplimiento.
- Reportes PDF con wrapping real, altura dinámica de filas y paginación por espacio disponible.
- Formularios administrativos con menor exposición de GUID y mayor consistencia UX.
- Tablas y formularios adaptados a resoluciones pequeñas y escenarios de zoom.

### Accesibilidad

- Implementación técnica alineada con lineamientos de NORTIC B2:2017 y WCAG 2.0.
- Navegación completa mediante teclado.
- Foco claramente visible.
- Mecanismo para evitar bloques repetitivos.
- Mejora de contraste y legibilidad.
- Soporte para reducción de movimiento y modos de alto contraste.

> La alineación con NORTIC B2 no constituye una certificación formal.

### Calidad

- Backend CI en verde.
- Frontend CI en verde.
- E2E CI en verde.
- Security CI en verde.
- Container CI en verde.
- 57 pruebas automatizadas de backend validadas.

### Documentación

- README actualizado al estado final de v1.1.0.
- Contexto académico aclarado: Auditoría Informática fue una materia teórica.
- Se documenta que AuditCore no fue el proyecto final original de la asignatura, sino una evolución posterior basada en la exposición grupal y en los conceptos trabajados durante la materia.
- Estado del proyecto actualizado a finalizado y estable.

---

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

AuditCore surge como evolución profesional posterior a la asignatura **Auditoría Informática (SOF-009)** del Instituto Tecnológico de Las Américas (ITLA), período **2017-C2**. La asignatura fue de naturaleza teórica; la plataforma se desarrolló posteriormente tomando como base conceptual la exposición realizada por el grupo y los temas estudiados durante la materia.
