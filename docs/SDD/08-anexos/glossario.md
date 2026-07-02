# Glossário

> Parte de [`../README.md`](../README.md) · Anexos

Termos unificados entre produto, SDD, backend e frontend. **Use sempre estes termos** em specs e implementações.

---

## Produto (PT-BR na UI)

| Termo | Definição | Código/API |
|-------|-----------|------------|
| **WOOPI AI Hub** | Nome do produto | — |
| **Esteira de Processamento** | Workflow operacional — documentos fluem por etapas | `Workflow`, rota `/workflow` |
| **Gestão de Esteiras** | Configuração/admin de workflows | `WorkflowManagement`, `/workflow/management` |
| **Etapa** | Step ordenado dentro da esteira | `Step` |
| **Card** | Instância documento+etapa no kanban | `Card` |
| **Documento** | Arquivo processado na plataforma | `Document` |
| **Agente** | Prompt de IA configurável | `Prompt`, menu "Agentes" |
| **Questionário** | Conjunto de perguntas aplicáveis a documentos | `Questionnaire`, `Quiz` handler |
| **Conector** | Integração N8N ou similar | `Tool` tipo N8N |
| **Template de API** | Modelo reutilizável de HTTP | `ApiTemplate` |
| **Tenant** | Ambiente isolado do cliente | Header `X-Tenant`, claim JWT `tenant` |
| **Painel de Consumo** | Dashboard de métricas | `Dashboard` |
| **Auditoria** | Trilha de ações | `Auditor`, `AuditLog` |

---

## Técnico — Backend

| Termo | Definição |
|-------|-----------|
| **HandlersTypes** | Constantes: OCR, Embeddings, Prompt, N8N, API, Quiz |
| **StepTool** | Ferramenta configurada em uma etapa |
| **StepToolExecution** | Registro de execução assíncrona |
| **StepToolOutput** | Resultado JSON de uma execução |
| **AutomationServices** | Orquestrador de ferramentas |
| **ToolFactoryHandler** | Factory de IToolHandler por tipo |
| **AppException** | Exceção de negócio com ErrorCode + LabelError |
| **HeadersDto** | X-Email, X-Tenant, X-Language |
| **Find*** | Prefixo obrigatório para métodos de leitura |
| **labelError** | Chave i18n retornada em erros API |

---

## Técnico — Frontend

| Termo | Definição |
|-------|-----------|
| **defaultLayout** | Sidebar + Navbar + conteúdo |
| **authLayout** | Login/logout sem sidebar |
| **css-theme-light/dark** | Classes de tema no `<html>` |
| **TableComponent** | Tabela padrão com ordenação/paginação |
| **$notify** | Toast global (NotificationComponent) |
| **LucideIcon** | Wrapper de ícones Lucide |
| **main-div** | Container card de formulário |

---

## Métricas

| Termo | MetricNames |
|-------|-------------|
| Automação | Automation |
| Execução | Execution |
| Token LLM | Token |
| Página OCR | Page |

---

## Erros (ErrorCode)

| Código | Nome | Uso |
|--------|------|-----|
| 3 | NotFound | Recurso inexistente |
| 11 | BusinessWarningOutput | Aviso (toast warning no login) |

Lista completa: `Domain/Enum/ErrorCode.cs`

---

## Documentação relacionada

- PRODUCT_DESIGN § vocabulário
- BACKEND_ARCHITECTURE §4 modelo
- Exemplos → [`exemplos.md`](./exemplos.md)
