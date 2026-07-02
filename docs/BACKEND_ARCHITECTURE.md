# WOOPI AI Hub — Documentação Técnica de Backend

Guia de referência para desenvolvedores, arquitetos e assistentes de IA que constroem **novos serviços agregados** ao repositório WOOPI AI Hub. Complementa [`PRODUCT_DESIGN.md`](./PRODUCT_DESIGN.md) (frontend/UX) e [`API_TEMPLATES.md`](./API_TEMPLATES.md) (módulo específico de Templates de API).

Este documento descreve a **arquitetura completa do backend**, padrões de código, fluxos de dados e o passo a passo para estender a plataforma corretamente.

---

## Índice

1. [Visão geral](#1-visão-geral)
2. [Estrutura da solução](#2-estrutura-da-solução)
3. [Arquitetura em camadas](#3-arquitetura-em-camadas)
4. [Modelo de domínio](#4-modelo-de-domínio)
5. [Camada API (Controllers)](#5-camada-api-controllers)
6. [Autenticação, autorização e headers](#6-autenticação-autorização-e-headers)
7. [Multitenancy](#7-multitenancy)
8. [Camada Application (Services)](#8-camada-application-services)
9. [Automação e Tool Handlers](#9-automação-e-tool-handlers)
10. [Mensageria (RabbitMQ)](#10-mensageria-rabbitmq)
11. [Camada Repository](#11-camada-repository)
12. [Camada Infrastructure](#12-camada-infrastructure)
13. [Integrações externas (Refit)](#13-integrações-externas-refit)
14. [SignalR e notificações em tempo real](#14-signalr-e-notificações-em-tempo-real)
15. [Tratamento de erros](#15-tratamento-de-erros)
16. [Validação](#16-validação)
17. [Configuração (appsettings)](#17-configuração-appsettings)
18. [Banco de dados e migrations](#18-banco-de-dados-e-migrations)
19. [Projetos satélite](#19-projetos-satélite)
20. [Catálogo de controllers](#20-catálogo-de-controllers)
21. [Guia: adicionar um novo serviço/módulo](#21-guia-adicionar-um-novo-serviçomódulo)
22. [Testes de unidade](#22-testes-de-unidade)
23. [Referências de código](#23-referências-de-código)

---

## 1. Visão geral

### Stack

| Componente | Tecnologia |
|------------|------------|
| Runtime | .NET 8 (`net8.0`) |
| API | ASP.NET Core Web API |
| ORM | Entity Framework Core 8.0.14 (Code First) |
| Banco | SQL Server (por tenant) |
| Cache | Redis (StackExchange) |
| Mensageria | RabbitMQ (via abstração `IMessagePublisher` / `IMessageConsumer`) |
| HTTP externo | Refit |
| Validação | FluentValidation |
| Mapeamento | AutoMapper |
| Auth | JWT Bearer + Refresh Token (Argon2 para senhas) |
| Tempo real | SignalR |
| Documentação API | Swagger (Swashbuckle) |

### Responsabilidades centrais

- Gestão **multi-tenant** de documentos, esteiras (workflows), cards e ferramentas de IA
- Processamento **assíncrono** via filas: OCR, Embeddings, Prompt/LLM, Quiz, N8N, API externa
- Autenticação JWT, permissões por perfil, auditoria de alterações
- Integração com APIs externas: File Repository, Indexer, AI Gateway, Marketplace, Anonimização, Graph Microsoft

### Princípios arquiteturais

1. **Clean Architecture simplificada** — dependências apontam para o Domain; Api → Application → Repository/Infrastructure → Domain
2. **Single Responsibility por camada** — Controller orquestra HTTP; Service contém regra de negócio; Repository acessa dados
3. **Interfaces no Domain** — contratos de Services, Repository e Handlers ficam em `WoopiAiHub.Domain/Interfaces/`
4. **Métodos de leitura com prefixo `Find`** — nunca `Get` (convenção do repositório, ver `AGENTS.md`)
5. **Injeção de dependência** — registro centralizado em `DependencyInjection/Extension.cs` de cada projeto
6. **Async end-to-end** — propagar `CancellationToken` quando aplicável

---

## 2. Estrutura da solução

```
woopiai-hub/
├── back-end/
│   ├── WoopiAiHub.Api/              # Controllers, Hubs, Exception handlers, Program.cs
│   ├── WoopiAiHub.Application/      # Services, Consumers, ToolHandlers, Utils
│   ├── WoopiAiHub.Domain/           # Models, DTOs, Interfaces, Enums, Validations
│   ├── WoopiAiHub.Infrastructure/  # RabbitMQ, Multitenancy context
│   ├── WoopiAiHub.Repository/       # EF Core, Repositories, Mappings, Migrations, Middleware
│   └── WoopiAiHub.Functions/        # Azure Functions (métricas/consumo)
├── external-api/
│   └── FileRepository.*             # API separada de armazenamento de arquivos
├── front-end/vueapp/                # SPA Vue 3 (ver PRODUCT_DESIGN.md)
└── tests/WoopiAiHub.UnitTests/      # Testes xUnit + Moq.AutoMock
```

### Dependências entre projetos

```
WoopiAiHub.Api
  ├── WoopiAiHub.Application
  ├── WoopiAiHub.Repository
  ├── WoopiAiHub.Infrastructure
  └── WoopiAiHub.Domain

WoopiAiHub.Application
  ├── WoopiAiHub.Repository (referência direta para alguns registros DI)
  ├── WoopiAiHub.Infrastructure
  └── WoopiAiHub.Domain

WoopiAiHub.Repository
  └── WoopiAiHub.Domain

WoopiAiHub.Infrastructure
  └── WoopiAiHub.Domain
```

---

## 3. Arquitetura em camadas

```
┌─────────────────────────────────────────────────────────────────┐
│  WoopiAiHub.Api                                                 │
│  Controllers · Hubs · GlobalExceptionHandler · Attributes       │
└────────────────────────────┬────────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────────┐
│  WoopiAiHub.Application                                         │
│  Services · ToolHandlers · Consumers · Validation helpers       │
└────────────┬───────────────────────────────┬────────────────────┘
             │                               │
┌────────────▼────────────┐    ┌─────────────▼─────────────────────┐
│  WoopiAiHub.Repository  │    │  WoopiAiHub.Infrastructure        │
│  EF Core · Repositories │    │  RabbitMQ · TenantContextService  │
│  Migrations · Middleware│    │                                   │
└────────────┬────────────┘    └─────────────┬─────────────────────┘
             │                               │
             └───────────────┬───────────────┘
                             │
┌────────────────────────────▼────────────────────────────────────┐
│  WoopiAiHub.Domain                                              │
│  Models · DTOs · Interfaces · Enums · Utils · Validations       │
└─────────────────────────────────────────────────────────────────┘
```

### Fluxo de uma requisição HTTP autenticada

```
Request
  → CORS
  → Authentication (JWT)
  → Authorization
  → MultiTenant middleware (valida X-Tenant, seta TenantConnection)
  → Controller
  → Service(s)
  → Repository(ies) / Refit client / MessagePublisher
  → ApplicationDbContext (SQL Server do tenant)
  → Response (JSON) ou ProblemDetails (erro)
```

### Bootstrap da API (`Program.cs`)

Ordem de registro relevante:

1. `AddExternalApi` — clientes Refit
2. `AddRepository` — EF Core, repositórios, Redis
3. `AddValidation` — FluentValidation
4. `AddInfrastructure` — RabbitMQ
5. `AddApplication` — services, handlers, consumers
6. JWT + SignalR + Swagger + CORS
7. Pipeline: CORS → Auth → MultiTenant → Controllers → ExceptionHandler

---

## 4. Modelo de domínio

### Entidades principais (`WoopiAiHub.Domain/Models/`)

Todas herdam de `BaseEntity` (`Id: int`, `Created: DateTime`).

#### Núcleo documental

| Entidade | Descrição |
|----------|-----------|
| `Document` | Arquivo lógico (nome, referenceFile, status, emailCreator) |
| `DocumentHistory` | Histórico de ações no documento |
| `DocumentNormalized` | Texto normalizado/OCR |
| `DocumentBatch` | Lote de upload |
| `DocumentAnalysisRejection` | Reprovações com justificativa |
| `DocumentAnonymization` | Versões anonimizadas |

#### Esteira (Workflow)

| Entidade | Descrição |
|----------|-----------|
| `Workflow` | Esteira (nome, descrição, enable, teams, steps) |
| `Step` | Etapa ordenada (profileId, statusId, stepTools) |
| `Card` | Instância documento+etapa na esteira (status, assignedUser) |
| `Status` | Status do card/etapa |

#### Ferramentas (Tools)

| Entidade | Descrição |
|----------|-----------|
| `ToolType` | Tipo: OCR, Embeddings, Prompt, N8N, API, Quiz |
| `Tool` | Ferramenta configurável (nome, tipo, dados) |
| `ToolData` | Dados auxiliares da ferramenta |
| `StepTool` | Ferramenta associada a uma etapa (ordem, parâmetros) |
| `StepToolParameter` | Parâmetros criptografados |
| `StepToolOutput` | Resultado de execução |
| `StepToolExecution` | Registro de execução assíncrona |
| `StepToolDependency` | Dependência entre step tools |
| `ApiTemplate` | Template HTTP reutilizável |
| `Prompt` / `PromptApiTemplate` | Agentes de IA |

#### Questionários

| Entidade | Descrição |
|----------|-----------|
| `Questionnaire` | Conjunto de perguntas |
| `Question` | Pergunta individual |
| `QuestionQuestionnaire` | Relação N:N |

#### Gestão de acesso

| Entidade | Descrição |
|----------|-----------|
| `User` | Usuário (email, senha hash, profile, teams) |
| `Team` | Time |
| `Profile` | Perfil de permissões |
| `Permission` | Permissão granular (module + action) |
| `StepProfilePermission` | Permissão por etapa |

#### Consumo e auditoria

| Entidade | Descrição |
|----------|-----------|
| `UsageDaily` / `UsageMonth` / `UsageLog` | Métricas de consumo |
| `UsageType` / `UsageUnit` | Tipos e unidades de métrica |
| `SubscriptionPeriod` | Período de assinatura |
| `AuditLog` / `AuditCard` | Trilha de auditoria |

#### Outros

| Entidade | Descrição |
|----------|-----------|
| `TypeDoc` | Tipos de documento |
| `ModelEmbedding` | Modelos de embedding disponíveis |

### Diagrama relacional simplificado (esteira)

```
Workflow ──< Step ──< StepTool ──< StepToolExecution
   │           │          │
   │           │          └── StepToolOutput
   │           └──< Card >── Document
   └──< Team
```

### Tipos de ferramenta (`HandlersTypes`)

```csharp
OCR         → extração de texto (Azure/Google)
Embeddings  → vetorização/indexação
Prompt      → LLM / agente
N8N         → automação via conector N8N
API         → chamada HTTP parametrizável (ApiTemplate)
Quiz        → questionário estruturado
```

Constantes em `WoopiAiHub.Domain/Interfaces/Utils/HandlersTypes.cs`.

### Métricas (`MetricNames`)

| Constante | Uso |
|-----------|-----|
| `Automation` | Execuções de automação |
| `Execution` | Execuções de step tool |
| `Token` | Consumo de tokens LLM |
| `Page` | Páginas processadas (OCR) |

---

## 5. Camada API (Controllers)

Localização: `back-end/WoopiAiHub.Api/Controllers/`

### Padrão de controller

```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class MeuController : ControllerBase
{
    private readonly IMeuServices _meuServices;

    public MeuController(IMeuServices meuServices)
    {
        _meuServices = meuServices;
    }

    /// <summary>
    /// Descreve o propósito do endpoint (não repita a assinatura).
    /// </summary>
    [HttpGet]
    [SwaggerOperation("Descrição para Swagger")]
    [ProducesResponseType(typeof(MeuDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> FindAll([FromQuery] MeuPagedDto dto,
                                             [FromHeader] HeadersDto headersDto)
    {
        var result = await _meuServices.FindAllAsync(dto, headersDto);
        return Ok(result);
    }
}
```

### Convenções

| Aspecto | Padrão |
|---------|--------|
| Rota base | `api/[controller]` |
| Integração externa | `api/integration/[controller]` |
| Autenticação | `[Authorize]` com JWT (exceto Account/login) |
| Headers de contexto | `[FromHeader] HeadersDto headersDto` |
| Paginação | Endpoints `Paged` ou query params `page`/`pageSize` |
| Swagger | `[SwaggerOperation]` + `[ProducesResponseType]` |
| Upload | `[DisableRequestSizeLimit]` + `[RequestFormLimits]` |
| XML summary | Obrigatório em métodos públicos novos |

### Controllers de integração

Pasta `Controllers/Integration/` — endpoints consumidos por serviços externos (Azure AI Search, etc.), com autenticação própria.

### Attributes customizados

- `OptionalTenantHeader` — login pode omitir tenant inicial
- `SwaggerCustomHeader` — documenta headers obrigatórios no Swagger

---

## 6. Autenticação, autorização e headers

### JWT

Configuração em `appsettings.json`:

```json
"JWT": {
  "Key": "...",
  "Issuer": "https://doc-dev.woopi.ai/",
  "Audience": "https://doc-dev.woopi.ai/",
  "AccessTokenExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

- Token enviado: `Authorization: Bearer {token}`
- Claim de tenant: `tenant` (`JwtClaimNames.Tenant`)
- SignalR aceita token via query: `?access_token=...` no hub

### Fluxos de login (`AccountController`)

| Endpoint | Descrição |
|----------|-----------|
| `POST /api/Account/login` | Email + senha |
| `POST /api/Account/login-sso` | SSO Microsoft |
| `POST /api/Account/authenticateApi` | Auth por chave interna |
| `GET /api/Account/clientId` | Client ID Azure para MSAL |
| Refresh token | Via `RefreshTokenServices` |

Resposta inclui token JWT, tenant, permissões embutidas no token.

### Headers HTTP obrigatórios (requests autenticados)

Definidos em `HeaderNames` e bindados via `HeadersDto`:

| Header | Constante | Descrição |
|--------|-----------|-----------|
| `X-Email` | `XEmail` | Email do usuário autenticado |
| `X-Tenant` | `XTenant` | Identificador do tenant |
| `X-Language` | `XLanguage` | Idioma (pt/en/es) |
| `X-Key-Mongo-Access` | `XKeyMongoAccess` | Chave de acesso Mongo (legado/indexer) |

Outros headers para integrações:

| Header | Uso |
|--------|-----|
| `Api-Key` | APIs externas |
| `Key-Access` | Acesso interno |
| `x-functions-key` | Azure Functions |

### Permissões

- Armazenadas em `Permission` (module + action)
- Associadas a `Profile` → `User`
- Validadas no **frontend** via JWT; backend valida tenant e regras de negócio por serviço
- Permissão especial: `DocumentRejection` (`PermissionNames.Rejection`)

---

## 7. Multitenancy

### Modelo

- Cada **tenant** possui banco SQL Server próprio
- Connection string template usa placeholder `___NEWDB___`:

```text
Server=localhost;Database=___NEWDB___;Trusted_Connection=True;TrustServerCertificate=True;
```

- Substituído em runtime pelo `DatabaseName` do tenant (cache Redis)

### Middleware (`MultiTenant.cs`)

Pipeline após autenticação:

1. `ITenantBindingValidator.TryValidateRequestBindingAsync` — valida `X-Tenant` vs claim JWT + acesso marketplace
2. Se inválido → **403 Forbidden** JSON `{ error: "Tenant mismatch or missing." }`
3. Se válido → resolve connection string → `HttpContext.Items["TenantConnection"]`

### DbContext

`ApplicationDbContext.OnConfiguring` lê `TenantConnection` de `HttpContext.Items` e reconfigura SQL Server por request.

Consumers RabbitMQ replicam o padrão manualmente:

```csharp
httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;
```

### Cache de tenant

- `ITenantCacheServices` — metadata do tenant (Redis)
- `IUserTenantAccessCacheServices` — acesso usuário→tenant

### Inicialização de tenant

`TenantController` → `InitializeTenant/{tenant}` — prepara ambiente do tenant após login.

---

## 8. Camada Application (Services)

Localização: `back-end/WoopiAiHub.Application/Services/`

### Mapa de serviços

| Serviço | Domínio |
|---------|---------|
| `AccountServices` | Login, JWT, SSO |
| `RefreshTokenServices` | Tokens de refresh |
| `TenantServices` | Gestão de tenants |
| `DocumentServices` | CRUD documentos |
| `DocumentUploadServices` | Upload chunked → File Repository |
| `DocumentDeletionServices` | Exclusão |
| `DocumentPipelineServices` | Pipeline OCR/embeddings |
| `DocumentHistoryServices` | Histórico |
| `DocumentMetadataServices` | Metadados |
| `DocumentQuestionnaireServices` | Questionários em documentos |
| `DocumentAnalysisRejectionServices` | Reprovações |
| `AnonymizationServices` | Anonimização |
| `WorkflowServices` | CRUD esteiras, steps, tools |
| `CardServices` | Cards no kanban, avanço de etapa |
| `AutomationServices` | Orquestração de ferramentas |
| `ExecutionServices` | Execuções de step tools |
| `ApiOutputServices` | Processamento resposta API |
| `N8NServices` | Conector N8N |
| `PromptServices` | Agentes/prompts |
| `PlaygroundServices` | Playground de prompts |
| `QuestionnaireServices` / `QuestionServices` | Questionários |
| `ToolServices` / `ToolTypeServices` / `ToolDataServices` | Ferramentas |
| `ApiTemplateServices` | Templates HTTP |
| `UserServices` / `TeamServices` / `ProfileServices` | Gestão |
| `PermissionServices` | Permissões |
| `AuditorServices` / `AuditCardService` | Auditoria |
| `UsageDailyServices` / `UsageMonthServices` | Consumo |
| `SubscriptionPeriodServices` | Assinaturas |

Interfaces correspondentes: `WoopiAiHub.Domain/Interfaces/Services/`.

### Padrão de service

```csharp
public class MeuServices : IMeuServices
{
    private readonly IMeuRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MeuServices(IMeuRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Finds all items matching the filter with pagination.
    /// </summary>
    public async Task<MeuPagedResultDto> FindAllPagedAsync(MeuPagedDto dto, string email)
    {
        // regra de negócio
        // throw new AppException(ErrorCode.NotFound, "...", LabelErrorKey);
        return await _repository.FindAllPagedAsync(dto);
    }
}
```

### Utilitários Application

| Classe | Função |
|--------|--------|
| `AppException` | Exceção de negócio com `ErrorCode` + `LabelError` |
| `AesGcmEncryptionService` | Criptografia de parâmetros sensíveis |
| `Argon2PasswordHasher` | Hash de senhas |
| `CurrentUserService` | Usuário do JWT |
| `TenantBindingValidator` | Validação tenant |
| `ApiClientFactory` | Factory de clientes HTTP |
| `RagInvocationRouter` | Roteamento RAG |

### Registro DI

`WoopiAiHub.Application/DependencyInjection/Extension.cs` — método `AddApplication()`.

---

## 9. Automação e Tool Handlers

### Visão geral

Quando um documento entra em uma esteira, ferramentas configuradas nas etapas executam em sequência (com dependências). O motor central é `AutomationServices`.

### Fluxo de execução

```
1. PrepareExecutionAsync     → cria StepToolExecutions
2. StartExecution / Trigger  → inicia primeira ferramenta
3. ToolFactoryHandler        → resolve handler por tipo
4. IToolHandler.BuildPayload → monta mensagem RabbitMQ
5. IMessagePublisher         → publica na fila
6. [Worker externo processa]
7. Consumer                  → recebe resposta
8. Service.ProcessMessage    → salva output, histórico, métricas
9. AutomationServices.ContinueExecution → próxima ferramenta
10. IHubNotifier             → notifica frontend (progresso)
```

### Tool Handlers (`ToolsHandler/`)

| Handler | Type | Fila típica |
|---------|------|-------------|
| `OcrHandler` | OCR | OcrQueue |
| `EmbeddingsHandler` | Embeddings | EmbeddingQueue |
| `PromptHandler` | Prompt | ChatCompletionQueue |
| `QuizHandler` | Quiz | AnswerQueue |
| `N8NHandler` | N8N | AutomationQueueConsumer |
| `ApiHandler` | API | ApiRequestQueue |

Registrados como `IToolHandler` (múltiplas implementações). Resolvidos via `ToolFactoryHandler`:

```csharp
var handler = _toolFactoryHandler.GetHandler(toolTypeName);
var payload = await handler.BuildPayload(automationDto, input, outputs, execution);
await _messagePublisher.PublishAsync(payload.Queue, payload.Message);
```

### Interface `IToolHandler`

```csharp
public interface IToolHandler
{
    string Type { get; }
    Task<ExecutionMessageDto> BuildPayload(
        AutomationServicesDto automationServicesDto,
        StepToolParameter? input,
        ICollection<StepToolOutput> outputs,
        StepToolExecution? execution = null);
}
```

### Dados sensíveis

Parâmetros de `StepToolParameter` são **criptografados** (`IEncryptionService`) — descriptografados apenas na execução.

### Cards e avanço manual

`CardServices` gerencia movimentação de cards entre etapas, atribuição, validação de ferramentas pendentes (OCR/Embeddings), reprovação.

---

## 10. Mensageria (RabbitMQ)

### Configuração

```json
"Messaging": {
  "BrokerType": "RabbitMQ",
  "Brokers": { "RabbitMQ": { "HostName": "...", "UserName": "...", "Password": "..." } },
  "Queues": { ... }
}
```

Infrastructure registra via `AddInfrastructure()`:
- `RabbitMqManager` (singleton)
- `IMessagePublisher<T>` → `RabbitMqPublisher<T>`
- `IMessageConsumer<T>` → `RabbitMqConsumer<T>`

### Filas principais (`MessageQueues`)

| Fila | Direção | Propósito |
|------|---------|-----------|
| `OcrQueue` / `OcrQueueAiHubResponse` | Out/In | OCR |
| `EmbeddingQueue` / `EmbeddingQueueAiHubResponse` | Out/In | Embeddings |
| `ChatCompletionQueue` / `...Response` | Out/In | LLM Prompt |
| `AnswerQueue` / `...Response` | Out/In | Quiz |
| `AutomationQueueConsumer` / `AutomationQueueResponse` | Out/In | N8N |
| `ApiRequestQueue` / `ApiRequestQueueResponse` | Out/In | API externa |
| `OpenAiResponseQueue` / `...Response` | Out/In | OpenAI Responses API |
| `ExternalFileUploadQueue` | In | Upload externo |
| `UsageAccountingQueue` | In | Contabilização de uso |
| `MarketplaceSubscriptionQueue` | In | Assinaturas marketplace |
| `DeleteQueueConsumer` / `DeleteQueuePublisher` | In/Out | Exclusão de arquivos |
| `OcrAnonymizationQueueConsumer` | In | OCR pós-anonimização |

### Consumers (`Application/Messaging/`)

Todos herdam `BaseConsumer` (BackgroundService):

| Consumer | Processa |
|----------|----------|
| `OcrConsumer` | Resultado OCR |
| `DocumentEmbeddingsConsumer` | Resultado embeddings |
| `PromptConsumer` | Resposta LLM |
| `QuizConsumer` | Resposta quiz |
| `N8NConsumer` | Resposta N8N |
| `ApiOutputConsumer` | Resposta API |
| `ExternalFileUploadConsumer` | Upload externo |
| `SubscriptionConsumer` | Eventos marketplace |
| `SubscriptionEndPeriodConsumer` | Fim de período |
| `UsageAccountingConsumer` | Métricas |

### Dead Letter Consumers

`Messaging/DeadLetter/` — reprocessamento/fallback para filas com falha:
`OcrDeadLetterConsumer`, `EmbeddingsDeadLetterConsumer`, `PromptDeadLetterConsumer`, `ApiDeadLetterConsumer`, `QuizDeadLetterConsumer`, `N8NDeadLetterConsumer`.

### Padrão de consumer

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await _consumer.ConsumerAsync(_queues.MinhaFilaResponse, async message =>
    {
        using var scope = _scopeFactory.CreateScope();
        var connectionString = await GetConnectionStringAsync(scope, message.Tenant);
        // seta TenantConnection no HttpContext
        var service = scope.ServiceProvider.GetRequiredService<IMeuServices>();
        await service.ProcessMessage(message);
        await _automationServices.ContinueExecution(automationDto);
    });
}
```

---

## 11. Camada Repository

Localização: `back-end/WoopiAiHub.Repository/`

### Componentes

| Pasta | Conteúdo |
|-------|----------|
| `Context/` | `ApplicationDbContext` |
| `Mappings/` | Fluent API por entidade (`*Map.cs`) |
| `Migrations/` | Migrations EF Core |
| `Middleware/` | `MultiTenant` |
| `Cache/` | Redis (tenant, user access) |
| `Audit/` | Repositórios de auditoria |
| `Util/` | `UnitOfWork`, extensions, projections |

### Padrão de repository

```csharp
public class MeuRepository : IMeuRepository
{
    private readonly ApplicationDbContext _context;

    public MeuRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MeuEntity?> FindByIdAsync(int id)
    {
        return await _context.Set<MeuEntity>().FindAsync(id);
    }
}
```

### Unit of Work

```csharp
public interface IUnitOfWork
{
    void BeginTransaction();
    void Commit();
    void Rollback();
    Task<int> SaveChangesAsync();
}
```

### Auditoria automática

`ApplicationDbContext.SaveChanges(Async)` intercepta alterações e gera `AuditLog` via `AuditExtensions` — usuário identificado por JWT ou header `X-Email`.

### Query extensions

- `QueryableExtensions` — filtros/ordenação dinâmica (`System.Linq.Dynamic.Core`)
- Projections em `Util/Records/` para queries otimizadas

---

## 12. Camada Infrastructure

Localização: `back-end/WoopiAiHub.Infrastructure/`

| Componente | Responsabilidade |
|------------|------------------|
| `Messaging/Managers/RabbitMqManager` | Conexão e lifecycle RabbitMQ |
| `Messaging/Publishers/RabbitMqPublisher` | Publicação |
| `Messaging/Consumers/RabbitMqConsumer` | Consumo genérico |
| `Messaging/Configuration/MessageQueues` | Nomes das filas |
| `Multitenancy/TenantContextService` | Contexto de tenant fora de HTTP |
| `MessageBrokerInitializer` | HostedService — inicializa filas |

---

## 13. Integrações externas (Refit)

Registradas em `Application/DependencyInjection/ExternalApi.cs`:

| Interface Refit | Base URL (config) | Propósito |
|-----------------|-------------------|-----------|
| `IEmbeddingsApi` | IndexerApiBaseAddress | Indexação/vetores |
| `IFileRepositoryApi` | FileRepositoryApiBaseAddress | Upload/download arquivos |
| `IFunctionFileRetriever` | FunctionGetFileBaseAddress | Azure Function get file |
| `IGraphApi` | GraphApiBaseAddress | Microsoft Graph |
| `IMarketPlaceApi` | MarketPlaceBaseAddress | Marketplace/assinaturas |
| `IChatCompletionApi` | AiGatewayApiBaseAddress | AI Gateway (LLM) |
| `IAnonymizationApi` | AnonymizationApiBaseAddress | Anonimização |
| `IAzureAiSearch` | IntegrationApiBaseAddress | Integração Azure AI Search |

Interfaces em `WoopiAiHub.Domain/Interfaces/Refit/`.

Erros Refit são capturados pelo `GlobalExceptionHandler` como `400 Bad Request`.

---

## 14. SignalR e notificações em tempo real

| Item | Valor |
|------|-------|
| Hub | `NotificationHub` |
| Rota | `/hubs/notifications` (`HubRoutePaths.NotificationsHub`) |
| Mapeamento | `ConnectionMappingService` — conexões por email |
| Notificador | `IHubNotifier` / `HubNotifier` (Application) |

Eventos notificados ao frontend:
- Progresso de card/automação (`CardProgessAsync`)
- Anonimização concluída (`AnonymizationReady`)
- Uploads e processamentos

Frontend conecta via `@microsoft/signalr` com JWT.

---

## 15. Tratamento de erros

### `AppException` (negócio)

```csharp
throw new AppException(
    ErrorCode.NotFound,
    "Entity not found",
    DocumentLabel.NotFound  // chave i18n para frontend
);
```

### `ErrorCode` enum

| Código | Valor | Uso |
|--------|-------|-----|
| `DefaultError` | 0 | Genérico |
| `Duplicated` | 1 | Registro duplicado |
| `RequiredField` | 2 | Campo obrigatório |
| `NotFound` | 3 | Não encontrado |
| `Conflict` | 4 | Conflito |
| `InvalidValue` | 5 | Valor inválido |
| `UploadFailed` | 6 | Falha upload |
| `NoCreditsAvailable` | 7 | Sem créditos |
| `RefitApiError` | 8 | Erro API externa |
| `KeyVaultError` | 9 | Key Vault |
| `ExistingStepToolOutput` | 10 | Output já existe |
| `BusinessWarningOutput` | 11 | Aviso (frontend trata como warning) |

### `GlobalExceptionHandler`

Retorna `AppProblemDetails` JSON:

```json
{
  "title": "An error occurred",
  "status": 400,
  "detail": "...",
  "errorCode": 3,
  "labelError": "document.notFound"
}
```

Mapeamento:
- `AppException` → 400 + errorCode + labelError
- `KeyNotFoundException` / `FileNotFoundException` → 404
- `UnauthorizedAccessException` → 401
- `ArgumentException` / `InvalidOperationException` → 400
- Default → 500

### Labels de erro

Organizados em `Domain/Utils/ErrorLabels/` — chaves consumidas pelo frontend i18n.

---

## 16. Validação

### FluentValidation

Registro: `Domain/DependencyInjection/Extension.cs` → `AddValidation()`.

Validators atuais:
- `DocumentHistoryValidator`
- `DocumentNormalizedValidator`
- `DocumentDtoValidator`

### Padrão para novos validators

```csharp
public class MeuDtoValidator : AbstractValidator<MeuCreateDto>
{
    public MeuDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
```

Registrar em `AddValidation()` e invocar no service antes de persistir.

---

## 17. Configuração (appsettings)

Arquivo principal: `back-end/WoopiAiHub.Api/appsettings.json`  
Override: `appsettings.Development.json` + variáveis de ambiente.

### Chaves obrigatórias

| Chave | Descrição |
|-------|-----------|
| `CORS` | Origem frontend (**API não inicia sem ela**) |
| `ConnectionStrings:TemplateConnection` | SQL Server template |
| `ConnectionStrings:Redis` | Redis |
| `JWT:Key/Issuer/Audience` | Autenticação |
| `Messaging:BrokerType` | `RabbitMQ` |
| `Messaging:Brokers:RabbitMQ` | Credenciais RabbitMQ |
| `Messaging:Queues` | Nomes das filas |
| `RefitExternalSettings:*` | URLs APIs externas |
| `EncryptionSettings:Key` | AES-GCM para parâmetros |

### Chaves opcionais relevantes

| Chave | Descrição |
|-------|-----------|
| `OCRSettings` | Azure Document Intelligence |
| `UseOcrGoogle` | Alternar OCR Google |
| `ChatCompletionSettings` | Config LLM |
| `OpenAiSettings` | OpenAI |
| `McpSettings` | MCP tools |
| `DatabaseSettings:DefaultPlan` | Plano padrão tenant |

**Nunca versionar segredos reais** — usar User Secrets ou variáveis de ambiente.

---

## 18. Banco de dados e migrations

### Abordagem

- **Code First** — entidades C# → migrations EF Core
- Contexto: `ApplicationDbContext`
- Migrations: `WoopiAiHub.Repository/Migrations/`

### Comandos

```bash
cd back-end/WoopiAiHub.Api
dotnet ef database update --project ../WoopiAiHub.Repository

dotnet ef migrations add NomeDaMigration \
  --project ../WoopiAiHub.Repository \
  --context ApplicationDbContext
```

### Mappings

Cada entidade tem classe `*Map.cs` em `Repository/Mappings/` configurando Fluent API (tabelas, relações, índices).

### Runtime migration

`InitApplicationDb.RunApplicationMigration(context)` — usado em fluxos de provisionamento de tenant.

---

## 19. Projetos satélite

### File Repository (`external-api/`)

API separada para armazenamento de arquivos (Azure Blob).

| Projeto | Função |
|---------|--------|
| `FileRepository.Api` | REST upload/download |
| `FileRepository.Application` | `FileService` |
| `FileRepository.Domain` | DTOs, interfaces |
| `FileRepository.Functions` | `FileRetrieverAsync` — Azure Function |

Integração Hub → Refit `IFileRepositoryApi`.

### Azure Functions (`WoopiAiHub.Functions/`)

| Function | Propósito |
|----------|-----------|
| `ManageConsumptionsFunction` | Gestão de consumo |
| `ResetMonthMetricsFunction` | Reset métricas mensais |

Usa `AddRepository` + `AddExternalApi` — processamento agendado fora da API.

---

## 20. Catálogo de controllers

Base URL dev: `https://localhost:7045`

| Controller | Rota | Domínio |
|------------|------|---------|
| `AccountController` | `/api/Account` | Autenticação |
| `TenantController` | `/api/Tenant` | Tenants |
| `DocumentController` | `/api/Document` | Documentos, upload |
| `DocumentHistoryController` | `/api/DocumentHistory` | Histórico |
| `DocumentMetadataController` | `/api/DocumentMetadata` | Metadados |
| `DocumentQuestionnarireController` | `/api/DocumentQuestionnarire` | Questionários doc |
| `DocumentAnalysisRejectionController` | `/api/DocumentAnalysisRejection` | Reprovações |
| `AnonymizationController` | `/api/Anonymization` | Anonimização |
| `WorkflowController` | `/api/Workflow` | Esteiras |
| `CardController` | `/api/Card` | Cards kanban |
| `AutomationController` | `/api/Automation` | Automação |
| `PromptController` | `/api/Prompt` | Agentes |
| `PlayGroundPromptsController` | `/api/PlayGroundPrompts` | Playground |
| `QuestionnaireController` | `/api/Questionnaire` | Questionários |
| `QuestionController` | `/api/Question` | Perguntas |
| `ToolController` | `/api/Tool` | Ferramentas |
| `ToolTypeController` | `/api/ToolType` | Tipos |
| `ToolDataController` | `/api/ToolData` | Dados tools |
| `ApiTemplateController` | `/api/ApiTemplate` | Templates API |
| `ApiTemplateRequestCheckController` | `/api/ApiTemplateRequestCheck` | Validação template |
| `UserController` | `/api/User` | Usuários |
| `TeamController` | `/api/Team` | Times |
| `ProfileController` | `/api/Profile` | Perfis |
| `PermissionController` | `/api/Permission` | Permissões |
| `TypeDocController` | `/api/TypeDoc` | Tipos documento |
| `StatusController` | `/api/Status` | Status |
| `DashboardController` | `/api/Dashboard` | Consumo |
| `UsageMonthController` | `/api/UsageMonth` | Métricas mensais |
| `AuditorController` | `/api/Auditor` | Auditoria |
| Integration/* | `/api/integration/*` | Integrações externas |

Swagger: `https://localhost:7045/swagger` (Development)

Health: `GET /healthz`

---

## 21. Guia: adicionar um novo serviço/módulo

Checklist para agregar funcionalidade ao backend seguindo os padrões existentes.

### 1. Domain

- [ ] Criar entidade em `Domain/Models/` (herdar `BaseEntity` se persistida)
- [ ] Criar DTOs Request/Response em `Domain/DTOs/`
- [ ] Criar interface `IMeuRepository` em `Domain/Interfaces/Repository/`
- [ ] Criar interface `IMeuServices` em `Domain/Interfaces/Services/`
- [ ] Adicionar labels de erro em `Domain/Utils/ErrorLabels/` (se necessário)
- [ ] Criar validator FluentValidation (se necessário)

### 2. Repository

- [ ] Criar `MeuMap.cs` em `Mappings/`
- [ ] Adicionar `DbSet<MeuEntity>` em `ApplicationDbContext`
- [ ] Registrar mapping em `OnModelCreating`
- [ ] Implementar `MeuRepository`
- [ ] Registrar em `Repository/DependencyInjection/Extension.cs`
- [ ] Gerar migration: `dotnet ef migrations add AddMeuEntity ...`

### 3. Application

- [ ] Implementar `MeuServices` com regra de negócio
- [ ] Usar `Find*` nos métodos de leitura
- [ ] Lançar `AppException` com `ErrorCode` + `LabelError`
- [ ] Adicionar `/// <summary>` em métodos públicos
- [ ] Registrar em `Application/DependencyInjection/Extension.cs`

### 4. API

- [ ] Criar `MeuController` com `[Authorize]`
- [ ] Receber `[FromHeader] HeadersDto` quando precisar de tenant/email
- [ ] Documentar com Swagger annotations
- [ ] Retornar DTOs (nunca entidades EF diretamente)

### 5. Testes

- [ ] Criar `MeuServicesTests` + `MeuFixture` (ver seção 22)
- [ ] Cobrir caminho feliz + throws de `AppException`

### 6. Frontend (se aplicável)

- [ ] Service JS em `front-end/vueapp/src/services/`
- [ ] Chaves i18n em pt/en/es
- [ ] Seguir `PRODUCT_DESIGN.md`

### Se o módulo envolve processamento assíncrono

- [ ] Criar `IToolHandler` **ou** consumer dedicado
- [ ] Adicionar filas em `MessageQueues` + appsettings
- [ ] Registrar `AddHostedService<MeuConsumer>()`
- [ ] Implementar `ContinueExecution` no final do processamento

### Diagrama de decisão: sync vs async

```
Precisa de processamento longo ou serviço externo?
  ├── Não → Service + Repository (sync HTTP)
  └── Sim → ToolHandler ou Consumer + RabbitMQ
              └── Worker externo ou consumer interno processa
                  └── ContinueExecution → próximo passo
```

---

## 22. Testes de unidade

Referência canônica: `tests/WoopiAiHub.UnitTests/Services/CardServicesTests.cs` + `Fixture/CardFixture.cs`.

### Stack

- **xUnit** — `[Fact]` + `[Trait]`
- **Moq + Moq.AutoMock** — `new AutoMocker()` + `_mocker.CreateInstance<T>()`
- **Fixture estática** — construir DTOs/entidades de teste
- **AAA** — comentários `// Arrange`, `// Act`, `// Assert`
- **Naming** — `Metodo_Cenario_ResultadoEsperado`

### Cobertura obrigatória

Todo método **público novo em Services** exige teste cobrindo:
- Caminho feliz
- Cada `throw new AppException(...)` — validar `ErrorCode` e `LabelError`
- `.Verify(..., Times.Once)` em interações relevantes

### Esqueleto

```csharp
[Collection(nameof(MeuCollection))]
public class MeuServicesTests
{
    private readonly AutoMocker _mocker;
    private readonly Mock<IMeuRepository> _repositoryMock;
    private readonly MeuServices _services;

    public MeuServicesTests()
    {
        _mocker = new AutoMocker();
        _repositoryMock = _mocker.GetMock<IMeuRepository>();
        _services = _mocker.CreateInstance<MeuServices>();
    }

    [Fact(DisplayName = "FindById returns dto when entity exists")]
    [Trait("FindById", "Success")]
    public async Task FindById_EntityExists_ReturnsDto()
    {
        // Arrange
        var entity = MeuFixture.FindValidEntity();
        _repositoryMock.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(entity);

        // Act
        var result = await _services.FindByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        _repositoryMock.Verify(r => r.FindByIdAsync(1), Times.Once);
    }
}
```

Executar: `dotnet test` na raiz ou `dotnet test tests/WoopiAiHub.UnitTests/`.

---

## 23. Referências de código

| Recurso | Caminho |
|---------|---------|
| Bootstrap API | `back-end/WoopiAiHub.Api/Program.cs` |
| DI Application | `back-end/WoopiAiHub.Application/DependencyInjection/Extension.cs` |
| DI Repository | `back-end/WoopiAiHub.Repository/DependencyInjection/Extension.cs` |
| DI Infrastructure | `back-end/WoopiAiHub.Infrastructure/DependencyInjection/Extension.cs` |
| DI External APIs | `back-end/WoopiAiHub.Application/DependencyInjection/ExternalApi.cs` |
| DbContext | `back-end/WoopiAiHub.Repository/Context/ApplicationDbContext.cs` |
| Multitenancy | `back-end/WoopiAiHub.Repository/Middleware/MultiTenant.cs` |
| Exception handler | `back-end/WoopiAiHub.Api/Exceptions/GlobalExceptionHandler.cs` |
| Automação | `back-end/WoopiAiHub.Application/Services/Automation/AutomationServices.cs` |
| Tool factory | `back-end/WoopiAiHub.Application/ToolsHandler/ToolFactoryHandler.cs` |
| Filas | `back-end/WoopiAiHub.Infrastructure/Messaging/Configuration/MessageQueues.cs` |
| Headers | `back-end/WoopiAiHub.Domain/DTOs/Request/HeadersDto.cs` |
| Convenções IA | `AGENTS.md` |
| README técnico | `README.md` |
| Design frontend | `docs/PRODUCT_DESIGN.md` |
| Templates API (detalhe) | `docs/API_TEMPLATES.md` |

---

## Changelog

| Data | Versão | Notas |
|------|--------|-------|
| 2026-06-22 | 1.0 | Documento inicial — arquitetura backend completa |

---

> **Manutenção:** Ao adicionar controllers, entidades, filas ou integrações, atualize este documento e o catálogo de controllers (seção 20).
