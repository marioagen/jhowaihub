# 02 — Diagramas

> Parte de [`../README.md`](../README.md) · Arquitetura do sistema

---

## C4 — Contexto (Nível 1)

```mermaid
C4Context
    title WOOPI AI Hub — Contexto

    Person(user, "Usuário", "Operador, Gestor, Admin")
    System(hub, "WOOPI AI Hub", "Plataforma documental + IA")
    System_Ext(fileRepo, "File Repository", "Armazenamento de arquivos")
    System_Ext(aiGateway, "AI Gateway", "LLM / Chat completion")
    System_Ext(indexer, "Indexer", "Embeddings / busca")
    System_Ext(rabbit, "RabbitMQ", "Filas assíncronas")
    System_Ext(marketplace, "Marketplace", "Assinaturas / tenants")

    Rel(user, hub, "Usa", "HTTPS + SignalR")
    Rel(hub, fileRepo, "Upload/download", "HTTPS")
    Rel(hub, aiGateway, "Prompts", "HTTPS")
    Rel(hub, indexer, "Embeddings", "HTTPS")
    Rel(hub, rabbit, "Publica/consome", "AMQP")
    Rel(hub, marketplace, "Tenant/plano", "HTTPS")
```

---

## C4 — Contêineres (Nível 2)

```mermaid
flowchart TB
    subgraph Client
        FE[Vue 3 SPA<br/>front-end/vueapp]
    end

    subgraph Backend
        API[WoopiAiHub.Api<br/>REST + SignalR]
        APP[WoopiAiHub.Application<br/>Services + Consumers]
        REPO[WoopiAiHub.Repository<br/>EF Core + SQL]
        INF[WoopiAiHub.Infrastructure<br/>RabbitMQ]
    end

    subgraph Data
        SQL[(SQL Server<br/>por tenant)]
        REDIS[(Redis)]
        RMQ[RabbitMQ]
    end

    subgraph External
        FR[File Repository API]
        AGW[AI Gateway]
    end

    FE -->|JWT REST| API
    FE -->|WebSocket| API
    API --> APP
    APP --> REPO
    APP --> INF
    REPO --> SQL
    REPO --> REDIS
    INF --> RMQ
    APP --> FR
    APP --> AGW
```

---

## Sequência — Upload e entrada na esteira

```mermaid
sequenceDiagram
    participant U as Usuário
    participant FE as Frontend
    participant API as DocumentController
    participant FR as File Repository
    participant DB as SQL Tenant
    participant AUTO as AutomationServices
    participant Q as RabbitMQ

    U->>FE: Seleciona arquivo(s)
    FE->>API: POST UploadByChunks (X-Tenant, X-Email)
    API->>FR: Envia chunks
    API->>DB: Cria Document + Cards
    API->>AUTO: PrepareExecution
    AUTO->>Q: Publica OCR/Embeddings/...
    Q-->>API: Consumer processa resposta
    API->>FE: SignalR progresso
```

---

## Sequência — Automação de ferramentas

```mermaid
sequenceDiagram
    participant AUTO as AutomationServices
    participant TF as ToolFactoryHandler
    participant H as IToolHandler
    participant Q as RabbitMQ
    participant C as Consumer
    participant HUB as SignalR

    AUTO->>TF: GetHandler(toolType)
    TF->>H: BuildPayload
    H->>Q: Publish(queue, message)
    Q->>C: Response message
    C->>AUTO: ContinueExecution
    AUTO->>HUB: CardProgessAsync
```

---

## Frontend — Layout autenticado

```
┌─────────────┬──────────────────────────────────────┐
│  Sidebar    │  Navbar (tenant, 🔔, tema, idioma)   │
│  240/60px   ├──────────────────────────────────────┤
│             │  Conteúdo (router-view)               │
│  Menu       │  padding 70px @ ≥1025px                │
└─────────────┴──────────────────────────────────────┘
```

Ver PRODUCT_DESIGN §2.

---

## Modelo esteira (simplificado)

```mermaid
erDiagram
    Workflow ||--o{ Step : contains
    Step ||--o{ StepTool : has
    Step ||--o{ Card : has
    Card }o--|| Document : references
    StepTool ||--o{ StepToolExecution : runs
    StepTool ||--o{ StepToolOutput : produces
    StepTool }o--|| Tool : uses
    Tool }o--|| ToolType : typed
```

Detalhe entidades → [`../04-design-detalhado/01-modelo-dados.md`](../04-design-detalhado/01-modelo-dados.md)

---

## Documentação relacionada

- Visão arquitetural → [`01-visao-arquitetural.md`](./01-visao-arquitetural.md)
- Fluxos de negócio → [`../04-design-detalhado/04-fluxos.md`](../04-design-detalhado/04-fluxos.md)
