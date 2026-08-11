<div align="center">

<img src="./docs/images/auditcore-logo.png" alt="Logo de AuditCore" width="520" />

<br/><br/>

<img src="https://img.shields.io/badge/ITLA-2017--C2-0057B8?style=for-the-badge" alt="ITLA 2017-C2" />
<img src="https://img.shields.io/badge/Estado-Full%20Stack%20completado-22C55E?style=for-the-badge" alt="Estado Full Stack completado" />
<img src="https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core 10" />
<img src="https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=0B1220" alt="React 19" />
<img src="https://img.shields.io/badge/TypeScript-6-3178C6?style=for-the-badge&logo=typescript&logoColor=white" alt="TypeScript 6" />
<img src="https://img.shields.io/badge/SQL_Server-2025-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server 2025" />
<img src="https://img.shields.io/badge/CI-Passing-22C55E?style=for-the-badge&logo=githubactions&logoColor=white" alt="CI Passing" />

<br/><br/>

<strong>Plataforma full stack para auditoría de TI, cumplimiento, riesgos, evidencias, hallazgos y planes de acción.</strong>

</div>

---

## 📌 Descripción

**AuditCore** es una plataforma web orientada a la gestión integral de auditorías de Tecnologías de la Información. Permite planificar y ejecutar auditorías, evaluar controles, registrar riesgos y hallazgos, gestionar evidencias, dar seguimiento a planes de acción y generar información ejecutiva.

El proyecto nace como una reconstrucción moderna de una experiencia académica de **Auditoría Informática (SOF-009)** del ITLA, cursada durante **2017-C2**.

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

AuditCore cuenta con backend y frontend funcionales e integrados, autenticación JWT, RBAC, aislamiento multiempresa, gestión de auditorías, riesgos, hallazgos, evidencias, planes de acción, marcos de control, reportes y Dashboard ejecutivo. El backend mantiene 57 pruebas automatizadas y los pipelines de backend/frontend se validan mediante GitHub Actions.

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
- TypeScript
- Vite
- React Router
- TanStack Query
- Axios
- React Hook Form
- Zod
- Recharts

### Ingeniería, calidad y CI/CD

<p>
  <img src="https://skillicons.dev/icons?i=git,github,githubactions" alt="Git, GitHub y GitHub Actions" />
</p>

<p>
  <img src="https://img.shields.io/badge/GitHub%20Actions-CI%2FCD-2088FF?style=flat-square&logo=githubactions&logoColor=white" alt="GitHub Actions" />
  <img src="https://img.shields.io/badge/xUnit-57%20tests-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="xUnit" />
</p>

- Clean Architecture
- SOLID, DRY y KISS
- Git / GitHub
- GitHub Actions
- Arquitectura modular
- Multi-tenancy lógico por organización

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
│   └── tests/
├── frontend/auditcore-web/
└── .github/workflows/
    ├── backend-ci.yml
    └── frontend-ci.yml
```

```text
Domain ← Application ← Infrastructure ← API
```

---

## 🔐 Seguridad

AuditCore implementa access tokens JWT de corta duración, refresh tokens rotativos, políticas de autorización por permiso, aislamiento por organización, rate limiting, validación de evidencias, manejo global de errores y auditoría automática de cambios.

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

## 🧪 Validación

```powershell
cd backend
dotnet test .\AuditCore.slnx
```

Estado validado: **57/57 pruebas pasando**.

```powershell
cd frontend\auditcore-web
npm ci
npm run lint
npm run build
```

Estado validado: **lint y build pasando en GitHub Actions**.

---

<div align="center">

**AuditCore — Audita. Evalúa. Protege.**

</div>
