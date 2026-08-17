<div align="center">

<img src="./docs/images/auditcore-logo.png" alt="Logo de AuditCore" width="520" />


<p align="center">
  <img src="https://img.shields.io/badge/ITLA-2017--C2-0057B8?style=for-the-badge" alt="ITLA 2017-C2" />
</p>

<p align="center">
<img src="https://img.shields.io/badge/Estado-v1.1.0%20finalizada-22C55E?style=for-the-badge" alt="Estado v1.1.0 finalizada" />
</p>


<br/><br/>



<img src="https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core 10" />
<img src="https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=0B1220" alt="React 19" />
<img src="https://img.shields.io/badge/TypeScript-6-3178C6?style=for-the-badge&logo=typescript&logoColor=white" alt="TypeScript 6" />
<img src="https://img.shields.io/badge/SQL_Server-2025-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server 2025" />
<img src="https://img.shields.io/badge/E2E-Playwright-2EAD33?style=for-the-badge&logo=playwright" alt="Playwright E2E" />
<img src="https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker Ready" />
<img src="https://img.shields.io/badge/CI-Passing-22C55E?style=for-the-badge&logo=githubactions&logoColor=white" alt="CI Passing" />
<img src="https://img.shields.io/badge/Accesibilidad-NORTIC%20B2%20%2F%20WCAG%202.0-0EA5E9?style=for-the-badge" alt="Accesibilidad alineada con NORTIC B2 y WCAG 2.0" />

<br/><br/>

<strong>Plataforma full stack para auditoría de TI, cumplimiento, riesgos, evidencias, hallazgos y planes de acción.</strong>

</div>

---

## 📌 Descripción

**AuditCore** es una plataforma web orientada a la gestión integral de auditorías de Tecnologías de la Información. Permite planificar y ejecutar auditorías, evaluar controles, registrar riesgos y hallazgos, gestionar evidencias, dar seguimiento a planes de acción y generar información ejecutiva.

El proyecto surge como una evolución profesional posterior a la asignatura **Auditoría Informática (SOF-009)** del ITLA, cursada durante **2017-C2**. La materia fue de naturaleza teórica: AuditCore no fue el proyecto final original de la asignatura, sino una reconstrucción posterior que toma como base conceptual la exposición realizada por el grupo y transforma esos contenidos en una aplicación full stack moderna.

A partir de esa exposición se desarrolló una plataforma real alrededor de los conceptos trabajados en clase: auditoría de TI, controles, riesgos, hallazgos, evidencias, planes de acción, cumplimiento, trazabilidad y administración organizacional.

## 🎓 Contexto académico

| Dato | Información |
|---|---|
| 🏫 Institución | Instituto Tecnológico de Las Américas (ITLA) |
| 📖 Asignatura | Auditoría Informática (SOF-009) |
| 👨‍🏫 Profesor | Simeon Clase Ulloa |
| 📅 Período | 2017-C2 |
| 📚 Naturaleza | Materia teórica |
| 🧩 Base conceptual | Exposición grupal de la asignatura, COBIT y administración de datos |
| 🛠️ Evolución posterior | Conversión de los contenidos académicos en una plataforma profesional de auditoría de TI |

### Integrantes del grupo original

| Nombre | Matrícula |
|---|---|
| Sianya Jesuína Castillo Perez | 2015-2734 |
| Sinver Vladimir Aguiló Flores | 2015-2872 |
| Leidy Jireth Medina Oleaga | 2015-2942 |
| Francis Jairo Matías Rosario | 2015-2984 |
| Pedro Arturo de León Parra | 2015-3018 |
| Yeidy Khris Utate | 2015-3143 |

## 🧭 Continuidad académica

