# 01 — Visão Arquitetural

> Parte de [`../README.md`](../README.md) · Arquitetura do sistema

Documentação estendida: [`../../BACKEND_ARCHITECTURE.md`](../../BACKEND_ARCHITECTURE.md)

---

## Estilo arquitetural

**Monorepo modular** com Clean Architecture simplificada:

- **Frontend:** SPA Vue 3 (Vite) — consome REST + SignalR
- **Backend:** API .NET 8 em camadas (Api / Application / Domain / Repository / Infrastructure)
- **Assíncrono:** RabbitMQ + workers externos + consumers hosted na API
- **Dados:** SQL Server por tenant; Redis para cache
- **Arquivos:** File Repository (API satélite + Azure Blob)

---

## Camadas backend

```
Api          → HTTP, Swagger, JWT, SignalR Hub, ExceptionHandler
Application  → Services, ToolHandlers, Consumers, Utils
Domain       → Models, DTOs, Interfaces, Enums, Validations
Repository   → EF Core, Repositories, Mappings, Migrations, MultiTenant MW
Infrastructure → RabbitMQ, TenantContextService
```

**Regra de dependência:** camadas externas dependem das internas; Domain não depende de ninguém.

---

## Camadas frontend

```
pages/       → Views por rota (Vue Router)
components/  → global/ (reutilizáveis) + domínio (workflow, documents…)
services/    → Axios → API REST
store/       → Vuex (user, theme, notifications)
locales/     → i18n pt/en/es
layouts/     → defaultLayout (sidebar+navbar) | authLayout
assets/css/  → global.css (design tokens)
```

Documentação UX: [`../../PRODUCT_DESIGN.md`](../../PRODUCT_DESIGN.md)

---

## Componentes externos

| Sistema | Integração | Protocolo |
|---------|------------|-----------|
| File Repository | Refit `IFileRepositoryApi` | HTTPS |
| LLM / AI Gateway | Refit `IChatCompletionApi` | HTTPS |
| Indexer / Embeddings | Refit `IEmbeddingsApi` | HTTPS |
| Marketplace | Refit `IMarketPlaceApi` | HTTPS + fila subscription |
| Anonimização | Refit `IAnonymizationApi` | HTTPS |
| Microsoft Graph | Refit `IGraphApi` | HTTPS (foto perfil SSO) |
| OCR workers | RabbitMQ | Filas OcrQueue / Response |
| API HTTP externa | RabbitMQ | ApiRequestQueue / Response |

---

## Multitenancy (visão)

```
Request + JWT + X-Tenant
    → MultiTenant middleware valida binding
    → Resolve DatabaseName (Redis cache)
    → ConnectionString em HttpContext.Items
    → ApplicationDbContext usa banco do tenant
```

---

## Extensão de funcionalidade

Checklist canônico: **BACKEND_ARCHITECTURE §21** + **SDD README (fluxo agente)**

Ordem: `Domain → Repository (+ migration) → Application → Api → Frontend (service, page, i18n) → Tests`

---

## Documentação relacionada

- Diagramas → [`02-diagramas.md`](./02-diagramas.md)
- Tecnologias → [`03-tecnologias.md`](./03-tecnologias.md)
- Módulos detalhados → [`../04-design-detalhado/02-design-modulos.md`](../04-design-detalhado/02-design-modulos.md)
