# 01 — Requisitos Funcionais

> Parte de [`../README.md`](../README.md) · Requisitos do sistema

Padrões existentes no produto. **Novas funcionalidades** devem adicionar RF numerados neste arquivo ou em spec dedicada (`08-anexos/template-nova-funcionalidade.md`).

---

## Módulos e capacidades

### Autenticação e tenant (RF-AUTH)

| ID | Requisito |
|----|-----------|
| RF-AUTH-01 | Login email/senha com seleção de tenant quando múltiplos |
| RF-AUTH-02 | Login SSO Microsoft (MSAL) |
| RF-AUTH-03 | JWT com permissões embutidas; refresh token |
| RF-AUTH-04 | Inicialização de tenant pós-login |
| RF-AUTH-05 | Logout invalida sessão frontend |

### Documentos (RF-DOC)

| ID | Requisito |
|----|-----------|
| RF-DOC-01 | Upload chunked para File Repository |
| RF-DOC-02 | Listagem paginada com busca e filtros |
| RF-DOC-03 | Associação de documento a uma ou mais esteiras |
| RF-DOC-04 | Histórico de alterações por documento |
| RF-DOC-05 | Análise: questionários, chat IA, reprovação com justificativa |
| RF-DOC-06 | Anonimização configurável por tipo + prompt |

### Esteiras e cards (RF-WF)

| ID | Requisito |
|----|-----------|
| RF-WF-01 | CRUD de esteiras (workflows) com etapas ordenadas |
| RF-WF-02 | Kanban operacional: cards por etapa/status |
| RF-WF-03 | Avanço manual de etapa; atribuição de usuário |
| RF-WF-04 | Ferramentas por etapa com ordem e dependências |
| RF-WF-05 | Execução automática de ferramentas ao entrar/processar card |
| RF-WF-06 | Permissões por etapa (StepProfilePermission) |

### Ferramentas (RF-TOOL)

| ID | Tipo | Requisito |
|----|------|-----------|
| RF-TOOL-01 | OCR | Extração de texto (Azure/Google) |
| RF-TOOL-02 | Embeddings | Vetorização/indexação |
| RF-TOOL-03 | Prompt | Agente LLM via AI Gateway |
| RF-TOOL-04 | Quiz | Questionário estruturado |
| RF-TOOL-05 | N8N | Automação via conector |
| RF-TOOL-06 | API | Chamada HTTP via ApiTemplate |

### Gestão (RF-MGT)

| ID | Requisito |
|----|-----------|
| RF-MGT-01 | CRUD usuários, times, perfis |
| RF-MGT-02 | Permissões por módulo/ação no perfil |
| RF-MGT-03 | Menu frontend filtrado por permissão JWT |

### Consumo e auditoria (RF-OPS)

| ID | Requisito |
|----|-----------|
| RF-OPS-01 | Dashboard de consumo por tenant |
| RF-OPS-02 | Métricas diárias/mensais (páginas, tokens, automação) |
| RF-OPS-03 | Auditoria de documentos, esteiras e usuários |
| RF-OPS-04 | Notificações SignalR (progresso, anonimização, upload) |

---

## Regras de negócio transversais

| ID | Regra |
|----|-------|
| RN-01 | Documento reprovado retorna à etapa indicada com justificativa obrigatória |
| RN-02 | Parâmetros sensíveis de StepTool são criptografados (AES-GCM) |
| RN-03 | Ferramentas respeitam dependências (`StepToolDependency`) — outputs anteriores alimentam placeholders |
| RN-04 | Card só avança quando regras de ferramentas pendentes forem satisfeitas (OCR/Embeddings etc.) |
| RN-05 | Alterações em entidades auditáveis geram `AuditLog` automaticamente |
| RN-06 | Labels de erro da API (`labelError`) devem existir no i18n frontend |

---

## Padrão para novos RF (SDD)

Ao especificar feature nova, documente:

```markdown
### RF-{MOD}-{NN}: {Título}
- **Ator:** Operador | Gestor | Admin | Sistema
- **Pré-condição:** ...
- **Fluxo principal:** 1. ... 2. ...
- **Pós-condição:** ...
- **Permissão:** Module.Action (ex: Tools.Prompts)
- **UI:** rota Vue + componentes
- **API:** método HTTP + controller
```

---

## Documentação relacionada

- RNF → [`02-requisitos-nao-funcionais.md`](./02-requisitos-nao-funcionais.md)
- Fluxos → [`../04-design-detalhado/04-fluxos.md`](../04-design-detalhado/04-fluxos.md)
- Vocabulário UI → [`../08-anexos/glossario.md`](../08-anexos/glossario.md)
