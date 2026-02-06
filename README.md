# Woopi AI Hub

README técnico do projeto — guia oficial para novos desenvolvedores. Documenta stack, arquitetura e forma de execução da aplicação (backend, frontend, banco de dados, dependências, configuração e execução local).

---

## Índice

- [Visão geral](#visão-geral)
- [Backend (.NET)](#backend-net)
- [Banco de dados](#banco-de-dados)
- [Frontend (Vue.js)](#frontend-vuejs)
- [Como rodar o projeto localmente](#como-rodar-o-projeto-localmente)
- [Scripts e comandos úteis](#scripts-e-comandos-úteis)
- [Boas práticas e observações](#boas-práticas-e-observações)

---

## Visão geral

### Propósito do projeto

O **Woopi AI Hub** é uma aplicação que integra **backend** (.NET) e **frontend** (Vue.js) para gestão de documentos, workflows, ferramentas de IA (OCR, embeddings, prompts), questionários, perfis, times e tenants. O sistema é **multi-tenant** e utiliza filas (RabbitMQ) para processamento assíncrono (OCR, embeddings, respostas de IA, automações).

### Principais responsabilidades

- **Backend**: API REST (Web API .NET), autenticação JWT, multitenancy, Entity Framework Core, Redis (cache), RabbitMQ (mensageria), SignalR (notificações em tempo real), integração com APIs externas (Indexer, File Repository, AI Gateway, etc.).
- **Frontend**: Interface SPA em Vue.js para documentos, fluxos/workflows, gestão de usuários, times, perfis, questionários, prompts, templates e ferramentas.
- **Banco de dados**: SQL Server por tenant; migrations gerenciadas via EF Core (Code First).

---

## Backend (.NET)

### Versão e tipo de aplicação

- **.NET**: 8.0 (`net8.0` em todos os projetos do backend).
- **Tipo**: Web API ASP.NET Core (projeto de entrada: `WoopiAiHub.Api`).

### Estrutura do projeto (camadas e pastas)

| Pasta / Projeto | Responsabilidade |
|-----------------|------------------|
| `back-end/WoopiAiHub.Api/` | API REST, controllers, Swagger, JWT, CORS, SignalR, exception handler, health checks. |
| `back-end/WoopiAiHub.Application/` | Serviços de negócio, consumers RabbitMQ, handlers de ferramentas (OCR, Embeddings, Prompt, N8N), validações (FluentValidation), integração com APIs externas (Refit). |
| `back-end/WoopiAiHub.Domain/` | Entidades, enums, interfaces (repositórios, serviços, handlers, utils). |
| `back-end/WoopiAiHub.Infrastructure/` | Mensageria (RabbitMQ), multitenancy (TenantContextService), configurações de filas. |
| `back-end/WoopiAiHub.Repository/` | Entity Framework Core, `ApplicationDbContext`, repositórios, mappings (Fluent API), migrations, cache Redis, middleware multi-tenant, Unit of Work. |
| `back-end/WoopiAiHub.Functions/` | Azure Functions (projeto separado, também .NET 8). |

### Entity Framework Core

- **Versão**: 8.0.14.
- **Abordagem**: **Code First** (modelo em C#, migrations geram/atualizam o banco).
- **Contexto**: `ApplicationDbContext` em `back-end/WoopiAiHub.Repository/Context/ApplicationDbContext.cs`.
- **Migrations**: em `back-end/WoopiAiHub.Repository/Migrations/`. Aplicadas via CLI (`dotnet ef`) ou em runtime em cenários de tenant (via `InitApplicationDb.RunApplicationMigration`).

### Principais bibliotecas e frameworks (backend)

- **API**: Swashbuckle (Swagger) 8.x, JWT Bearer, Newtonsoft.Json, MassTransit.RabbitMQ, Azure AI Document Intelligence, Argon2.
- **Repository**: EF Core 8.0.14, EF Core SqlServer, EF Core Design/Tools, StackExchange Redis, AutoMapper, System.Linq.Dynamic.Core.
- **Application**: FluentValidation, Refit, AutoMapper, Azure (Document Intelligence, Form Recognizer, Storage Blobs), Google.Cloud.Vision (OCR), Serilog, PDFsharp, etc.
- **Infrastructure**: RabbitMQ.Client, Polly, Microsoft.Extensions.* (Configuration, Logging, Options).

### Configurações importantes (appsettings e variáveis de ambiente)

Arquivos principais:

- `back-end/WoopiAiHub.Api/appsettings.json`
- `back-end/WoopiAiHub.Api/appsettings.Development.json` (e variáveis de ambiente sobrescrevem)

Principais chaves:

| Chave | Descrição |
|-------|-----------|
| `ConnectionStrings:TemplateConnection` | Connection string do SQL Server (template multi-tenant: use `___NEWDB___` como placeholder do nome do banco do tenant). |
| `ConnectionStrings:Redis` | Endereço do Redis (ex.: `localhost:6379`). |
| `CORS` | Origem permitida para o frontend (ex.: `https://localhost:3000`). **Obrigatório** — a API falha na inicialização se não estiver definido. |
| `JWT:Key`, `JWT:Issuer`, `JWT:Audience` | Configuração do token JWT. |
| `RefitExternalSettings:*` | URLs das APIs externas (Indexer, FileRepository, Function, Graph, Marketplace, AiGateway). |
| `Messaging:BrokerType`, `Messaging:Brokers:RabbitMQ`, `Messaging:Queues` | RabbitMQ (host, credenciais, nomes das filas). |
| `OCRSettings`, `UseOcrGoogle`, `Azure:ClientId`, `EncryptionSettings:Key`, `KeyAccess`, `IndexerApiKey` | OCR, Azure e chaves de integração. |

Variáveis de ambiente têm precedência sobre o JSON (ConfigurationBuilder com `AddEnvironmentVariables()`).

### Padrões adotados

- **Injeção de dependência**: serviços registrados em `Program.cs` e em extensions (AddRepository, AddApplication, AddValidation, AddInfrastructure, AddExternalApi).
- **Repository**: um repositório por entidade/agregado, interfaces no Domain, implementações no Repository.
- **Unit of Work**: `IUnitOfWork` em `WoopiAiHub.Repository/Util/UnitOfWork.cs`.
- **Serviços de aplicação**: camada Application com serviços por domínio (Documents, Workflows, Users, Teams, etc.).
- **Validação**: FluentValidation registrado via `AddValidation()` (Domain).
- **Multitenancy**: header `X-Tenant`; connection string por tenant com `___NEWDB___`; middleware em `WoopiAiHub.Repository/Middleware/MultiTenant.cs`.
- **Mensageria**: RabbitMQ com consumers em background (OCR, Embeddings, Prompt, N8N, Subscription).
- **API externa**: Refit para chamadas HTTP tipadas.
- **AutoMapper**: mapeamento entre entidades/DTOs.
- **Exception handling**: `GlobalExceptionHandler` + `AddProblemDetails()`.

---

## Banco de dados

### Tipo de banco

- **SQL Server** (via `Microsoft.EntityFrameworkCore.SqlServer`).

### Connection string

- Configurada em `appsettings.json` (ou variável de ambiente) na chave **`ConnectionStrings:TemplateConnection`**.
- Exemplo (desenvolvimento):

  ```text
  Server=localhost;Database=WoopiAiHub;Trusted_Connection=True;TrustServerCertificate=True;
  ```

- Em cenário **multi-tenant**, o template usa o placeholder `___NEWDB___`, substituído pelo nome do banco do tenant (ex.: `Server=.;Database=TenantXYZ;...`).

### Criar/atualizar o banco via EF Core

**Opção 1 — CLI (recomendado para desenvolvimento):**

O projeto que contém o contexto é o **Repository**; o projeto de startup para o tooling é a **API** (possui `Microsoft.EntityFrameworkCore.Design`). Na raiz do repositório:

```bash
cd back-end/WoopiAiHub.Api
dotnet ef database update --project ../WoopiAiHub.Repository
```

Para adicionar uma nova migration (a partir do Repository):

```bash
cd back-end/WoopiAiHub.Api
dotnet ef migrations add NomeDaMigration --project ../WoopiAiHub.Repository --context ApplicationDbContext
```

**Opção 2 — Runtime:**  
Em alguns fluxos (tenant, uso interno), as migrations são aplicadas em código via `InitApplicationDb.RunApplicationMigration(context)` (que chama `context.Database.Migrate()`).

---

## Frontend (Vue.js)

### Versões e setup

- **Vue.js**: 3.x (ex.: `"vue": "^3.2.26"`).
- **Build / dev server**: **Vite** 7.x (não Vue CLI).
- **Node.js**: recomendado **18+** (CI usa Node 20 no format-check).
- **Gerenciador de pacotes**: **npm** (há `package-lock.json` no frontend).

### Estrutura de pastas (front-end/vueapp)

| Pasta | Conteúdo |
|-------|----------|
| `src/` | Código fonte da SPA. |
| `src/components/` | Componentes Vue (analyze, authentication, dashboard, documents, flow, global, layout, management, pages, prompts, questions, quizzes, templates, tools, types, workflow). |
| `src/pages/` | Páginas/views (dashboard, documents, flows, login, management, prompts, templates, workflow, etc.). |
| `src/router/` | Vue Router. |
| `src/store/` | Vuex (store global). |
| `src/services/` | Chamadas à API (axios), SignalR, serviços por domínio. |
| `src/locales/` | Vue I18n (traduções pt, en, es). |
| `src/layouts/` | Layouts (auth, default). |
| `src/constants/`, `src/helpers/`, `src/utils/`, `src/validators/`, `src/directives/`, `src/workers/` | Constantes, helpers, validação, workers. |
| `public/config/appsettings.js` | Configuração estática (nome da app, URLs) carregada no browser. |

### Principais bibliotecas (frontend)

- **Core**: Vue 3, Vue Router 4, Vuex 4, vuex-persistedstate.
- **HTTP / real-time**: Axios, @microsoft/signalr.
- **UI / forms**: VeeValidate, Yup, @vueform/multiselect, floating-vue, @popperjs/core, Lucide (lucide-vue-next).
- **Gráficos / fluxo**: ApexCharts, vue3-apexcharts, @vue-flow/core, @vue-flow/background.
- **Upload**: dropzone, vue3-simple-dropzone, @jaxtheprime/vue3-dropzone.
- **Outros**: vue-i18n, vue-gtag, date-fns, jwt-decode, js-cookie, qs, mitt.

### Configurações de ambiente (frontend)

- **Arquivo**: `front-end/vueapp/.env` (variáveis com prefixo `VUE_APP_`).
- Exemplos:
  - `VUE_APP_NAME` — Nome da aplicação.
  - `VUE_APP_BASE_URL_API` — URL da API (ex.: `https://localhost:7045`).
  - `VUE_APP_BASE_URL_API_AZURE`, `VUE_APP_KEY_API_AZURE`, `VUE_APP_CLIENT_ID_AZURE` — Integração Azure.
  - `VUE_APP_WAITING_TIME_MSG_UPLD` — Timeout de upload.
- O dev server Vite roda por padrão em **http://localhost:3000** (ou https se existirem `localhost-key.pem` e `localhost.pem` no projeto).

---

## Como rodar o projeto localmente

### 1. Pré-requisitos

- **.NET 8 SDK**
- **Node.js** 18+ (recomendado 20) e **npm**
- **SQL Server** (local ou instância acessível)
- **Redis** (ex.: `localhost:6379`)
- **RabbitMQ** (para consumers da API; host/credenciais em `appsettings`)

### 2. Clonar o repositório

```bash
git clone <url-do-repositorio>
cd <nome-do-repositorio>
```

### 3. Configurar variáveis de ambiente e appsettings

**Backend:**

- Copiar/editar `back-end/WoopiAiHub.Api/appsettings.json` e preencher:
  - `ConnectionStrings:TemplateConnection` (SQL Server).
  - `ConnectionStrings:Redis`.
  - `CORS` (ex.: `https://localhost:3000` ou `http://localhost:3000` conforme o frontend).
  - `JWT:Key`, `JWT:Issuer`, `JWT:Audience`.
  - Configurações de RabbitMQ em `Messaging:Brokers:RabbitMQ`.
  - Demais chaves conforme necessidade (APIs externas, OCR, etc.).
- Opcional: usar `appsettings.Development.json` ou variáveis de ambiente para sobrescrever.

**Frontend:**

- Editar `front-end/vueapp/.env` e definir `VUE_APP_BASE_URL_API` para a URL da API (ex.: `https://localhost:7045`).

### 4. Restaurar dependências

**Backend:**

```bash
dotnet restore
# ou, a partir da solução:
dotnet restore WoopiaiHub.sln
```

**Frontend:**

```bash
cd front-end/vueapp
npm install
cd ../..
```

### 5. Rodar migrations do banco

```bash
cd back-end/WoopiAiHub.Api
dotnet ef database update --project ../WoopiAiHub.Repository
cd ../..
```

Garanta que a connection string em `appsettings` (ou env) aponte para o servidor desejado antes de rodar.

### 6. Subir o backend

```bash
cd back-end/WoopiAiHub.Api
dotnet run
```

Ou pela solução (Visual Studio / Rider): definir **WoopiAiHub.Api** como projeto de início e executar.

- **URL da API**: conforme `launchSettings.json`: **https://localhost:7045** (HTTPS) e **http://localhost:5215** (HTTP).
- **Swagger**: https://localhost:7045/swagger (em Development).

### 7. Subir o frontend

Em outro terminal:

```bash
cd front-end/vueapp
npm run dev
```

- **URL do frontend**: **http://localhost:3000** (ou https se certificados locais estiverem configurados no Vite).

### 8. URLs de acesso

| Serviço | URL típica (desenvolvimento) |
|---------|------------------------------|
| API (Swagger) | https://localhost:7045/swagger |
| API (base) | https://localhost:7045 |
| Frontend | http://localhost:3000 |
| Health check | https://localhost:7045/healthz |
| SignalR (notificações) | https://localhost:7045/hubs/notifications |

O frontend deve ter `CORS` na API configurado com a origem do frontend (ex.: `http://localhost:3000` ou `https://localhost:3000`).

---

## Scripts e comandos úteis

### Raiz do repositório (package.json)

| Comando | Descrição |
|---------|-----------|
| `npm run format:changed` | Formata arquivos alterados (root + frontend). |
| `npm run format` | Formata apenas arquivos da raiz (JSON, YAML, MD, etc.). |
| `npm run format:frontend` | Formata apenas arquivos do frontend. |
| `npm run format:check` | Verifica formatação (Prettier) na raiz. |
| `npm run format:staged` | Formata apenas arquivos staged (útil para hooks). |
| `npm run lint:frontend` | Roda ESLint no frontend. |
| `npm run setup:hooks` | Configura git hooks (formatação no pre-commit). |

### Backend

| Comando | Descrição |
|---------|-----------|
| `dotnet restore` | Restaura pacotes NuGet. |
| `dotnet build back-end/WoopiAiHub.Api/WoopiAiHub.Api.csproj` | Build da API. |
| `dotnet run --project back-end/WoopiAiHub.Api` | Executa a API. |
| `dotnet test` | Executa testes (ex.: WoopiAiHub.UnitTests). |
| `dotnet ef database update --project back-end/WoopiAiHub.Repository` (a partir de `back-end/WoopiAiHub.Api`) | Aplica migrations. |
| `dotnet ef migrations add NomeDaMigration --project back-end/WoopiAiHub.Repository --context ApplicationDbContext` (a partir de `back-end/WoopiAiHub.Api`) | Cria nova migration. |

### Frontend (dentro de front-end/vueapp)

| Comando | Descrição |
|---------|-----------|
| `npm run dev` | Sobe o servidor de desenvolvimento (Vite). |
| `npm run build` | Build de produção. |
| `npm run lint` | ESLint. |
| `npm run format` | Prettier (escreve). |
| `npm run format:check` | Prettier (apenas verifica). |

### CI/CD (referência)

- **Build e testes**: workflow `build.yml` — restore, build da API, FileRepository.Api e testes unitários (.NET 6 e 8).
- **Formatação**: workflow `format-check.yml` — Node 20, `npm run format:changed` e verificação de diff.
- **Imagens Docker**: workflows `BuildImageBackEnd.yml`, `BuildImageFrontEnd.yml`, `BuildImageFileRepository.yml`, `BuildImageFunctions.yml`.

---

## Boas práticas e observações

### Regras e convenções

- **EditorConfig**: o repositório usa `.editorconfig` (indentação, line endings, regras C#, JSON, YAML, MD). C#: 4 espaços, `max_line_length = 120`, CRLF, charset UTF-8.
- **Formatação**: Prettier para frontend e arquivos da raiz; hook **pre-commit** formata arquivos staged (configuração em `.githooks` e scripts em `scripts/`). Evite `--no-verify` a não ser em casos excepcionais.
- **C#**: seguir convenções do EditorConfig; usar `dotnet format` quando aplicável para manter consistência.
- **Multitenancy**: a API espera o header **X-Tenant** em requisições que precisem de contexto de tenant; a connection string do tenant usa o placeholder `___NEWDB___` no nome do banco.

### Pontos de atenção para novos desenvolvedores

1. **CORS**: a API **exige** a chave `CORS` configurada (appsettings ou variável de ambiente); caso contrário a aplicação não inicia.
2. **Banco por tenant**: em ambiente multi-tenant, cada tenant pode ter seu próprio banco; as migrations podem ser aplicadas por tenant em runtime ou via scripts/ferramentas.
3. **RabbitMQ e Redis**: necessários para consumers e cache; sem eles, funcionalidades que dependem de filas ou cache podem falhar.
4. **Certificados HTTPS (frontend)**: o Vite pode usar HTTPS em dev se existirem `localhost-key.pem` e `localhost.pem` no diretório do vueapp; caso contrário usa HTTP (e o `CORS` da API deve permitir essa origem).
5. **appsettings.Development.json**: contém dados sensíveis (ex.: chaves Google); não versionar segredos reais; usar variáveis de ambiente ou User Secrets em desenvolvimento.
6. **Git hooks**: após clonar, executar `npm run setup:hooks` (ou o script em `scripts/`) para ativar a formatação automática no pre-commit (ver `.githooks/README.md`).

### Estrutura da solução (.sln)

- **back-end**: WoopiAiHub.Api, WoopiAiHub.Application, WoopiAiHub.Domain, WoopiAiHub.Infrastructure, WoopiAiHub.Repository, WoopiAiHub.Functions.
- **external-api**: FileRepository.Api, FileRepository.Application, FileRepository.Domain, FileRepository.Functions.
- **front-end**: vueapp (Vue/Vite).
- **tests**: WoopiAiHub.UnitTests.

---

Este README cobre backend (.NET 8), frontend (Vue 3 + Vite), banco de dados (SQL Server + EF Core), configuração e execução local, e deve ser suficiente para onboarding de novos desenvolvedores. Para dúvidas sobre um módulo específico (ex.: File Repository, Functions), consulte os projetos em `external-api/` e `back-end/WoopiAiHub.Functions/`.
