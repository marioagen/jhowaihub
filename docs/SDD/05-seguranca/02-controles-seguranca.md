# 02 — Controles de Segurança

> Parte de [`../README.md`](../README.md) · Segurança

---

## Proteção de dados

| Controle | Implementação |
|----------|---------------|
| Isolamento tenant | Banco SQL dedicado por tenant |
| Criptografia parâmetros | `AesGcmEncryptionService` em `StepToolParameter` |
| Hash senhas | Argon2 (`Argon2PasswordHasher`) |
| Segredos config | Variáveis de ambiente / User Secrets — não commitar |
| Upload | File Repository isolado; referenceFile opaco |

---

## Controles de rede

| Controle | Detalhe |
|----------|---------|
| HTTPS | Redirection habilitado na API |
| CORS | Origem única configurada — obrigatória |
| API Keys externas | IndexerApiKey, KeyAccess, FunctionApiKey em config |

---

## Auditoria e compliance

| Controle | Detalhe |
|----------|---------|
| AuditLog automático | `ApplicationDbContext.SaveChanges` intercepta alterações |
| AuditCard | Ações em cards/esteira |
| AuditorServices | Consulta histórico documentos/workflows/usuários |
| DocumentHistory | Histórico por documento (tools, questionários) |

Usuário auditoria: JWT email ou header `X-Email` (fallback).

---

## Validação de entrada

| Camada | Mecanismo |
|--------|-----------|
| DTO | FluentValidation (`AddValidation`) |
| Negócio | `AppException` + ErrorCode |
| API externa | Refit + tratamento `ApiException` |
| JSON | Serialização System.Text.Json; ciclos ignorados |

---

## Mensageria segura

- Mensagens carregam `Tenant` + `Email` para resolver banco correto
- Consumers criam scope isolado por mensagem
- Dead letter queues para análise de falhas
- Retry configurável: `MaxRetryAttempts`, `InitialRetryDelaySeconds`

---

## Frontend

| Controle | Detalhe |
|----------|---------|
| Token storage | Vuex + localStorage (`project`, theme) |
| Logout | Limpa store e redireciona |
| XSS i18n | `escapeParameter: true`, `warnHtmlMessage: false` |
| Tema/logos | Sem embed de scripts externos não confiáveis |

---

## ErrorCode sensível

Não expor stack trace ao cliente — `GlobalExceptionHandler` retorna mensagem genérica para 500.

`labelError` deve ser chave i18n, não mensagem técnica.

---

## Checklist agente (segurança)

```
□ Query filtra por tenant implicitamente (via DbContext)?
□ Parâmetro sensível criptografado antes de persistir?
□ Endpoint valida permissão de negócio no Service?
□ Logs não gravam senhas/tokens/chaves?
□ Novo secret documentado apenas em appsettings exemplo vazio?
```

---

## Documentação relacionada

- Auth → [`01-autenticacao-autorizacao.md`](./01-autenticacao-autorizacao.md)
- RNF segurança → [`../02-requisitos/02-requisitos-nao-funcionais.md`](../02-requisitos/02-requisitos-nao-funcionais.md)
