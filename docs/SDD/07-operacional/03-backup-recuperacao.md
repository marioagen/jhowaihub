# 03 — Backup e Recuperação

> Parte de [`../README.md`](../README.md) · Operação

---

## Dados por tenant

Cada tenant possui **banco SQL Server isolado** (`DatabaseName` no cache tenant).

### Backup recomendado

| Asset | Estratégia |
|-------|------------|
| SQL Server (por tenant) | Backup full diário + log incremental (política DBA) |
| Redis | Cache reconstituível — backup opcional |
| RabbitMQ | Filas transientes — mensagens não são source of truth |
| File Repository (Blob) | Backup/geo-redundancy Azure Storage (política infra) |

---

## Source of truth

| Dado | Onde |
|------|------|
| Metadados documento, esteiras, usuários | SQL tenant |
| Arquivo binário | Azure Blob via File Repository |
| Outputs IA (OCR, prompt…) | SQL (`StepToolOutput`, `DocumentHistory`) |
| Sessão frontend | JWT (stateless) + localStorage |

---

## Recuperação de desastre

1. Restaurar banco SQL do tenant afetado
2. Validar migrations aplicadas (`dotnet ef database update`)
3. Verificar Redis (reaquecer cache tenant)
4. File Repository — arquivos independentes; `ReferenceFile` deve existir no blob
5. Reprocessar cards falhos via `FailingCardService` / reprocessamento manual se necessário

---

## Exclusão de dados

- Document delete → fila `DeleteQueueConsumer` / `DeleteQueuePublisher`
- Soft delete: `Enable = false` em Document/Card quando aplicável
- Auditoria mantém trilha mesmo após desativação

---

## Migrations

- Sempre versionadas em git (`Repository/Migrations/`)
- Rollback: estratégia forward-only — criar migration corretiva, não reverter em prod sem plano

---

## Retenção

Definir por contrato enterprise (fora do código):
- AuditLog / DocumentHistory
- UsageLogs
- Arquivos blob

---

## Documentação relacionada

- Deploy → [`01-deploy.md`](./01-deploy.md)
- Modelo de dados → [`../04-design-detalhado/01-modelo-dados.md`](../04-design-detalhado/01-modelo-dados.md)
