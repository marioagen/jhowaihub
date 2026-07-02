# 03 — Interfaces e Contratos

> Parte de [`../README.md`](../README.md) · Design detalhado

---

## API REST — convenções

| Aspecto | Padrão |
|---------|--------|
| Base | `https://{host}/api/{Controller}` |
| Integração | `/api/integration/{Controller}` |
| Auth | `Authorization: Bearer {jwt}` |
| Content-Type | `application/json` (exceto upload multipart) |
| Paginação | Query `page`, `pageSize` ou endpoint `/Paged` |
| Swagger | `[SwaggerOperation]` + `[ProducesResponseType]` |

---

## Headers HTTP obrigatórios

Bindados via `HeadersDto`:

| Header | Constante | Obrigatório |
|--------|-----------|-------------|
| `X-Email` | `HeaderNames.XEmail` | Sim (operações autenticadas) |
| `X-Tenant` | `HeaderNames.XTenant` | Sim |
| `X-Language` | `HeaderNames.XLanguage` | Sim (pt/en/es) |
| `X-Key-Mongo-Access` | `HeaderNames.XKeyMongoAccess` | Contextual (indexer) |

Frontend envia via `src/services/api.js` (interceptors).

---

## Resposta de erro (ProblemDetails)

```json
{
  "title": "An error occurred",
  "status": 400,
  "detail": "Entity not found",
  "errorCode": 3,
  "labelError": "document.notFound"
}
```

| errorCode | Enum | Uso frontend |
|-----------|------|--------------|
| 3 | NotFound | Toast danger |
| 11 | BusinessWarningOutput | Toast warning |
| … | Ver ErrorCode enum | Mapear via `$t(labelError)` |

---

## Resposta de sucesso — padrões

| Operação | Retorno típico |
|----------|----------------|
| Listagem paginada | `{ items, totalCount, page, pageSize, totalPages }` |
| Create/Update | `true` ou DTO criado |
| Find by id | DTO |
| Delete | `Ok()` ou `BadRequest` |

---

## SignalR

| Item | Valor |
|------|-------|
| URL | `/hubs/notifications` |
| Auth | `?access_token={jwt}` |
| Hub | `NotificationHub` |
| Agrupamento | Por email (`IConnectionMappingService`) |

Eventos consumidos no frontend: `signalRServices.js`

---

## Refit — APIs externas

Interfaces em `Domain/Interfaces/Refit/`:

| Cliente | Base config |
|---------|-------------|
| `IFileRepositoryApi` | FileRepositoryApiBaseAddress |
| `IChatCompletionApi` | AiGatewayApiBaseAddress |
| `IEmbeddingsApi` | IndexerApiBaseAddress |
| `IMarketPlaceApi` | MarketPlaceBaseAddress |
| `IAnonymizationApi` | AnonymizationApiBaseAddress |
| `IGraphApi` | GraphApiBaseAddress |
| `IAzureAiSearch` | IntegrationApiBaseAddress |

---

## RabbitMQ — contrato de mensagem

Padrão em automação:

```csharp
// Publicação
ExecutionMessageDto { Queue, Message }

// Consumer processa → salva output →
AutomationServices.ContinueExecution(AutomationServicesDto)
```

Filas: ver `MessageQueues` + BACKEND_ARCHITECTURE §10.

---

## Frontend — contrato de serviço JS

```javascript
// src/services/{modulo}/{Modulo}Service.js
import api from '@/services/api';

export default {
    FindAllPaged(params) {
        return api.get('/MeuController', { params });
    },
    Create(dto) {
        return api.post('/MeuController', dto);
    },
};
```

Headers injetados automaticamente pelo interceptor (tenant, email, language).

---

## i18n — contrato de chaves

```javascript
// pt.js — espelhar em en.js e es.js
meuModulo: {
    title: 'Título',
    subtitle: 'Subtítulo',
    saveSuccess: 'Salvo com sucesso.',
}
```

Uso: `$t('meuModulo.title')` — **nunca** string literal na template.

---

## Permissões — contrato JWT

Frontend: `hasPermission(module, action)` em `utils/permissions.js`

Backend meta router: `meta: { module: 'Tools', action: 'Prompts' }`

Novos módulos: registrar permissão no seed/migration + frontend menu.

---

## Catálogo de controllers

Lista completa: BACKEND_ARCHITECTURE §20

---

## Documentação relacionada

- Fluxos → [`04-fluxos.md`](./04-fluxos.md)
- Auth → [`../05-seguranca/01-autenticacao-autorizacao.md`](../05-seguranca/01-autenticacao-autorizacao.md)
- Templates API → [`../../API_TEMPLATES.md`](../../API_TEMPLATES.md)
