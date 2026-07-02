# SDD — WOOPI AI Hub

**Spec-Driven Development (SDD)** — documentação contextual e instruções operacionais para agentes de IA e desenvolvedores que implementam novas funcionalidades no WOOPI AI Hub.

---

## Objetivo deste documento

Este pacote SDD é o **ponto de entrada único** antes de qualquer implementação. Ele consolida visão de produto, arquitetura, requisitos, design, segurança, testes e operação — com referências às documentações detalhadas do repositório.

| Documento estendido | Conteúdo |
|---------------------|----------|
| [`../PRODUCT_DESIGN.md`](../PRODUCT_DESIGN.md) | Design system, UX, componentes, i18n, protótipos |
| [`../BACKEND_ARCHITECTURE.md`](../BACKEND_ARCHITECTURE.md) | Backend completo: camadas, domínio, filas, multitenancy |
| [`../API_TEMPLATES.md`](../API_TEMPLATES.md) | Módulo Templates de API (detalhe) |
| [`../../AGENTS.md`](../../AGENTS.md) | Convenções de código e testes (obrigatório) |
| [`../GIT_HOOKS_BEST_PRACTICES.md`](../GIT_HOOKS_BEST_PRACTICES.md) | Hooks de formatação |

---

## Escopo do sistema

**WOOPI AI Hub** — plataforma B2B multi-tenant para automatizar processos documentais com IA: documentos, esteiras de processamento, ferramentas (OCR, Embeddings, Agentes, Questionários, Conectores, Templates de API), gestão de usuários, consumo e auditoria.

**Stack:** .NET 8 API + Vue 3 SPA + SQL Server (por tenant) + Redis + RabbitMQ + SignalR.

---

## Stakeholders e referências

| Papel | Interesse |
|-------|-----------|
| Operador / Analista | Análise de documentos, questionários, avanço de etapas |
| Gestor | Configuração de esteiras, ferramentas, perfis, consumo |
| Administrador | Usuários, times, tenants, auditoria |
| Desenvolvedor / Agente IA | Extensão do produto seguindo SDD e padrões do repo |

**Repositório:** `woopiai-hub/`  
**Frontend:** `front-end/vueapp/`  
**Backend:** `back-end/WoopiAiHub.*`  
**Testes:** `tests/WoopiAiHub.UnitTests/`

---

## Como navegar o SDD

```
SDD/
├── README.md                          ← Você está aqui (instruções do agente)
├── 01-visao-geral/                    ← Contexto e objetivos
├── 02-requisitos/                     ← RF, RNF, regras de negócio
├── 03-arquitetura/                    ← Visão, diagramas, tecnologias
├── 04-design-detalhado/               ← Dados, módulos, interfaces, fluxos
├── 05-seguranca/                      ← Auth, tenant, controles
├── 06-testes/                         ← Estratégia e casos
├── 07-operacional/                    ← Deploy, monitoramento, backup
└── 08-anexos/                         ← Glossário, exemplos, templates
```

**Dica:** mantenha arquivos curtos, objetivos e bem referenciados entre si. Ao implementar uma nova funcionalidade, **atualize a seção SDD correspondente** antes ou junto com o código.

---

## Instruções para o agente de IA

### Princípio SDD

> **Spec first, code second.** Nenhuma implementação começa sem entender o contexto deste SDD e, para features novas, sem um mini-spec documentado (usar [`08-anexos/template-nova-funcionalidade.md`](./08-anexos/template-nova-funcionalidade.md)).

### Fluxo obrigatório para nova funcionalidade

```
1. CONTEXTO
   └─ Ler README.md + seções SDD relevantes
   └─ Ler AGENTS.md (convenções de código)

2. ESPECIFICAR
   └─ Preencher template em 08-anexos/ (ou seções 02 + 04)
   └─ Definir: escopo, RF, entidades, endpoints, telas, permissões

3. VALIDAR ESCOPO
   └─ Confirmar alinhamento com vocabulário (08-anexos/glossario.md)
   └─ Confirmar padrões UI (PRODUCT_DESIGN.md)
   └─ Confirmar camadas backend (BACKEND_ARCHITECTURE.md seção 21)

4. IMPLEMENTAR
   └─ Backend: Domain → Repository → Application → Api (+ migration se persistir)
   └─ Frontend: pages + components + services + i18n (pt/en/es)
   └─ Respeitar multitenancy, headers, permissões

5. TESTAR
   └─ Testes unitários em Services (06-testes/)
   └─ Verificar lint/format se alterou frontend

6. DOCUMENTAR
   └─ Atualizar SDD se comportamento/contrato mudou
   └─ Atualizar PRODUCT_DESIGN ou BACKEND_ARCHITECTURE se padrão novo
```

