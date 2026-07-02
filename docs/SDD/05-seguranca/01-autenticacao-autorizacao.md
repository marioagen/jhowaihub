# 01 — Autenticação e Autorização

> Parte de [`../README.md`](../README.md) · Segurança

Detalhe backend: [`../../BACKEND_ARCHITECTURE.md`](../../BACKEND_ARCHITECTURE.md) §6

---

## Autenticação

### Mecanismos

| Mecanismo | Endpoint | Token |
|-----------|----------|-------|
| Email/senha | `POST /api/Account/login` | JWT |
| SSO Microsoft | `POST /api/Account/login-sso` | JWT (+ tokenAzure Graph) |
| API interna | `POST /api/Account/authenticateApi` | JWT |

### JWT

- Algoritmo: simétrico (`JWT:Key`)
- Claims: email, permissões, `tenant` (`JwtClaimNames.Tenant`)
- Expiração: `AccessTokenExpirationMinutes` (default 60)
- Refresh: `RefreshTokenServices` (7 dias default)

### Envio

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
X-Email: usuario@empresa.com
X-Tenant: tenant-id
X-Language: pt
```

### SignalR

Token via query string na conexão WebSocket (ver `Program.cs` JwtBearerEvents).

---

## Autorização

### Camada API

- Controllers protegidos: `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`
- Exceções: `AccountController` (login), endpoints com `[OptionalTenantHeader]`

### Camada frontend

- Router guard: `hasPermission(to.meta.module, to.meta.action)`
- Rota `/unauthorized` se negado
- Menu sidebar filtrado por permissões JWT

### Permissões (exemplos)

| Module | Action | UI |
|--------|--------|-----|
| Documents | View | Esteiras operacionais |
| Workflow | View | Kanban documentos |
| WorkflowManagement | View | Gestão de esteiras |
| Tools | Prompts / Quizzes / Connectors / APIs | Ferramentas |
| Management | Users / Teams / Profiles | Gestão |
| Dashboard | View | Painel consumo |
| Auditor | View | Auditoria |
| DocumentRejection | — | Reprovar documento |

---

## Multitenancy (autorização de dados)

```
ITenantBindingValidator.TryValidateRequestBindingAsync
  → Compara X-Tenant header com claim JWT
  → Valida acesso marketplace (cache)
  → Falha → HTTP 403 { error: "Tenant mismatch or missing." }
```

Middleware: `MultiTenant.cs` — executa **após** `UseAuthentication`.

---

## SSO Microsoft

1. Frontend obtém `clientId` → `GET /api/Account/clientId`
2. MSAL popup → access token Graph
3. `POST /api/Account/login-sso` com token Azure
4. Backend valida e emite JWT próprio

---

## Checklist segurança (nova feature)

- [ ] Endpoints autenticados com `[Authorize]`
- [ ] Headers tenant/email propagados
- [ ] Permissão registrada (backend seed + frontend router + menu)
- [ ] Dados filtrados por tenant (connection string correta)
- [ ] Secrets não expostos em DTOs de resposta
- [ ] labelError sem dados sensíveis

---

## Documentação relacionada

- Controles → [`02-controles-seguranca.md`](./02-controles-seguranca.md)
- Multitenancy → BACKEND_ARCHITECTURE §7
