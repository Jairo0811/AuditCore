# Arquitectura de AuditCore

AuditCore se desarrollará como un **monolito modular** apoyado en **Clean Architecture**. La solución se despliega como una sola aplicación, pero cada capacidad de negocio mantiene límites explícitos para reducir acoplamiento y permitir una evolución segura.

## Principios

- Organización por dominio, no únicamente por tipo técnico.
- Dependencias dirigidas hacia el dominio.
- Comunicación entre módulos mediante contratos explícitos.
- Persistencia y servicios externos aislados en Infrastructure.
- API utilizada como composition root.
- Sin referencias directas entre infraestructuras de módulos.
- Shared Kernel pequeño y estable.

## Módulos previstos

| Módulo | Responsabilidad |
|---|---|
| Identity | Usuarios, roles, permisos, sesiones y autenticación |
| Organizations | Empresas, sucursales, departamentos y aislamiento multiempresa |
| Frameworks | COBIT, ISO 27001, NIST, CIS y marcos personalizados |
| Audits | Planificación, alcance, ejecución y ciclo de vida de auditorías |
| Assessments | Controles, preguntas, checklists y evaluaciones |
| Evidence | Evidencias, archivos, metadatos y trazabilidad |
| Findings | Hallazgos, recomendaciones, responsables y estados |
| Risks | Riesgos, impacto, probabilidad, tratamiento y seguimiento |
| ActionPlans | Acciones correctivas, fechas, responsables y verificación |
| Reporting | Dashboard, indicadores y exportaciones PDF/Excel |
| Notifications | Alertas internas y notificaciones futuras |

## Capas por módulo

Cada módulo funcional seguirá esta estructura conceptual:

```text
Modules/<ModuleName>/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   └── Rules/
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Contracts/
│   ├── Dtos/
│   └── Validators/
├── Infrastructure/
│   ├── Persistence/
│   ├── Repositories/
│   └── Services/
└── Presentation/
    ├── Controllers/
    └── Contracts/
```

Mientras el proyecto conserve cuatro ensamblados principales, las carpetas de cada módulo se reflejarán dentro de `AuditCore.Domain`, `AuditCore.Application`, `AuditCore.Infrastructure` y `AuditCore.Api`. Si un módulo adquiere suficiente complejidad, podrá extraerse a ensamblados propios sin cambiar sus contratos públicos.

## Regla de dependencias

```text
Presentation/API
      │
      ▼
Application
      │
      ▼
Domain

Infrastructure ──► Application / Domain
```

- Domain no depende de ninguna otra capa.
- Application solo depende de Domain.
- Infrastructure implementa contratos definidos por Application.
- API compone módulos, middleware, seguridad y endpoints.

## Composition root

`AuditCore.Api` es el único punto autorizado para componer la aplicación. El arranque se divide en:

```text
Program
├── AddAuditCoreApi
│   ├── Controllers
│   ├── OpenAPI
│   └── AddAuditCoreModules
└── UseAuditCorePipeline
    ├── Swagger/HSTS
    ├── HTTPS
    ├── Authorization
    └── Controllers
```

Cada módulo añadirá sus servicios mediante métodos de extensión específicos, por ejemplo:

```csharp
services
    .AddIdentityModule(configuration)
    .AddOrganizationsModule(configuration)
    .AddAuditsModule(configuration);
```

## Comunicación entre módulos

Se utilizarán, en orden de preferencia:

1. Contratos de Application.
2. Eventos de dominio o integración.
3. Consultas de solo lectura explícitamente diseñadas.

No se permitirá que un módulo acceda directamente a repositorios, entidades internas o tablas privadas de otro módulo.

## Estrategia de evolución

1. Fundación modular y composition root.
2. Identity y Organizations.
3. Frameworks y motor de auditoría.
4. Evidence, Findings, Risks y ActionPlans.
5. Reporting, Notifications y preparación para producción.

Esta estrategia conserva la simplicidad operativa de un monolito y evita adoptar microservicios antes de que existan necesidades reales de escalado o despliegue independiente.
