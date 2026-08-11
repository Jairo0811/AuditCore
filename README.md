<div align="center">

<img src="./docs/images/auditcore-logo.png" alt="Logo de AuditCore" width="520" />

<br/><br/>

<img src="https://img.shields.io/badge/ITLA-2017--C2-0057B8?style=for-the-badge" alt="ITLA 2017-C2" />
<img src="https://img.shields.io/badge/Estado-v1.0.0%20estable-22C55E?style=for-the-badge" alt="Estado v1.0.0 estable" />
<img src="https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core 10" />
<img src="https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=0B1220" alt="React 19" />
<img src="https://img.shields.io/badge/TypeScript-6-3178C6?style=for-the-badge&logo=typescript&logoColor=white" alt="TypeScript 6" />
<img src="https://img.shields.io/badge/SQL_Server-2025-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server 2025" />
<img src="https://img.shields.io/badge/E2E-Playwright-2EAD33?style=for-the-badge&logo=playwright&logoColor=white" alt="Playwright E2E" />
<img src="https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker Ready" />
<img src="https://img.shields.io/badge/CI-Passing-22C55E?style=for-the-badge&logo=githubactions&logoColor=white" alt="CI Passing" />

<br/><br/>

<strong>Plataforma full stack para auditoría de TI, cumplimiento, riesgos, evidencias, hallazgos y planes de acción.</strong>

</div>

---

## 📌 Descripción

**AuditCore** es una plataforma web orientada a la gestión integral de auditorías de Tecnologías de la Información. Permite planificar y ejecutar auditorías, evaluar controles, registrar riesgos y hallazgos, gestionar evidencias, dar seguimiento a planes de acción y generar información ejecutiva.

El proyecto nace como una reconstrucción moderna de una experiencia académica de **Auditoría Informática (SOF-009)** del ITLA, cursada durante **2017-C2**, evolucionada hasta una aplicación full stack con arquitectura limpia, seguridad, CI, pruebas automatizadas, contenedores y configuración de producción.

## 🎓 Contexto académico

| Dato | Información |
|---|---|
| 🏫 Institución | Instituto Tecnológico de Las Américas (ITLA) |
| 📖 Asignatura | Auditoría Informática (SOF-009) |
| 👨‍🏫 Profesor | Simeon Clase Ulloa |
| 📅 Período | 2017-C2 |
| 📚 Naturaleza | Materia teórica |
| 🧩 Base conceptual | COBIT y administración de datos |

### Integrantes del grupo original

| Nombre | Matrícula |
|---|---|
| Sianya Jesuína Castillo Perez | 2015-2734 |
| Sinver Vladimir Aguiló Flores | 2015-2872 |
| Leidy Jireth Medina Oleaga | 2015-2942 |
| Francis Jairo Matías Rosario | 2015-2984 |
| Pedro Arturo de León Parra | 2015-3018 |
| Yeidy Khris Utate | 2015-3143 |

---

## ✅ Estado actual

**AuditCore v1.0.0** está funcionalmente cerrado para su alcance actual.

La plataforma incluye:

- autenticación JWT con refresh tokens rotativos;
- autorización RBAC basada en permisos;
- aislamiento multiempresa por organización;
- organizaciones, sucursales y departamentos;
- usuarios, roles y permisos;
- auditorías y ciclo de vida completo;
- riesgos y tratamiento;
- hallazgos y seguimiento;
- evidencias con almacenamiento persistente y validación segura;
- planes de acción;
- marcos, controles, preguntas, evaluaciones y respuestas;
- Dashboard ejecutivo;
- exportación de reportes CSV, Excel y PDF;
- health checks, rate limiting y manejo global de errores;
- CI automatizado para backend, frontend, seguridad, contenedores y E2E;
- Dockerfiles y Docker Compose para ejecución full stack;
- pruebas E2E con Playwright sobre SQL Server y API reales.

El backend mantiene **57 pruebas automatizadas** y la matriz de cierre de `v1.0.0` fue validada completamente en GitHub Actions.

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
│   └── auditcore-web/
│       ├── src/
│       ├── Dockerfile
│       └── nginx.conf
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

### Backend

```powershell
cd backend
dotnet restore .\AuditCore.slnx
dotnet build .\AuditCore.slnx
dotnet run --project .\src\AuditCore.Api\AuditCore.Api.csproj
```

### Frontend

```powershell
cd frontend\auditcore-web
npm ci
npm run dev
```

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
cd frontend\auditcore-web
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

### Matriz CI de v1.0.0

| Pipeline | Estado |
|---|---|
| Backend CI | ✅ Passing |
| Frontend CI | ✅ Passing |
| E2E CI | ✅ Passing |
| Security CI | ✅ Passing |
| Container CI | ✅ Passing |

---

## 📦 Versionado

La primera versión estable corresponde a **v1.0.0**.

Consulta [`CHANGELOG.md`](./CHANGELOG.md) para ver el detalle funcional y técnico de cada versión.

---

<div align="center">

**AuditCore — Audita. Evalúa. Protege.**

</div>
