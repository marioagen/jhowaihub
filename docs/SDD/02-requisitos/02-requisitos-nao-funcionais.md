# 02 — Requisitos Não Funcionais

> Parte de [`../README.md`](../README.md) · Requisitos do sistema

---

## Performance

| ID | Requisito | Referência |
|----|-----------|------------|
| RNF-PERF-01 | Upload chunked — não carregar arquivo inteiro em memória HTTP | `DocumentUploadServices` |
| RNF-PERF-02 | Processamento IA assíncrono via RabbitMQ | BACKEND_ARCHITECTURE §9–10 |
| RNF-PERF-03 | Cache Redis para metadata de tenant | `ITenantCacheServices` |
| RNF-PERF-04 | Paginação obrigatória em listagens grandes | `TableComponent`, endpoints `Paged` |
| RNF-PERF-05 | SignalR keep-alive 15s, timeout cliente 60s | `Program.cs` |

---

## Escalabilidade e disponibilidade

| ID | Requisito |
|----|-----------|
| RNF-ESC-01 | Banco isolado por tenant (`___NEWDB___` template) |
| RNF-ESC-02 | Consumers RabbitMQ stateless; tenant resolvido por mensagem |
| RNF-ESC-03 | Health check em `/healthz` |
| RNF-ESC-04 | Dead letter consumers para filas críticas |

---

## Segurança

| ID | Requisito | Detalhe |
|----|-----------|---------|
| RNF-SEC-01 | JWT obrigatório (exceto login) | Bearer + claim `tenant` |
| RNF-SEC-02 | Binding X-Tenant ↔ JWT | Middleware MultiTenant → 403 |
| RNF-SEC-03 | Senhas Argon2 | `Argon2PasswordHasher` |
| RNF-SEC-04 | Parâmetros StepTool criptografados | `AesGcmEncryptionService` |
| RNF-SEC-05 | CORS explícito — API não inicia sem `CORS` | appsettings |
| RNF-SEC-06 | Segredos via env/User Secrets, não versionados | — |

Ver [`../05-seguranca/`](../05-seguranca/)

---

## Usabilidade e acessibilidade

| ID | Requisito | Referência |
|----|-----------|------------|
| RNF-UX-01 | Idioma PT primário; EN/ES completos | vue-i18n |
| RNF-UX-02 | Tema claro/escuro persistente | `css-theme-light/dark` |
| RNF-UX-03 | Feedback imediato (loading, toast, confirmação destrutiva) | PRODUCT_DESIGN §12 |
| RNF-UX-04 | Sidebar colapsável; responsivo < 768px | PRODUCT_DESIGN §13 |
| RNF-UX-05 | Vocabulário consistente (Esteira, Agente, etc.) | glossário |

---

## Manutenibilidade

| ID | Requisito | Referência |
|----|-----------|------------|
| RNF-MAN-01 | Clean Architecture em camadas | BACKEND_ARCHITECTURE §3 |
| RNF-MAN-02 | Métodos ≤ 20 linhas (regra AGENTS.md) | AGENTS.md |
| RNF-MAN-03 | `Find*` para leitura; `AppException` para negócio | AGENTS.md |
| RNF-MAN-04 | XML summary em métodos públicos novos | AGENTS.md |
| RNF-MAN-05 | Prettier/EditorConfig no pre-commit | GIT_HOOKS |
| RNF-MAN-06 | Migrations EF Code First versionadas | Repository/Migrations |

---

## Compatibilidade

| ID | Requisito |
|----|-----------|
| RNF-CMP-01 | .NET 8 SDK |
| RNF-CMP-02 | Node 18+ (CI usa 20) para frontend |
| RNF-CMP-03 | SQL Server (EF SqlServer provider) |
| RNF-CMP-04 | Browsers: >1%, last 2 versions (browserslist) |

---

## Observabilidade

| ID | Requisito |
|----|-----------|
| RNF-OBS-01 | Logs estruturados em consumers e exception handler |
| RNF-OBS-02 | Métricas de uso em `UsageDaily` / `UsageMonth` |
| RNF-OBS-03 | Swagger em Development |

---

## Documentação relacionada

- RF → [`01-requisitos-funcionais.md`](./01-requisitos-funcionais.md)
- Segurança detalhada → [`../05-seguranca/02-controles-seguranca.md`](../05-seguranca/02-controles-seguranca.md)