### Regras inegociáveis

| Área | Regra |
|------|-------|
| **Leitura C#** | Métodos de leitura usam prefixo `Find`, nunca `Get` |
| **Exceções** | Negócio → `AppException(ErrorCode, message, labelError)` |
| **Tenant** | Header `X-Tenant` + validação JWT; banco isolado por tenant |
| **Headers API** | `X-Email`, `X-Tenant`, `X-Language` via `HeadersDto` |
| **UI strings** | Nunca hardcodar — chaves i18n em `pt.js`, `en.js`, `es.js` |
| **UI visual** | Tokens CSS em `global.css`; componentes globais existentes |
| **Testes** | Todo método público novo em `Services` → teste unitário |
| **Commits** | Só commitar quando o usuário pedir explicitamente |
| **Escopo** | Diff mínimo; não refatorar código não relacionado |

### Decisão: sync vs async (backend)

```
Processamento rápido / CRUD?
  → Service + Repository + Controller (HTTP síncrono)

Processamento longo / serviço externo / fila?
  → IToolHandler ou Consumer RabbitMQ
  → ContinueExecution ao finalizar
```

Ver [`04-design-detalhado/04-fluxos.md`](./04-design-detalhado/04-fluxos.md).

### Decisão: componentes UI

```
Listagem?
  → TableComponent + SearchComponent + PaginationComponent + ConfirmModal

Formulário?
  → .main-div + form-control + VeeValidate + ModalComponent (se modal)

Feedback?
  → $notify (NotificationComponent) ou ConfirmModal
```

Ver [`../PRODUCT_DESIGN.md`](../PRODUCT_DESIGN.md) seções 8 e 14.

### Ordem de leitura recomendada (primeira sessão)

1. Este README
2. [`01-visao-geral/01-contexto.md`](./01-visao-geral/01-contexto.md)
3. [`03-arquitetura/01-visao-arquitetural.md`](./03-arquitetura/01-visao-arquitetural.md)
4. [`05-seguranca/01-autenticacao-autorizacao.md`](./05-seguranca/01-autenticacao-autorizacao.md)
5. [`../PRODUCT_DESIGN.md`](../PRODUCT_DESIGN.md) — seções 1, 5, 8, 10, 11
6. [`../BACKEND_ARCHITECTURE.md`](../BACKEND_ARCHITECTURE.md) — seções 1, 4, 7, 15, 21
7. [`../../AGENTS.md`](../../AGENTS.md)

### Prompt inicial sugerido (copiar para o agente)

```text
Você desenvolve funcionalidades para o WOOPI AI Hub.
Antes de codificar:
1. Leia docs/SDD/README.md e as seções SDD relevantes à tarefa.
2. Siga AGENTS.md para convenções C# e testes.
3. UI: docs/PRODUCT_DESIGN.md | Backend: docs/BACKEND_ARCHITECTURE.md
4. Para feature nova, preencha docs/SDD/08-anexos/template-nova-funcionalidade.md
5. Implemente diff mínimo; multitenancy e i18n são obrigatórios.
```

---

## Índice rápido por tarefa

| Tarefa | Ler primeiro |
|--------|--------------|
| Nova tela Vue | PRODUCT_DESIGN §8, §11, §14 + SDD 04-design §02 |
| Novo endpoint REST | BACKEND_ARCHITECTURE §5, §6, §21 + SDD 04-design §03 |
| Nova entidade/tabela | BACKEND_ARCHITECTURE §4, §18 + SDD 04-design §01 |
| Nova ferramenta de esteira | BACKEND_ARCHITECTURE §9, §10 + API_TEMPLATES (se API) |
| Permissões / auth | SDD 05-seguranca + BACKEND_ARCHITECTURE §6 |
| Testes | SDD 06-testes + AGENTS.md §2 |
| Deploy local | SDD 07-operacional + README.md raiz |

---

## Changelog SDD

| Data | Versão | Notas |
|------|--------|-------|
| 2026-06-22 | 1.0 | Estrutura SDD inicial + instruções para agente IA |
