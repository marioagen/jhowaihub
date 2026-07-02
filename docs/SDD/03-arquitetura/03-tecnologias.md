# 03 — Tecnologias

> Parte de [`../README.md`](../README.md) · Arquitetura do sistema

---

## Backend

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| .NET | 8.0 | Runtime API e Functions |
| ASP.NET Core | 8 | Web API, JWT, SignalR |
| EF Core | 8.0.14 | ORM Code First |
| SQL Server | — | Persistência por tenant |
| Redis | StackExchange | Cache tenant/user access |
| RabbitMQ | via MassTransit.Client / custom | Mensageria |
| Refit | — | Clientes HTTP tipados |
| FluentValidation | — | Validação DTO/model |
| AutoMapper | — | Mapeamento entidade↔DTO |
| Swashbuckle | 8.x | Swagger |
| Serilog | — | Logging (Application) |
| Argon2 | — | Hash senhas |
| xUnit + Moq | — | Testes unitários |

---

## Frontend

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| Vue | 3.2+ | Framework UI |
| Vite | 7.x | Build/dev server |
| Vue Router | 4 | SPA routing (hash mode) |
| Vuex | 4 | State global |
| Bootstrap | 5.0.2 | Grid, componentes base |
| Lucide Vue Next | — | Ícones |
| VeeValidate + Yup | 4.x | Formulários |
| vue-i18n | 11.x | Traduções |
| Axios | 1.x | HTTP client |
| SignalR client | 9.x | Notificações |
| ApexCharts | 5.x | Dashboard |
| @vue-flow/core | — | Editor de fluxo |

---

## Infraestrutura e CI

| Item | Detalhe |
|------|---------|
| Docker | Imagens backend, frontend, FileRepository, Functions |
| GitHub Actions | build.yml, format-check.yml, BuildImage*.yml |
| EditorConfig + Prettier | Formatação; pre-commit hook |
| Node | 18+ dev; 20 no CI format |

---

## URLs de desenvolvimento

| Serviço | URL |
|---------|-----|
| API + Swagger | https://localhost:7045/swagger |
| Frontend | http://localhost:3000 |
| Health | https://localhost:7045/healthz |
| SignalR | https://localhost:7045/hubs/notifications |

---

## Justificativas de escolha (resumo)

| Escolha | Por quê |
|---------|---------|
| SQL por tenant | Isolamento forte multi-tenant enterprise |
| RabbitMQ | Desacoplar OCR/LLM/API de longa duração |
| Vue 3 + Bootstrap | Base existente; componentes globais maduros |
| JWT + headers | Stateless API; tenant explícito por request |
| EF Code First | Migrations versionadas; alinhado ao time .NET |
| Refit | Contratos tipados para múltiplas APIs externas |

---

## Documentação relacionada

- Setup local → [`../07-operacional/01-deploy.md`](../07-operacional/01-deploy.md)
- Backend detalhado → [`../../BACKEND_ARCHITECTURE.md`](../../BACKEND_ARCHITECTURE.md) §1
- Frontend detalhado → [`../../PRODUCT_DESIGN.md`](../../PRODUCT_DESIGN.md) §15
