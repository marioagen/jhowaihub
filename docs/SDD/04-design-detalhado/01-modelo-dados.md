# 01 — Modelo de Dados

> Parte de [`../README.md`](../README.md) · Design detalhado

Referência completa: [`../../BACKEND_ARCHITECTURE.md`](../../BACKEND_ARCHITECTURE.md) §4

---

## Convenções

- Entidades persistidas herdam `BaseEntity` (`Id: int`, `Created: DateTime`)
- Mappings Fluent API em `Repository/Mappings/*Map.cs`
- DbContext: `ApplicationDbContext`
- Migrations: `Repository/Migrations/`

---

## Grupos de entidades

### Documental

| Entidade | Tabela lógica | Chave relações |
|----------|---------------|----------------|
| `Document` | Documentos | → Cards, Workflows, Histories |
| `DocumentHistory` | Histórico | → Document |
| `DocumentNormalized` | Texto normalizado | 1:1 Document |
| `DocumentBatch` | Lotes upload | → Cards |
| `DocumentAnalysisRejection` | Reprovações | → Document |
| `DocumentAnonymization` | Versões anonimizadas | → Document |

### Esteira

| Entidade | Descrição |
|----------|-----------|
| `Workflow` | Esteira (Name, Enable, Teams, Steps) |
| `Step` | Etapa (Order, ProfileId, StatusId) |
| `Card` | Documento na etapa (StepId, DocumentId, StatusId, AssignedUserId) |
| `Status` | Status do card |

### Ferramentas e execução

| Entidade | Descrição |
|----------|-----------|
| `ToolType` | OCR, Embeddings, Prompt, N8N, API, Quiz |
| `Tool` | Instância configurável |
| `ToolData` | Dados auxiliares |
| `StepTool` | Tool na etapa (Order) |
| `StepToolParameter` | JSON criptografado |
| `StepToolDependency` | Dependência entre tools |
| `StepToolExecution` | Execução assíncrona |
| `StepToolOutput` | Resultado (JSON) |
| `ApiTemplate` | Template HTTP |
| `Prompt` / `PromptApiTemplate` | Agentes |

### Questionários

`Questionnaire` ↔ `Question` (via `QuestionQuestionnaire`)

### Acesso

`User` → `Profile` → `Permission`  
`User` ↔ `Team`  
`StepProfilePermission` — permissão por etapa

### Consumo e auditoria

`UsageDaily`, `UsageMonth`, `UsageLog`, `UsageType`, `UsageUnit`, `SubscriptionPeriod`  
`AuditLog`, `AuditCard`

---

## Diagrama relacional (esteira + execução)

```
Workflow
  └── Step (Order)
        ├── StepTool (Order) ── Tool ── ToolType
        │     ├── StepToolParameter (encrypted)
        │     ├── StepToolDependency
        │     ├── StepToolExecution ── Card
        │     └── StepToolOutput ── Card
        └── Card ── Document
```

---

## Padrão para nova entidade (SDD)

```markdown
### Entidade: {Nome}
- **Propósito:** ...
- **Campos:** ...
- **Relações:** ...
- **Índices:** ...
- **Auditoria:** sim/não
- **Migration:** {NomeDaMigration}
```

Implementação:
1. `Domain/Models/{Entity}.cs`
2. `Domain/Interfaces/Repository/I{Entity}Repository.cs`
3. `Repository/Mappings/{Entity}Map.cs`
4. `DbSet` + `OnModelCreating`
5. `dotnet ef migrations add ...`

---

## DTOs (não persistidos)

Organização em `Domain/DTOs/`:
- `Request/` — entrada API (inclui `HeadersDto`)
- `Response/` — saída API
- `Messaging/` — payloads RabbitMQ

**Regra:** Controllers retornam DTOs, nunca entidades EF diretamente.

---

## Documentação relacionada

- Interfaces API → [`03-interfaces.md`](./03-interfaces.md)
- Módulos → [`02-design-modulos.md`](./02-design-modulos.md)
