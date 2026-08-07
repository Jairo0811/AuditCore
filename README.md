<div align="center">

<img src="./docs/images/auditcore-logo.png" alt="Logo de AuditCore" width="520" />
<br/>
<br/>

<img src="https://img.shields.io/badge/ITLA-2017--C2-0057B8?style=for-the-badge" alt="ITLA 2017-C2" />

<br/>
<br/>

![Estado](https://img.shields.io/badge/Estado-Backend%20completado%20%7C%20Frontend%20en%20desarrollo-22C55E?style=for-the-badge)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?style=for-the-badge&logo=react&logoColor=0B1220)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?style=for-the-badge&logo=typescript&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-2025-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Clean-14B8A6?style=for-the-badge)
![CI](https://img.shields.io/badge/Backend_CI-Passing-22C55E?style=for-the-badge&logo=githubactions&logoColor=white)

> Estado actual: **backend completado, validado y fusionado a `main`**. La API, seguridad, persistencia, motor de auditoría, reportes, multiempresa y pruebas automatizadas están operativos. El trabajo pendiente se concentra en la integración y finalización del frontend React.

</div>

---

## 📌 Descripción

**AuditCore** es una plataforma web orientada a la gestión integral de auditorías de Tecnologías de la Información, cumplimiento, riesgos, evidencias, hallazgos y planes de acción.

El proyecto nace como una reconstrucción moderna de una experiencia académica de la asignatura **Auditoría Informática (SOF-009)** del Instituto Tecnológico de Las Américas (ITLA), cursada durante el período **2017-C2**.

La materia fue principalmente teórica. Sin embargo, la exposición realizada como proyecto final sirvió como base conceptual para transformar aquellos contenidos en una aplicación profesional, escalable y preparada para evolucionar a producto comercial.

> 💡 La idea de convertir aquel proyecto académico en una plataforma de software moderna fue concebida por **Francis Jairo Matías Rosario**.

---

## 🎓 Contexto académico

| Dato | Información |
|---|---|
| 🏫 Institución | Instituto Tecnológico de Las Américas (ITLA) |
| 📖 Asignatura | Auditoría Informática (SOF-009) |
| 👨‍🏫 Profesor | Simeon Clase Ulloa |
| 📅 Período académico | 2017-C2 |
| 📚 Naturaleza de la materia | Teórica |
| 🧩 Base conceptual | Exposición final sobre COBIT y administración de datos |
| 💡 Idea de reconstrucción | Francis Jairo Matías Rosario |

---

## 👥 Integrantes del grupo original

| Nombre completo | Matrícula |
|---|---|
| 👩‍🎓 **Sianya Jesuína Castillo Perez** | 2015-2734 |
| 👨‍🎓 **Sinver Vladimir Aguiló Flores** | 2015-2872 |
| 👩‍🎓 **Leidy Jireth Medina Oleaga** | 2015-2942 |
| 👨‍💻 **Francis Jairo Matías Rosario** | 2015-2984 |
| 👨‍🎓 **Pedro Arturo de León Parra** | 2015-3018 |
| 👩‍🎓 **Yeidy Khris Utate** | 2015-3143 |

> El grupo participó en la exposición académica original. La reconstrucción moderna de AuditCore corresponde a una iniciativa posterior desarrollada por Francis Jairo Matías Rosario.

---

## 🎯 Objetivo general

Desarrollar una plataforma profesional para planificar, ejecutar, documentar y dar seguimiento a auditorías de TI, permitiendo evaluar controles, identificar riesgos, registrar evidencias, gestionar hallazgos y generar reportes ejecutivos y técnicos.

---

## 🚀 Funcionalidades implementadas en el backend

- 📊 Dashboard ejecutivo conectado a datos reales
- 🏢 Gestión de organizaciones, sucursales y departamentos
- 👥 Usuarios, roles y permisos
- 🔐 Autenticación JWT con refresh tokens rotativos
- 🛡️ Autorización basada en permisos (RBAC)
- 🏷️ Aislamiento multiempresa
- 📋 Planificación y ciclo de vida de auditorías
- 🧭 Marcos de control, controles y preguntas
- ✅ Evaluaciones y respuestas de controles
- 📁 Evidencias documentales y metadatos
- ⚠️ Gestión de riesgos
- 🔎 Registro y seguimiento de hallazgos
- 🛠️ Planes de acción
- 📈 Indicadores de cumplimiento
- 📄 Exportación de reportes CSV, Excel y PDF
- 🧾 Auditoría automática de creación y modificación
- 🗑️ Soporte de soft delete
- ⚙️ Control de concurrencia con `rowversion`
- ❤️ Health checks
- 🚦 Rate limiting en autenticación
- 🌐 CORS configurable
- 🧯 Manejo global de excepciones
- 🧪 Pruebas unitarias e integración automatizadas
- 🔁 CI de backend con GitHub Actions

---

## 🧩 Marcos y estándares

AuditCore está diseñado para soportar múltiples marcos y buenas prácticas, entre ellos:

- COBIT
- ISO/IEC 27001
- NIST Cybersecurity Framework
- CIS Controls
- ITIL
- Marcos internos personalizados

COBIT constituye el marco inicial de referencia por su relación directa con la exposición académica que inspiró el proyecto.

---

## 🛠️ Stack tecnológico

### Backend

<p>
  <img src="https://skillicons.dev/icons?i=dotnet,cs" alt=".NET y C#" />
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/swagger/swagger-original.svg" alt="Swagger" width="48" height="48" />
</p>

<p>
  <img src="https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="ASP.NET Core Web API" />
  <img src="https://img.shields.io/badge/Entity%20Framework%20Core-Persistencia-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="Entity Framework Core" />
  <img src="https://img.shields.io/badge/JWT-Bearer-000000?style=flat-square&logo=jsonwebtokens&logoColor=white" alt="JWT Bearer" />
  <img src="https://img.shields.io/badge/xUnit-Pruebas-5E2B97?style=flat-square&logo=dotnet&logoColor=white" alt="xUnit" />
</p>

- .NET 10
- ASP.NET Core Web API
- C#
- Entity Framework Core 10
- SQL Server
- JWT Bearer Authentication
- Swagger / OpenAPI
- xUnit
- GitHub Actions

### Frontend

<p>
  <img src="https://skillicons.dev/icons?i=react,ts,vite" alt="React, TypeScript y Vite" />
</p>

<p>
  <img src="https://img.shields.io/badge/React%20Router-Navegación-CA4245?style=flat-square&logo=reactrouter&logoColor=white" alt="React Router" />
  <img src="https://img.shields.io/badge/TanStack%20Query-Datos-FF4154?style=flat-square&logo=reactquery&logoColor=white" alt="TanStack Query" />
  <img src="https://img.shields.io/badge/Axios-HTTP-5A29E4?style=flat-square&logo=axios&logoColor=white" alt="Axios" />
  <img src="https://img.shields.io/badge/React%20Hook%20Form-Formularios-EC5990?style=flat-square&logo=reacthookform&logoColor=white" alt="React Hook Form" />
  <img src="https://img.shields.io/badge/Zod-Validación-3E67B1?style=flat-square" alt="Zod" />
  <img src="https://img.shields.io/badge/Recharts-Gráficas-22B5BF?style=flat-square" alt="Recharts" />
  <img src="https://img.shields.io/badge/Lucide-Iconos-F56565?style=flat-square" alt="Lucide React" />
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

### Base de datos

<p>
  <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/microsoftsqlserver/microsoftsqlserver-plain.svg" alt="SQL Server" width="48" height="48" />
</p>

<p>
  <img src="https://img.shields.io/badge/Microsoft%20SQL%20Server-2025-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white" alt="Microsoft SQL Server 2025" />
  <img src="https://img.shields.io/badge/Migraciones-EF%20Core-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="Migraciones EF Core" />
</p>

### Arquitectura y herramientas

<p>
  <img src="https://skillicons.dev/icons?i=git,github,azure,aws" alt="Git, GitHub, Azure y AWS" />
</p>

<p>
  <img src="https://img.shields.io/badge/Clean%20Architecture-Modular-14B8A6?style=flat-square" alt="Clean Architecture" />
  <img src="https://img.shields.io/badge/SOLID-Principios-0F172A?style=flat-square" alt="SOLID" />
  <img src="https://img.shields.io/badge/OpenAPI-Documentación-6BA539?style=flat-square&logo=openapiinitiative&logoColor=white" alt="OpenAPI" />
</p>

- Clean Architecture
- Principios SOLID
- Git y GitHub
- CI con GitHub Actions
- Preparado para despliegues cloud

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
└── frontend/
    └── auditcore-web/
        └── src/
            ├── app/
            ├── components/
            ├── features/
            ├── hooks/
            ├── lib/
            ├── services/
            ├── styles/
            ├── types/
            └── utils/
```

Regla de dependencias:

```text
Domain ← Application ← Infrastructure ← API
```

- **Domain:** entidades y reglas de negocio.
- **Application:** contratos, DTOs, casos de uso y seguridad transversal.
- **Infrastructure:** EF Core, SQL Server, autenticación, servicios y persistencia.
- **API:** endpoints, middleware, CORS, Swagger, health checks y rate limiting.
- **Frontend:** SPA React desacoplada del backend.

---

## 🔐 Seguridad

- Access tokens JWT de corta duración
- Refresh tokens rotativos y revocables
- Hash SHA-256 de refresh tokens persistidos
- Autorización basada en permisos
- Separación multiempresa
- Rate limiting en autenticación
- Manejo centralizado de errores
- Gestión segura de secretos mediante configuración/variables de entorno
- Soft delete y trazabilidad de cambios
- Control de concurrencia mediante `rowversion`

---

## 🧪 Calidad y validación

El backend fue validado antes de fusionarse a `main` con:

- ✅ Compilación `Release` sin errores ni warnings
- ✅ **57/57 pruebas automatizadas**
  - 37 pruebas de dominio
  - 3 pruebas de aplicación
  - 17 pruebas de integración de API
- ✅ Validación de autenticación, refresh, logout y RBAC
- ✅ Validación de endpoints protegidos
- ✅ Health checks
- ✅ Generación completa del script de migraciones EF Core
- ✅ Modelo EF Core sin cambios pendientes respecto al snapshot
- ✅ Pipeline de GitHub Actions en verde

---

## 📦 Estado actual

| Área | Estado | Detalle |
|---|---|---|
| Backend | ✅ Completado | API y lógica de negocio finalizadas para el alcance actual |
| Clean Architecture | ✅ Completada | Separación Domain / Application / Infrastructure / API |
| Persistencia | ✅ Completada | EF Core, SQL Server, migraciones y seeders |
| Autenticación | ✅ Completada | JWT, refresh tokens y logout |
| RBAC | ✅ Completado | Roles, permisos y políticas dinámicas |
| Multiempresa | ✅ Completado | Restricción de acceso por organización |
| Organizaciones | ✅ Completado | Organizaciones, sucursales y departamentos |
| Usuarios | ✅ Completado | Usuarios, roles, bloqueo y activación |
| Auditorías | ✅ Completado | Ciclo de vida completo |
| Riesgos | ✅ Completado | Evaluación, tratamiento y cierre |
| Hallazgos | ✅ Completado | Registro y flujo de estados |
| Evidencias | ✅ Completado | Metadatos, hash y vinculación a auditorías/hallazgos |
| Planes de acción | ✅ Completado | Seguimiento y progreso |
| Motor de controles | ✅ Completado | Marcos, controles, preguntas, evaluaciones y respuestas |
| Dashboard | ✅ Completado | Métricas operativas y de cumplimiento |
| Reportes | ✅ Completado | CSV, Excel y PDF |
| Seguridad transversal | ✅ Completada | CORS, rate limiting, soft delete, auditoría y excepciones |
| Tests | ✅ Completados | Unitarios e integración |
| Backend CI | ✅ Completado | Build, tests y validación EF Core |
| Frontend | 🚧 En desarrollo | Integración final con la API pendiente |

---

## 🗺️ Roadmap

### Fase 1 — Fundación técnica

- [x] Arquitectura backend
- [x] Proyecto frontend
- [x] Estructura modular del frontend
- [x] Swagger / OpenAPI
- [x] Inyección de dependencias
- [x] Persistencia EF Core + SQL Server
- [x] Pruebas base y CI

### Fase 2 — Identidad y acceso

- [x] Usuarios
- [x] Roles
- [x] Permisos
- [x] JWT
- [x] Refresh tokens

### Fase 3 — Organizaciones y multiempresa

- [x] Organizaciones
- [x] Sucursales
- [x] Departamentos
- [x] Separación multiempresa

### Fase 4 — Motor de auditoría

- [x] Marcos
- [x] Controles
- [x] Preguntas
- [x] Evaluaciones
- [x] Respuestas

### Fase 5 — Gestión de resultados

- [x] Evidencias
- [x] Hallazgos
- [x] Riesgos
- [x] Recomendaciones
- [x] Planes de acción

### Fase 6 — Reportes y calidad backend

- [x] Dashboard conectado a datos reales
- [x] PDF
- [x] Excel
- [x] CSV
- [x] Health checks
- [x] Rate limiting
- [x] CI backend
- [x] Validación de migraciones

### Fase 7 — Frontend e integración final

- [ ] Conectar autenticación real
- [ ] Gestión de sesión y refresh token
- [ ] Conectar dashboard
- [ ] Integrar organizaciones y usuarios
- [ ] Integrar auditorías, riesgos y hallazgos
- [ ] Integrar evidencias y planes de acción
- [ ] Integrar motor de controles
- [ ] Integrar reportes
- [ ] UX/UI final
- [ ] Pruebas frontend y E2E
- [ ] Preparación para publicación

---

## ▶️ Ejecución local

### Backend

```powershell
cd backend
dotnet restore .\AuditCore.slnx
dotnet build .\AuditCore.slnx
dotnet run --project .\src\AuditCore.Api\AuditCore.Api.csproj
```

### Pruebas

```powershell
cd backend
dotnet test .\AuditCore.slnx
```

### Migraciones

```powershell
cd backend
dotnet ef database update `
  --project .\src\AuditCore.Infrastructure\AuditCore.Infrastructure.csproj `
  --startup-project .\src\AuditCore.Api\AuditCore.Api.csproj `
  --context AuditCoreDbContext
```

### Frontend

```powershell
cd frontend\auditcore-web
npm install
npm run dev
```

---

## ⚙️ Configuración

La configuración sensible no debe almacenarse en el repositorio.

Variables relevantes:

```text
ConnectionStrings__DefaultConnection
Jwt__Key
Jwt__Issuer
Jwt__Audience
Jwt__AccessTokenMinutes
Jwt__RefreshTokenDays
Cors__AllowedOrigins__0
```

Para desarrollo local se puede utilizar `appsettings.Development.json` junto con variables de entorno o secretos de usuario.

---

## 📚 Documentación adicional

La documentación específica del backend se encuentra en:

```text
backend/README.md
```

Incluye detalles de arquitectura, seguridad, endpoints, pruebas y configuración del backend.

---

## 📄 Licencia

Consulta el archivo `LICENSE` del repositorio para conocer los términos de uso.
