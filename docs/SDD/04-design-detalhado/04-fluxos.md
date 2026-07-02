# 04 — Fluxos

> Parte de [`../README.md`](../README.md) · Design detalhado

---

## Fluxo 1 — Login e sessão

```
1. POST /api/Account/login (ou login-sso)
2. Se múltiplos tenants → modal seleção
3. Resposta: JWT + tenant + permissões
4. Frontend persiste em Vuex + localStorage
5. GET /api/Tenant/InitializeTenant/{tenant}
6. Redirect /home
7. SignalR conecta com access_token
```

Permissões decidem menu (`SidebarComponent`).

---

## Fluxo 2 — Upload de documento

```
1. Usuário seleciona arquivo(s) na esteira
2. Frontend envia chunks → POST /api/Document/UploadByChunks
3. Backend → File Repository (Refit)
4. Cria Document + Cards nas esteiras selecionadas
5. AutomationServices.PrepareExecutionAsync
6. Publica filas (OCR, Embeddings…) conforme StepTools
7. Toasts de progresso (GlobalEventService + Vuex)
8. SignalR notifica conclusão de automação
```

---

## Fluxo 3 — Automação de ferramenta (genérico)

```
1. Trigger: upload, avanço de etapa ou consumer anterior
2. AutomationServices identifica próximo StepToolExecution
3. ToolFactoryHandler → IToolHandler.BuildPayload
4. Descriptografa StepToolParameter
5. Substitui placeholders ({{ocr}}, {{prompt}}, {{embeddings}})
6. IMessagePublisher → fila específica
7. [Worker externo processa]
8. Consumer recebe resposta
9. Salva StepToolOutput + DocumentHistory
10. Registra UsageDaily
11. HubNotifier → progresso %
12. AutomationServices.ContinueExecution → próxima tool ou fim
```

Handlers: OCR, Embeddings, Prompt, Quiz, N8N, API — ver BACKEND_ARCHITECTURE §9.

---

## Fluxo 4 — Análise de documento (operador)

```
1. Usuário abre card → /analyzer/:documentId/:cardId
2. Visualiza documento + painel IA
3. Aplica questionário OU pergunta livre
4. Backend processa via Prompt/Quiz handlers
5. Operador confirma output ou edita
6. Avançar etapa → CardServices (valida tools pendentes)
   OU Reprovar → modal justificativa + DocumentAnalysisRejection
7. AuditCard registra ações
```

UI: PRODUCT_DESIGN + páginas `analyze/`

---

## Fluxo 5 — CRUD gestão (padrão reutilizável)

```
Listagem:
  Page → Service.FindAllPaged → TableComponent
  Filtros → SearchComponent
  Ações linha → edit / delete (ConfirmModal)

Criação/Edição:
  Form em .main-div → VeeValidate
  Salvar → Service.Create/Update → toast success → redirect listagem
  Erro → toast com labelError i18n

Exclusão:
  ConfirmModal → Service.Delete → toast
```

Wireframes: PRODUCT_DESIGN §14

---

## Fluxo 6 — Configurar esteira (gestor)

```
1. Gestão de Esteiras → list workflows
2. New/Edit workflow → steps ordenados
3. Por step: profile, status, step tools (drag/order)
4. Por step tool: seleciona Tool, configura parâmetros (criptografados)
5. Dependências entre tools (outputs → placeholders)
6. Associa teams ao workflow
7. Save → WorkflowServices (+ validações ValidateWorkflow/ValidateStep)
```

Editor visual de fluxo: `@vue-flow/core` em páginas `flows/`

---

## Fluxo 7 — Nova funcionalidade (agente SDD)

```
Spec (template 08-anexos)
  → Domain + Migration (se persistir)
  → Repository + Service + Controller
  → Testes unitários
  → Frontend: service + page + i18n + router + menu
  → Atualizar SDD seções 02 e 04
  → Validar multitenancy + permissões + tema dark
```

---

## Placeholders API Template

| Placeholder | Origem |
|-------------|--------|
| `{{ocr}}` | Output tool OCR |
| `{{embeddings}}` | Output Embeddings |
| `{{prompt}}` | Output Prompt |

Detalhe: API_TEMPLATES.md

---

## Documentação relacionada

- Diagramas → [`../03-arquitetura/02-diagramas.md`](../03-arquitetura/02-diagramas.md)
- RF → [`../02-requisitos/01-requisitos-funcionais.md`](../02-requisitos/01-requisitos-funcionais.md)
- Interação UI → [`../../PRODUCT_DESIGN.md`](../../PRODUCT_DESIGN.md) §12
