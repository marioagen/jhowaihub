# 02 — Monitoramento

> Parte de [`../README.md`](../README.md) · Operação

---

## Health check

```
GET /healthz
```

Registrado em `Program.cs` via `AddHealthChecks()`.

---

## Logs

| Fonte | Conteúdo |
|-------|----------|
| `GlobalExceptionHandler` | Exceções não tratadas (500) |
| Consumers (`ILogger`) | Falhas processamento fila |
| Services | Erros de negócio relevantes |

Nível default: `Information` (appsettings Logging).

---

## Métricas de negócio

Persistidas em SQL (por tenant):

| Tabela | Métrica |
|--------|---------|
| `UsageDaily` | Consumo diário (pages, tokens, automation…) |
| `UsageMonth` | Agregação mensal |
| `UsageLog` | Log detalhado |

`UsageAccountingConsumer` processa fila de contabilização.

Azure Functions:
- `ManageConsumptionsFunction`
- `ResetMonthMetricsFunction`

---

## SignalR (observabilidade UX)

Progresso de automação notificado em tempo real — não substitui logs backend, mas dá feedback ao usuário.

Frontend: `NavbarNotificationComponent` + store upload notifications.

---

## RabbitMQ

Monitorar:
- Tamanho das filas (OCR, Embedding, API response…)
- Dead letter queues (`*DeadLetterConsumer`)
- Retry config: `MaxRetryAttempts: 4`

---

## CI/CD observability

| Workflow | Verifica |
|----------|----------|
| `build.yml` | Build + testes .NET |
| `format-check.yml` | Prettier/format changed |

---

## Alertas recomendados (produção)

- Health check failing
- Fila RabbitMQ acima de threshold
- Taxa de erro 5xx na API
- Redis indisponível (cache tenant)
- Consumer parado (lag crescente)

---

## Documentação relacionada

- Deploy → [`01-deploy.md`](./01-deploy.md)
- Filas → BACKEND_ARCHITECTURE §10
