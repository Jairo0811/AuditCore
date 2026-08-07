<div align="center">

<img src="./docs/images/auditcore-logo.png" alt="Logo de AuditCore" width="520" />

<br />

<img src="https://img.shields.io/badge/ITLA-2017--C2-0057B8?style=for-the-badge" alt="ITLA 2017-C2" />
![Estado](https://img.shields.io/badge/Estado-Full%20Stack%20completado-22C55E?style=for-the-badge)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=0B1220)
![TypeScript](https://img.shields.io/badge/TypeScript-6-3178C6?style=for-the-badge&logo=typescript&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-2025-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Clean-14B8A6?style=for-the-badge)
![CI](https://img.shields.io/badge/CI-Passing-22C55E?style=for-the-badge&logo=githubactions&logoColor=white)

**Plataforma full stack para auditoría de TI, cumplimiento, riesgos, evidencias, hallazgos y planes de acción.**

</div>

---

## 📌 Descripción

**AuditCore** es una plataforma web orientada a la gestión integral de auditorías de Tecnologías de la Información. Permite planificar y ejecutar auditorías, evaluar controles, registrar riesgos y hallazgos, gestionar evidencias, dar seguimiento a planes de acción y generar información ejecutiva.

El proyecto nace como una reconstrucción moderna de una experiencia académica de la asignatura **Auditoría Informática (SOF-009)** del Instituto Tecnológico de Las Américas (ITLA), cursada durante el período **2017-C2**. La exposición académica original sobre COBIT y administración de datos sirvió como base conceptual para evolucionar la idea hacia una aplicación profesional.

---

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

> El grupo participó en la exposición académica original. La reconstrucción moderna de AuditCore corresponde a una iniciativa posterior.

---

## ✅ Estado actual

AuditCore cuenta con **backend y frontend funcionales e integrados**.

### Backend

- ✅ Clean Architecture: Domain, Application, Infrastructure y API
- ✅ ASP.NET Core Web API sobre .NET 10
- ✅ Entity Framework Core + SQL Server
- ✅ Migraciones y ModelSnapshot sincronizados
- ✅ JWT Bearer + refresh token rotation
- ✅ RBAC por roles y permisos
- ✅ Aislamiento multiempresa
- ✅ Organizaciones, sucursales y departamentos
- ✅ Usuarios, roles y permisos
- ✅ Auditorías y ciclo de estados
- ✅ Riesgos y tratamiento
- ✅ Hallazgos y seguimiento
- ✅ Evidencias con validación de archivos y SHA-256
- ✅ Planes de acción
- ✅ Marcos, controles, preguntas, evaluaciones y respuestas
- ✅ Dashboard ejecutivo
- ✅ Exportación CSV, Excel y PDF
- ✅ ProblemDetails, rate limiting y health checks
- ✅ Auditoría automática y soft delete
- ✅ 57 pruebas automatizadas pasando
- ✅ Backend CI en verde

### Frontend

- ✅ React 19 + TypeScript + Vite
- ✅ Login conectado a la API
- ✅ Sesión JWT y renovación automática
- ✅ Rutas protegidas
- ✅ Logout real
- ✅ Dashboard conectado a métricas reales
- ✅ Shell responsive y navegación modular
- ✅ Gestión de auditorías
- ✅ Gestión de riesgos
- ✅ Gestión de hallazgos
- ✅ Evidencias: carga, consulta, descarga y eliminación
- ✅ Planes de acción y progreso
- ✅ Marcos y controles
- ✅ Workbench de evaluaciones
- ✅ Organizaciones, sucursales y departamentos
- ✅ Usuarios y roles
- ✅ Reportes CSV, Excel y PDF
- ✅ Frontend CI: lint + build en verde

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
- Lucide React

### Ingeniería

- Clean Architecture
- SOLID
- DRY
- KISS
- Git / GitHub
- GitHub Actions
- Arquitectura modular
- Multi-tenancy lógico por organización

---

## 🏗️ Arquitectura

```text
AuditCore/
├── backend/
│   ├── AuditCore.slnx
│   ├── src/
│   │   ├── AuditCore.Domain/
│   │   ├── AuditCore.Application/
│   │   ├── AuditCore.Infrastructure/
│   │   └── AuditCore.Api/
│   └── tests/
│       ├── AuditCore.Domain.Tests/
│       ├── AuditCore.Application.Tests/
│       └── AuditCore.Api.IntegrationTests/
│
├── frontend/
│   └── auditcore-web/
│       └── src/
│           ├── app/
│           ├── components/
│           ├── features/
│           ├── lib/
│           ├── services/
│           └── styles/
│
└── .github/workflows/
    ├── backend-ci.yml
    └── frontend-ci.yml
```

Dependencias del backend:

```text
Domain ← Application ← Infrastructure ← API
```

---

## 🔐 Seguridad

AuditCore implementa access tokens JWT de corta duración, refresh tokens rotativos, políticas de autorización por permiso, aislamiento por organización, rate limiting en autenticación, validación de evidencias, manejo global de errores y auditoría automática de cambios.

Los secretos de desarrollo y producción deben gestionarse fuera del repositorio mediante variables de entorno, User Secrets o un gestor de secretos.

---

## ▶️ Ejecución local

### Backend

```powershell
cd backend
dotnet restore .\AuditCore.slnx
dotnet build .\AuditCore.slnx
dotnet ef database update `
  --project .\src\AuditCore.Infrastructure\AuditCore.Infrastructure.csproj `
  --startup-project .\src\AuditCore.Api\AuditCore.Api.csproj `
  --context AuditCoreDbContext
dotnet run --project .\src\AuditCore.Api\AuditCore.Api.csproj
```

### Frontend

```powershell
cd frontend\auditcore-web
npm ci
npm run dev
```

Por defecto el frontend espera la API en:

```text
http://localhost:5047/api
```

Puede cambiarse mediante la variable de entorno usada por el cliente HTTP del frontend.

---

## 🧪 Validación

### Backend

```powershell
cd backend
dotnet build .\AuditCore.slnx
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

Estado validado: **lint y build pasando en GitHub Actions**.

---

## 📈 Evolución futura

La versión actual cubre el alcance funcional full stack. Las mejoras futuras pueden enfocarse en experiencia de usuario avanzada, catálogos adicionales de marcos de control, notificaciones, almacenamiento externo de evidencias, observabilidad, despliegue cloud y pruebas E2E del navegador.

---

<div align="center">

**AuditCore — Audita. Evalúa. Protege.**

</div>
