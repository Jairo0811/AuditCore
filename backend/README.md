# AuditCore Backend

Backend profesional de AuditCore construido con ASP.NET Core 10, Entity Framework Core 10 y SQL Server siguiendo Clean Architecture.

## Arquitectura

La solución mantiene la regla de dependencias:

```text
Domain <- Application <- Infrastructure <- Api
```

- `AuditCore.Domain`: entidades y reglas de negocio.
- `AuditCore.Application`: contratos, DTOs, permisos e interfaces.
- `AuditCore.Infrastructure`: EF Core, SQL Server, identidad, servicios y persistencia.
- `AuditCore.Api`: controladores, middleware, CORS, rate limiting, health checks y composición.

## Módulos implementados

- Autenticación JWT con access token y refresh token rotativo.
- Revocación de refresh tokens y protección contra reutilización.
- Roles, permisos y políticas RBAC.
- Organizaciones, sucursales y departamentos.
- Usuarios y asignación de roles.
- Auditorías y ciclo de vida de ejecución.
- Riesgos con probabilidad, impacto, nivel y tratamiento.
- Hallazgos y seguimiento de estados.
- Evidencias documentales con validación de tamaño/tipo, hash SHA-256 y almacenamiento seguro.
- Planes de acción, responsable, fecha de compromiso, progreso y vencimiento.
- Marcos de control, controles, preguntas, evaluaciones y respuestas.
- Dashboard ejecutivo.
- Exportación de resumen de auditorías a CSV, XLSX y PDF.
- Aislamiento multiempresa mediante el claim `organization_id`.
- Auditoría automática de `CreatedByUserId` y `UpdatedByUserId`.
- Soft delete y `rowversion` para concurrencia optimista.
- CORS configurable para el frontend.
- Rate limiting en autenticación.
- Health check público en `/health`.
- Manejo global de excepciones y respuestas Problem Details.

## Requisitos

- .NET SDK 10
- SQL Server
- `dotnet-ef` 10.0.10 para administración manual de migraciones

## Configuración local

La API usa la sección `ConnectionStrings:DefaultConnection` y configuración JWT. No se deben versionar contraseñas ni claves reales.

Ejemplo con variables de entorno en PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=AuditCoreDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
$env:Jwt__Key = "REEMPLAZAR-POR-UNA-CLAVE-SEGURA-DE-AL-MENOS-32-CARACTERES"
$env:SeedData__AdminPassword = "REEMPLAZAR-POR-UNA-CONTRASENA-SEGURA"
```

El correo del administrador inicial se puede definir mediante `SeedData__AdminEmail`.

## Compilación y pruebas

Desde `backend`:

```powershell
dotnet restore .\AuditCore.slnx
dotnet build .\AuditCore.slnx
dotnet test .\AuditCore.slnx
```

Para validar en configuración Release:

```powershell
dotnet build .\AuditCore.slnx --configuration Release
dotnet test .\AuditCore.slnx --configuration Release --no-build
```

## Migraciones

Aplicar migraciones:

```powershell
dotnet ef database update `
  --project .\src\AuditCore.Infrastructure\AuditCore.Infrastructure.csproj `
  --startup-project .\src\AuditCore.Api\AuditCore.Api.csproj `
  --context AuditCoreDbContext
```

Verificar que el modelo esté sincronizado con el snapshot:

```powershell
dotnet ef migrations has-pending-model-changes `
  --project .\src\AuditCore.Infrastructure\AuditCore.Infrastructure.csproj `
  --startup-project .\src\AuditCore.Api\AuditCore.Api.csproj `
  --context AuditCoreDbContext
```

El pipeline de GitHub Actions ejecuta restore, build Release, pruebas, generación del script completo de migraciones y validación de cambios pendientes del modelo.

## Seguridad

- Las claves JWT y contraseñas se suministran por configuración segura o variables de entorno.
- Los endpoints protegidos requieren autenticación y permisos explícitos.
- Los servicios sensibles verifican el tenant/organización del usuario autenticado.
- Las evidencias limitan tamaño y tipos MIME, normalizan nombres, calculan SHA-256 y validan la ruta física para impedir path traversal.
- El entorno `Testing` eleva únicamente el límite de rate limiting para evitar interferencias entre pruebas paralelas; producción conserva el límite configurado.

## Endpoints principales

```text
/api/auth
/api/organizations
/api/branches
/api/departments
/api/users
/api/roles
/api/permissions
/api/audits
/api/risks
/api/findings
/api/evidence
/api/action-plans
/api/frameworks
/api/reports
/health
```

## CI

Workflow: `.github/workflows/backend-ci.yml`.

Un cambio de backend solo se considera listo para integrar cuando:

1. compila en Release sin errores;
2. todas las pruebas pasan;
3. la cadena de migraciones puede generar un script SQL válido;
4. EF Core no reporta cambios pendientes entre el modelo y el snapshot.