**AuditCore** representa el primer punto documentado de una continuidad académica por **compañero recurrente** con [**IngSoft Studio**](https://github.com/Jairo0811/IngSoft-Studio) dentro de la trayectoria de Francis Jairo Matías Rosario en el Instituto Tecnológico de Las Américas (ITLA). La relación entre ambos proyectos es **formativa y cronológica**: no existe una dependencia técnica entre las aplicaciones, sino la coincidencia de un mismo integrante en dos grupos académicos de materias teóricas cursadas en períodos consecutivos.

La primera coincidencia ocurrió en **2017-C2** durante **Auditoría Informática (SOF-009)**, asignatura que posteriormente sirvió como base conceptual para AuditCore. En el período siguiente, **2017-C3**, **Pedro Arturo de León Parra (2015-3018)** volvió a coincidir con Francis Jairo Matías Rosario en **Introducción a la Ingeniería en Software (SOF-015)**, cuyos contenidos inspiraron posteriormente IngSoft Studio.

| Orden | Código | Asignatura | Proyecto | Período | Compañero recurrente |
|---:|---|---|---|---|---|
| 1 | SOF-009 | Auditoría Informática | **AuditCore** | 2017-C2 | **Pedro Arturo de León Parra — 2015-3018** |
| 2 | SOF-015 | Introducción a la Ingeniería en Software | [**IngSoft Studio**](https://github.com/Jairo0811/IngSoft-Studio) | 2017-C3 | **Pedro Arturo de León Parra — 2015-3018** |

Vistos en conjunto, ambos proyectos documentan una continuidad real entre compañeros a lo largo de dos períodos académicos consecutivos y muestran una progresión conceptual desde **auditoría, controles y cumplimiento** hacia **ingeniería de software, calidad y ciclo de vida del desarrollo**. Cada repositorio conserva su identidad académica original y su implementación profesional posterior.

---

## ✅ Estado actual

**AuditCore v1.1.0 está finalizada y estable.**

La plataforma incluye:

- autenticación JWT con refresh tokens rotativos;
- autorización RBAC basada en permisos;
- matriz RBAC predeterminada para todos los roles del sistema;
- aislamiento multiempresa por organización;
- organizaciones, sucursales y departamentos;
- asociación opcional de usuarios con organización, sucursal y departamento;
- selectores dependientes Organización → Sucursal → Departamento;
- usuarios, roles y permisos;
- auditorías y ciclo de vida completo;
- riesgos y tratamiento;
- hallazgos y seguimiento;
- evidencias con almacenamiento persistente y validación segura;
- planes de acción;
- marcos, controles, preguntas, evaluaciones y respuestas;
- Dashboard ejecutivo;
- exportación de reportes CSV, Excel y PDF;
- reportes PDF con wrapping y paginación dinámicos;
- identidad visual propia integrada en frontend y reportes PDF;
- navegación administrativa organizada por áreas funcionales;
- CRUD administrativo con validaciones y manejo de relaciones;
- generación automática de códigos;
- estados y severidades legibles en la interfaz;
- interfaz responsive para escritorio, tablet y móvil;
- accesibilidad alineada con NORTIC B2:2017 y WCAG 2.0;
- navegación por teclado, foco visible y enlace para saltar al contenido principal;
- soporte para `prefers-reduced-motion` y `forced-colors`;
- health checks, rate limiting y manejo global de errores;
- CI automatizado para backend, frontend, seguridad, contenedores y E2E;
- Dockerfiles y Docker Compose para ejecución full stack;
- pruebas E2E con Playwright sobre SQL Server y API reales.

El backend mantiene **57 pruebas automatizadas**, y la matriz final de validación de `v1.1.0` quedó completamente verde en GitHub Actions.

---

## ♿ Accesibilidad y responsive

AuditCore fue revisada para funcionar correctamente en escritorio, tablet y móvil, incluyendo resoluciones pequeñas y escenarios de zoom.

La implementación de accesibilidad toma como referencia los lineamientos de **NORTIC B2:2017** y **WCAG 2.0**, especialmente los principios Perceptible, Operable, Comprensible y Robusto y criterios A/AA aplicables a la interfaz.

Entre las medidas incorporadas se incluyen:

- navegación completa mediante teclado;
- foco de teclado claramente visible;
- enlace “Saltar al contenido principal”;
- estructura semántica con `nav`, `main` y etiquetas ARIA;
- cierre del menú móvil mediante `Escape`;
- iconos decorativos ocultos a tecnologías asistivas;
- objetivos táctiles de al menos 44 px;
- mejora de contraste y legibilidad;
- formularios y acciones adaptables a pantallas pequeñas;
- tablas con desplazamiento horizontal controlado;
- soporte para reducción de movimiento;
- compatibilidad con modos de alto contraste mediante `forced-colors`.

> Esta implementación está alineada técnicamente con NORTIC B2:2017 / WCAG 2.0, pero no implica una certificación formal emitida por una entidad evaluadora.

---

## 🛠️ Stack tecnológico

### Backend

<p>
  <img src="https://skillicons.dev/icons?i=dotnet,cs" alt=".NET y C#" />
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/swagger/swagger-original.svg" alt="Swagger" width="48" height="48" />
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/microsoftsqlserver/microsoftsqlserver-plain.svg" alt="SQL Server" width="48" height="48" />
</p>

- .NET 10
- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- Swagger / OpenAPI
- xUnit

### Frontend

<p>
  <img src="https://skillicons.dev/icons?i=react,ts,vite" alt="React, TypeScript y Vite" />
</p>

- React 19
- TypeScript 6
- Vite
- React Router
- TanStack Query
- Axios
- React Hook Form
- Zod
- Recharts

### QA, seguridad y entrega

<p>
  <img src="https://skillicons.dev/icons?i=git,github,githubactions,docker" alt="Git, GitHub, GitHub Actions y Docker" />
</p>

- xUnit
- Playwright
- GitHub Actions
- auditoría de dependencias NuGet y npm
- Docker / Docker Compose
- Nginx para servir el frontend en producción
- Clean Architecture
- SOLID, DRY y KISS
- arquitectura modular
- multi-tenancy lógico por organización

---

## 🏗️ Arquitectura

```text
AuditCore/
├── backend/
│   ├── src/
│   │   ├── AuditCore.Domain/
│   │   ├── AuditCore.Application/
│   │   ├── AuditCore.Infrastructure/
│   │   └── AuditCore.Api/
│   ├── tests/
│   └── Dockerfile
├── frontend/
│   ├── src/
│   ├── public/
│   ├── package.json
│   ├── Dockerfile
│   └── nginx.conf
├── e2e/
│   └── tests/
├── .github/workflows/
│   ├── backend-ci.yml
│   ├── frontend-ci.yml
│   ├── e2e-ci.yml
│   ├── security-ci.yml
│   └── container-ci.yml
├── compose.yml
└── .env.example
```

```text
Domain ← Application ← Infrastructure ← API
                           ↑
                       React SPA
```

---

## 🔐 Seguridad

AuditCore implementa:

- access tokens JWT de corta duración;
- refresh tokens rotativos;
- políticas de autorización por permiso;
- matriz RBAC para los roles `SUPER_ADMIN`, `ORGANIZATION_ADMIN`, `AUDIT_MANAGER`, `AUDITOR`, `AUDITEE`, `RISK_OWNER` y `VIEWER`;
- aislamiento por organización;
- validación de pertenencia multiempresa;
- rate limiting para autenticación;
- validación segura de evidencias;
- protección contra traversal en almacenamiento de archivos;
- manejo global de errores mediante Problem Details;
- auditoría automática de creación y modificación;
- secretos externos por variables de entorno;
- auditoría automatizada de dependencias NuGet y npm.

---

## ▶️ Ejecución local

> Los comandos son relativos al repositorio y funcionan independientemente de la ubicación local donde se haya clonado AuditCore.

### Backend

```powershell
cd backend
dotnet restore .\AuditCore.slnx
dotnet build .\AuditCore.slnx
dotnet run --project .\src\AuditCore.Api\AuditCore.Api.csproj
```

La API de desarrollo escucha por defecto en:

```text
http://localhost:5047
```

### Frontend

En otra terminal:

```powershell
cd frontend
npm ci
npm run dev
```

Vite expone normalmente la aplicación en:

```text
http://localhost:5173
```

### 🔑 Acceso local de desarrollo

La instalación de desarrollo crea un usuario administrador por defecto:

| Campo | Valor |
|---|---|
| Correo | `admin@auditcore.local` |
| Rol | `SUPER_ADMIN` |
| Contraseña | `AuditCore123..` |

> La contraseña corresponde al valor actual de `SeedData:AdminPassword` en `backend/src/AuditCore.Api/appsettings.Development.json`. Este acceso es exclusivamente para desarrollo y demostración local. No reutilices estas credenciales en producción; configura secretos propios mediante variables de entorno o un proveedor de secretos.

---

## 🐳 Ejecución con Docker

Copia la plantilla de variables de entorno:

```powershell
Copy-Item .env.example .env
```

Reemplaza los valores de ejemplo de `.env` por secretos propios y ejecuta:

```powershell
docker compose up --build
```

Servicios por defecto:

| Servicio | URL / Puerto |
|---|---|
| Frontend | `http://localhost:8080` |
| API | `http://localhost:5047` |
| SQL Server | `localhost:1433` |

Los datos de SQL Server y las evidencias utilizan volúmenes persistentes.

---

## 🧪 Validación

### Backend

```powershell
cd backend
dotnet test .\AuditCore.slnx
```

Estado validado: **57/57 pruebas pasando**.

### Frontend

```powershell
cd frontend
npm ci
npm run lint
npm run build
```

### End-to-End

El pipeline E2E levanta SQL Server, aplica las migraciones de EF Core, inicia la API, compila el frontend y ejecuta Playwright sobre el sistema real.

El flujo integral cubre:

```text
Login
  ↓
Auditoría
  ↓
Riesgo
  ↓
Hallazgo
  ↓
Evidencia
  ↓
Plan de acción
  ↓
Cierre de estados
  ↓
Dashboard
  ↓
Reporte CSV
```

### Matriz CI final de v1.1.0

| Pipeline | Estado |
|---|---|
| Backend CI | ✅ Passing |
| Frontend CI | ✅ Passing |
| E2E CI | ✅ Passing |
| Security CI | ✅ Passing |
| Container CI | ✅ Passing |

---

## 📦 Versionado

- **v1.0.0** — cierre funcional inicial.
- **v1.1.0** — cierre final con refinamientos administrativos, RBAC, responsive, accesibilidad y estabilización UX.

AuditCore se considera **finalizada** en su alcance actual y lista para demostración, portafolio y evolución futura si se decide continuar comercialmente.

Consulta [`CHANGELOG.md`](./CHANGELOG.md) para ver el detalle funcional y técnico de cada versión.

---

<div align="center">

**AuditCore — Audita. Evalúa. Protege.**

</div>
