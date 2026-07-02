# 02 — Design de Módulos

> Parte de [`../README.md`](../README.md) · Design detalhado

---

## Backend — mapa de módulos

| Módulo | Services | Controller | Observação |
|--------|----------|------------|------------|
| Account | AccountServices, JwtTokenServices, RefreshTokenServices | AccountController | Sem JWT no login |
| Tenant | TenantServices | TenantController | InitializeTenant |
| Document | DocumentServices, Upload, Deletion, Pipeline, History… | DocumentController | Upload chunked |
| Workflow | WorkflowServices | WorkflowController | Steps + tools |
| Card | CardServices | CardController | Kanban operacional |
| Automation | AutomationServices, ExecutionServices | AutomationController | Motor de filas |
| Prompt | PromptServices, PlaygroundServices | PromptController | Agentes |
| Questionnaire | QuestionnaireServices, QuestionServices | QuestionnaireController | Quizzes |
| Tool | ToolServices, ToolTypeServices, ToolDataServices | ToolController | Conectores |
| ApiTemplate | ApiTemplateServices | ApiTemplateController | Ver API_TEMPLATES.md |
| User/Team/Profile | UserServices, TeamServices, ProfileServices | Respectivos | Gestão |
| Permission | PermissionServices | PermissionController | — |
| Dashboard | Usage*Services | DashboardController, UsageMonthController | Consumo |
| Auditor | AuditorServices, AuditCardService | AuditorController | — |
| Anonymization | AnonymizationServices | AnonymizationController | — |

Registro DI: `Application/DependencyInjection/Extension.cs`

---

## Frontend — mapa de módulos

| Módulo UI | Pages | Components | Service JS |
|-----------|-------|------------|------------|
| Home | `pages/home.vue` | — | DashboardServices |
| Documentos/Esteira | `documentsHub/` | `documentsHub/`, `analyze/` | DocumentsServices, CardsServices |
| Gestão WF | `workflow/` | `workflow/` | WorkflowService |
| Agentes | `prompts/` | `prompts/` | PromptsService |
| Questionários | `managementQuizzes/` | `quizzes/`, `questions/` | QuizzesService |
| Conectores | `tools.vue` | `tools/` | ToolsServices |
| Templates API | `templates/` | `templates/` | TemplateService |
| Gestão | `management/` | `management/` | UserService, TeamsService… |
| Dashboard | `dashboard.vue` | `graphs/` | DashboardServices |
| Auditoria | `auditor.vue` | — | AuditorsService |
| Auth | `login.vue` | `authentication/` | AuthService |

Rotas: `src/router/index.js` — meta `module` + `action` para permissões.

---

## Tool Handlers (Application)

| Handler | Type constant | Consumer resposta |
|---------|---------------|-------------------|
| OcrHandler | OCR | OcrConsumer |
| EmbeddingsHandler | Embeddings | DocumentEmbeddingsConsumer |
| PromptHandler | Prompt | PromptConsumer |
| QuizHandler | Quiz | QuizConsumer |
| N8NHandler | N8N | N8NConsumer |
| ApiHandler | API | ApiOutputConsumer |

Resolução: `ToolFactoryHandler.GetHandler(type)`

---

## Componentes globais frontend (reutilizar)

Local: `src/components/global/`

**Obrigatório reutilizar:** TableComponent, ModalComponent, ConfirmModal, PaginationComponent, SearchComponent, TabsComponent, BadgeComponent, LucideIcon, LoadingComponent, NotificationComponent.

Catálogo completo: [`../../PRODUCT_DESIGN.md`](../../PRODUCT_DESIGN.md) §8

---

## Padrão de novo módulo backend

```
Domain/Models/{Entity}.cs
Domain/Interfaces/Repository/I{Entity}Repository.cs
Domain/Interfaces/Services/I{Entity}Services.cs
Domain/DTOs/Request/ + Response/
Repository/{Entity}Repository.cs + Mappings/
Application/Services/{Entity}Services.cs
Api/Controllers/{Entity}Controller.cs
tests/.../Services/{Entity}ServicesTests.cs + Fixture/
```

---

## Padrão de novo módulo frontend

```
src/services/{modulo}/{Modulo}Service.js   → api.get/post com headers
src/pages/{modulo}/index.vue               → listagem
src/pages/{modulo}/new*.vue | edit*.vue    → formulários
src/components/{modulo}/                   → tabela, filtros, forms
src/locales/translations/pt.js (+ en, es)  → chaves {modulo}.*
src/router/index.js                        → rota + meta module/action
SidebarComponent.vue                       → item menu + permissão
```

---

## Documentação relacionada

- Modelo de dados → [`01-modelo-dados.md`](./01-modelo-dados.md)
- Interfaces → [`03-interfaces.md`](./03-interfaces.md)
- Design UX → [`../../PRODUCT_DESIGN.md`](../../PRODUCT_DESIGN.md)
