# Arquitectura de AuditCore

AuditCore está implementado como un **monolito modular full stack** apoyado en **Clean Architecture**. La solución se despliega como una sola plataforma, pero cada capacidad de negocio mantiene límites explícitos para reducir acoplamiento, preservar el aislamiento multiempresa y permitir una evolución segura.

## Vista general

```mermaid
flowchart LR
    User["Usuario"] --> Web["React 19 · TypeScript · Vite"]
    Web --> Router["Rutas protegidas / TanStack Query"]
    Router --> API["ASP.NET Core Web API"]

    API --> Security["JWT · RBAC · Rate Limiting"]
    Security --> Application["Application · Casos de uso / DTO / Validación"]
    Application --> Domain["Domain · Entidades / Reglas / Eventos"]

    Infra["Infrastructure"] --> Application
    Infra --> Domain
    Infra --> EF["Entity Framework Core"]
    EF --> SQL[("SQL Server")]

    Infra --> Evidence["Evidencias / SHA-256"]
    Infra --> Reports["CSV · Excel · PDF"]
    API --> Health["Health checks / ProblemDetails"]
    API --> Audit["Auditoría automática"]
```

La regla estructural principal es que el dominio no conoce detalles de infraestructura. La API actúa como **composition root**, Application coordina los casos de uso e Infrastructure implementa persistencia, servicios técnicos y adaptadores externos.

## Principios

- Organización por dominio, no únicamente por tipo técnico.
- Dependencias dirigidas hacia el dominio.
- Comunicación entre módulos mediante contratos explícitos.
- Persistencia y servicios externos aislados en Infrastructure.
- API utilizada como composition root.
- Sin referencias directas entre infraestructuras de módulos.
- Shared Kernel pequeño y estable.
- Aislamiento multiempresa aplicado desde el contexto autenticado.

## Módulos funcionales

```mermaid
flowchart TB
    Identity["Identity"] --> Platform["AuditCore Platform"]
    Organizations["Organizations"] --> Platform
    Frameworks["Frameworks / Controls"] --> Platform
    Audits["Audits"] --> Platform
    Assessments["Assessments"] --> Platform
    Evidence["Evidence"] --> Platform
    Findings["Findings"] --> Platform
    Risks["Risks"] --> Platform
    Actions["Action Plans"] --> Platform
    Reporting["Reporting"] --> Platform

    Platform --> SQL[("SQL Server")]
```

| Módulo | Responsabilidad |
|---|---|
| Identity | Usuarios, roles, permisos, sesiones y autenticación |
| Organizations | Empresas, sucursales, departamentos y aislamiento multiempresa |
| Frameworks | Marcos, controles y catálogos de cumplimiento |
| Audits | Planificación, alcance, ejecución y ciclo de vida de auditorías |
| Assessments | Controles, preguntas, checklists, evaluaciones y respuestas |
| Evidence | Evidencias, archivos, metadatos, hashes y trazabilidad |
| Findings | Hallazgos, recomendaciones, responsables y estados |
| Risks | Riesgos, impacto, probabilidad, tratamiento y seguimiento |
| ActionPlans | Acciones correctivas, fechas, responsables y verificación |
| Reporting | Dashboard, indicadores y exportaciones CSV/Excel/PDF |

## Capas

La solución mantiene cuatro ensamblados principales:

```text
AuditCore.Domain
AuditCore.Application
AuditCore.Infrastructure
AuditCore.Api
```

Responsabilidades:

- **Domain:** entidades, reglas de negocio, value objects y eventos.
- **Application:** casos de uso, DTO, contratos, validadores y orquestación.
- **Infrastructure:** EF Core, SQL Server, repositorios, evidencias, reportes y servicios técnicos.
- **API:** endpoints, autenticación, autorización, middleware, OpenAPI y composition root.
- **Frontend:** React modular, rutas protegidas, estado remoto y experiencia de usuario.

## Regla de dependencias

```mermaid
flowchart TD
    Web["React Frontend"] --> API["AuditCore.Api"]
    API --> Application["AuditCore.Application"]
    Application --> Domain["AuditCore.Domain"]
    Infrastructure["AuditCore.Infrastructure"] --> Application
    Infrastructure --> Domain
```

- Domain no depende de ninguna otra capa.
- Application solo depende de Domain.
- Infrastructure implementa contratos definidos por Application y utiliza Domain.
- API compone módulos, middleware, seguridad y endpoints.
- El frontend consume exclusivamente contratos HTTP publicados por la API.

## Seguridad multiempresa

```mermaid
sequenceDiagram
    participant U as Usuario
    participant W as React
    participant A as API
    participant S as Seguridad
    participant UC as Caso de uso
    participant DB as SQL Server

    U->>W: inicia sesión
    W->>A: credenciales
    A->>S: autenticar
    S-->>W: access + refresh token
    W->>A: solicitud autenticada
    A->>S: validar rol, permiso y organización
    S->>UC: contexto autorizado
    UC->>DB: consulta filtrada por organización
    DB-->>UC: datos permitidos
    UC-->>W: respuesta
```

El cliente nunca se considera fuente confiable del tenant. La organización y los permisos efectivos se obtienen del contexto autenticado y vuelven a validarse en backend.

## Composition root

`AuditCore.Api` es el único punto autorizado para componer la aplicación. Desde `Program` se registran controladores, OpenAPI, autenticación, autorización, módulos, persistencia, servicios técnicos, rate limiting y health checks.

## Comunicación entre módulos

Se utilizan, en orden de preferencia:

1. Contratos de Application.
2. Servicios de dominio o aplicación con límites explícitos.
3. Eventos cuando una operación requiera desacoplamiento.
4. Consultas de solo lectura diseñadas específicamente para reporting.

Un módulo no debe acceder directamente a repositorios o detalles internos de otro módulo.

## Criterio de evolución

La arquitectura conserva la simplicidad operativa de un monolito y evita adoptar microservicios antes de que existan necesidades reales de escalado o despliegue independiente. Un módulo solo debería extraerse si aparecen límites operativos, de propiedad o de escalabilidad suficientemente claros.
