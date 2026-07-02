# 01 — Deploy e Ambientes

> Parte de [`../README.md`](../README.md) · Operação

Setup completo: [`../../README.md`](../../README.md) (raiz do repositório)

---

## Pré-requisitos locais

| Componente | Versão |
|------------|--------|
| .NET SDK | 8.0 |
| Node.js | 18+ (recomendado 20) |
| SQL Server | Local ou remoto |
| Redis | localhost:6379 |
| RabbitMQ | localhost |

---

## Configuração inicial

### Backend

1. Editar `back-end/WoopiAiHub.Api/appsettings.json`:
   - `ConnectionStrings:TemplateConnection`
   - `ConnectionStrings:Redis`
   - `CORS` → origem do frontend (ex: `http://localhost:3000`)
   - `JWT:*`, `Messaging:*`, `RefitExternalSettings:*`
2. `dotnet ef database update --project ../WoopiAiHub.Repository` (a partir de `WoopiAiHub.Api`)

### Frontend

1. `front-end/vueapp/.env` → `VUE_APP_BASE_URL_API=https://localhost:7045`
2. `npm install && npm run dev`

### Hooks

```bash
npm run setup:hooks
```

---

## Executar localmente

```bash
# Terminal 1 — API
cd back-end/WoopiAiHub.Api
dotnet run

# Terminal 2 — Frontend
cd front-end/vueapp
npm run dev
```

| Serviço | URL |
|---------|-----|
| Swagger | https://localhost:7045/swagger |
| Frontend | http://localhost:3000 |
| Health | https://localhost:7045/healthz |

---

## Build produção

```bash
# Backend
dotnet build back-end/WoopiAiHub.Api/WoopiAiHub.Api.csproj

# Frontend
cd front-end/vueapp && npm run build
```

---

## Docker (CI)

Workflows GitHub Actions:
- `BuildImageBackEnd.yml`
- `BuildImageFrontEnd.yml`
- `BuildImageFileRepository.yml`
- `BuildImageFunctions.yml`

---

## Ambientes

| Ambiente | Notas |
|----------|-------|
| Development | Swagger on; appsettings.Development.json |
| QA/Prod | Secrets via env; CORS origem real; HTTPS |

**Nunca** commitar segredos reais (Google keys, JWT keys, RabbitMQ passwords).

---

## Provisionamento tenant

- Template connection com `___NEWDB___`
- Marketplace / TenantServices cria banco
- `InitApplicationDb.RunApplicationMigration` em runtime (fluxo tenant)

---

## Documentação relacionada

- Monitoramento → [`02-monitoramento.md`](./02-monitoramento.md)
- Tecnologias → [`../03-arquitetura/03-tecnologias.md`](../03-arquitetura/03-tecnologias.md)
