# WOOPI AI Hub — Documentação de Product Design

Guia de referência para designers, product managers e desenvolvedores que constroem novos serviços, protótipos e interfaces agregados ao ecossistema WOOPI AI Hub. Este documento reflete o **estado atual da aplicação** (`front-end/vueapp`) e deve ser usado como contexto único de padrões visuais, componentes, linguagem e arquitetura de experiência.

---

## Índice

1. [Visão do produto](#1-visão-do-produto)
2. [Arquitetura de informação](#2-arquitetura-de-informação)
3. [Princípios de design](#3-princípios-de-design)
4. [Identidade visual](#4-identidade-visual)
5. [Sistema de cores](#5-sistema-de-cores)
6. [Tipografia](#6-tipografia)
7. [Layout e espaçamento](#7-layout-e-espaçamento)
8. [Biblioteca de componentes](#8-biblioteca-de-componentes)
9. [Iconografia](#9-iconografia)
10. [Voz, tom e microcopy](#10-voz-tom-e-microcopy)
11. [Internacionalização (i18n)](#11-internacionalização-i18n)
12. [Padrões de interação](#12-padrões-de-interação)
13. [Responsividade](#13-responsividade)
14. [Guia para protótipos](#14-guia-para-protótipos)
15. [Referências técnicas](#15-referências-técnicas)

---

## 1. Visão do produto

### O que é

O **WOOPI AI Hub** é uma plataforma B2B multi-tenant para **automatizar e otimizar processos documentais com inteligência artificial**. Integra gestão de documentos, esteiras de processamento (workflows), ferramentas de IA (OCR, embeddings, agentes/prompts, questionários, conectores N8N, templates de API), gestão de usuários e auditoria.

### Proposta de valor (para UX)

| Para quem | O produto entrega |
|-----------|-------------------|
| Operadores / analistas | Analisar documentos, aplicar questionários, conversar com documentos via IA, avançar/reprovar etapas |
| Gestores | Configurar esteiras, ferramentas, perfis de acesso e acompanhar consumo |
| Administradores | Gerenciar usuários, times, tenants e auditar ações |
| Desenvolvedores / integradores | Conectar APIs, templates e automações externas |

### Domínio de linguagem do produto

Use estes termos de forma consistente em protótipos e interfaces:

| Termo na UI (PT) | Significado | Evitar |
|------------------|-------------|--------|
| **Esteira de Processamento** | Workflow operacional onde documentos fluem por etapas | "Pipeline", "Fluxo" (salvo contexto técnico) |
| **Gestão de Esteiras** | Configuração/administração de workflows | "Workflow management" em PT |
| **Agentes** | Prompts de IA configuráveis | "Prompts" na navegação principal |
| **Questionários** | Conjuntos estruturados de perguntas aplicáveis a documentos | "Quizzes" |
| **Conectores** | Integrações de ferramentas (N8N etc.) | "Tools" genérico |
| **Templates de API** | Modelos reutilizáveis de chamadas HTTP | "Templates" isolado |
| **Documento** | Arquivo processado na plataforma | "File" na UI |
| **Tenant** | Ambiente isolado do cliente (multi-tenant) | Traduzir como "Tenant" ou "Ambiente" conforme contexto |
| **Etapa** | Step dentro de uma esteira | "Phase" na UI em PT |

### Módulos funcionais

```
Home
├── Esteiras de Processamento      (Kanban operacional de documentos)
├── Gestão de Esteiras             (Configuração de workflows)
├── Ferramentas
│   ├── Agentes (Prompts)
│   ├── Conectores
│   ├── Templates de API
│   └── Questionários
├── Gestão de Usuários             (Usuários, Times, Perfis)
├── Painel de Consumo              (Dashboard)
└── Auditoria
```

Funcionalidades transversais: **upload de documentos**, **análise com IA**, **anonimização**, **notificações em tempo real** (SignalR), **tema claro/escuro**, **multi-idioma**.

---

## 2. Arquitetura de informação

### Layout autenticado (`defaultLayout`)

```
┌─────────────────────────────────────────────────────────────┐
│ Sidebar (240px / 60px colapsada) │ Navbar (~58px altura)   │
│                                   ├──────────────────────────│
│  Logo                             │ Badge tenant │ 🔔 🌙 🌐 👤│
│  ─────────                        ├──────────────────────────│
│  Home                             │                          │
│  Gestão de Usuários               │   Área de conteúdo       │
│  Esteiras de Processamento        │   (scroll vertical)      │
│  Gestão de Esteiras               │                          │
│  Ferramentas ▾                    │                          │
│  Painel de Consumo                │                          │
│  Auditoria                        │                          │
└─────────────────────────────────────────────────────────────┘
```

- **Sidebar fixa** à esquerda, colapsável (240px → 60px).
- **Navbar fixa** no topo da área de conteúdo, com tenant, notificações, tema, idioma e perfil.
- **Conteúdo** com padding horizontal de 70px em telas ≥ 1025px; scroll na área principal.
- **Separadores** de 1px entre sidebar/navbar/conteúdo (`--color-border-form-control`).

### Layout de autenticação (`authLayout`)

- Tela centralizada, sem sidebar/navbar.
- Card de login (~25rem / 400px) com logo acima.
- Fundo usa `--color-bg-body-content`.

### Hierarquia de navegação secundária

- **Breadcrumb** em páginas internas (links sublinhados).
- **Tabs em pills** (`nav-pills`, `rounded-pill`) para alternar seções dentro de uma página (ex.: Gestão → Usuários / Times / Perfis).
- **Modais** para ações focadas (criar, confirmar, histórico).
- **Offcanvas** para painéis laterais contextuais.

### Permissões e visibilidade

Itens do menu são filtrados por permissões JWT. Ao prototipar, considere estados:
- Item visível e acessível
- Item oculto (sem permissão)
- Página `/unauthorized` quando acesso negado

---

## 3. Princípios de design

1. **Operacional primeiro** — Interfaces densas e funcionais; o usuário executa tarefas repetitivas (análise, avanço de etapas, upload). Priorize eficiência sobre decoração.
2. **Consistência Bootstrap + tokens CSS** — Componentes baseados em Bootstrap 5.0.2, customizados via variáveis CSS (`--color-*`). Novos serviços devem respeitar os mesmos tokens, não cores hardcoded.
3. **Feedback imediato** — Loading spinners, toasts/alerts no canto superior direito, estados de processamento visíveis (upload, anonimização, IA "Encontrando a melhor resposta").
4. **Segurança perceptível** — Confirmações para ações destrutivas; modal com input de validação para exclusões críticas; badge de tenant sempre visível.
5. **Acessível em enterprise** — Suporte a SSO Microsoft, multi-tenant, auditoria completa, tema escuro para uso prolongado.
6. **Extensível** — Chips coloridos por tipo de ferramenta; badges semânticos; padrão tabela + filtros + paginação replicável em CRUDs.

---

## 4. Identidade visual

### Marca

- **Nome oficial:** WOOPI AI (logo: "WOOPI AI")
- **Produto:** WOOPI AI Hub
- **Logos** (`src/assets/img/`):
  - `woopiai-logo-dark.png` — tema claro
  - `woopiai-logo-light.png` — tema escuro
  - `woopiai-hub-small-logo.png` — sidebar colapsada (35×35px)

### Personalidade visual

- **Profissional e confiável** — Paleta azul Bootstrap como primária, verde menta para seleção/sucesso.
- **Enterprise SaaS** — Cards brancos sobre fundo cinza claro; dark mode azul-ardósia profundo.
- **Tecnológico sem ser futurista** — Ícones Lucide lineares; sem gradientes chamativos na UI principal.

---

## 5. Sistema de cores

O tema é controlado por classes no `<html>`:
- `css-theme-light` (padrão)
- `css-theme-dark`

Definição canônica: `front-end/vueapp/src/assets/css/global.css`

### Paleta semântica (ambos os temas)

| Token / Uso | Light | Dark | Aplicação |
|-------------|-------|------|-----------|
| **Primary** | `#0d6efd` | `#0d6efd` | Botões primários, links, ícones ativos, paginação ativa |
| **Primary alt / links** | `#0073e6` | `#0073e6` | Títulos de modal, botão cancel custom, bordas dropzone |
| **Primary dark** | `#123263` | `#123263` | Variante escura de botão primário |
| **Success** | `#0eaa42` | `#0eaa42` | Botões de confirmação positiva |
| **Danger** | `#dc3545` | `#dc3545` | Exclusão, erros |
| **Secondary / muted** | `#6c757d` | `#9196b0` | Texto auxiliar, paginação desabilitada |
| **Selection / active nav** | `#d2f4ea` (verde menta) | `#27447e` (azul) | Item de menu selecionado, hover sidebar |
| **Multiselect tag** | `#10b981` | `#10b981` | Tags de seleção múltipla |

### Superfícies — Tema claro

| Token CSS | Hex | Uso |
|-----------|-----|-----|
| `--color-bg-body-content` | `#f5f5f5` | Fundo geral |
| `--color-bg-main` | `#f8f9fb` | Fundo `<main>` |
| `--color-bg-sidebar-content` | `#ffffff` | Sidebar, navbar |
| `--color-card-content` | `#ffffff` | Cards, inputs |
| `--color-heading-title` | `#212529` | Títulos |
| `--color-body-content` | `#212529` | Texto principal |
| `--color-card-text` | `#212529` | Texto em cards |
| `--color-span-muted` | `#5e5873` | Texto secundário |
| `--color-border-form-control` | `#d3d3d3` | Bordas, separadores |

### Superfícies — Tema escuro

| Token CSS | Hex | Uso |
|-----------|-----|-----|
| `--color-bg-body-content` | `#1f2132` | Fundo geral |
| `--color-bg-main` | `#1f2132` | Fundo main |
| `--color-bg-sidebar-content` | `#1a1b2e` | Sidebar, navbar |
| `--color-card-content` | `#292f4c` | Cards, inputs, tabelas |
| `--color-heading-title` | `#e2e4ea` | Títulos |
| `--color-body-content` | `#d5d8e0` | Texto principal |
| `--color-card-text` | `#b0b4c8` | Texto em cards |
| `--color-border-form-control` | `#393e5c` | Bordas |

### Badges semânticos

| Variante | Background (light) | Texto (light) |
|----------|-------------------|---------------|
| Primary | `#dbeafe` | `#193cb8` |
| Success | `#a0dfc2` | `#408264` |
| Warning | `#ffe082` | `#ffc107` |
| Error | `#e5848d` | `#dc3545` |

Componente `BadgeComponent` usa variantes adicionais: `danger`, `info`, `secondary` com paleta pastel fixa.

### Chips por tipo de ferramenta (workflow)

Use estas cores para identificar visualmente tipos de step/tool:

| Tipo | BG (light) | Texto | Borda |
|------|------------|-------|-------|
| **N8N** | `#fff3e0` | `#bf5a00` | `#ffcc80` |
| **Prompt / Agente** | `#e8f5e9` | `#256329` | `#a5d6a7` |
| **API** | `#e3f2fd` | `#1054a0` | `#90caf9` |
| **Quiz / Questionário** | `#f3e5f5` | `#5a1782` | `#ce93d8` |
| **Embeddings** | `#e0f2f1` | `#004d40` | `#80cbc4` |
| **Default** | `#f5f5f5` | `#424242` | `#bdbdbd` |

Tokens: `--chip-{tipo}-bg`, `--chip-{tipo}-text`, `--chip-{tipo}-border`

### Kanban (esteiras)

| Estado | BG | Texto/accent |
|--------|-----|--------------|
| Success | `#d0fae5` | `#007a55` |
| Warning | `#fef9c2` | `#a65f00` |
| Danger | `#ffedd4` | `#ca3500` |
| Primary | `#dbeafe` | `#2b7fff` |

### Toasts / Alertas

Variantes: `primary`, `success`, `warning`, `danger`, `upload`

Posição: **canto superior direito**, empilhados, fade in/out 0.5s, duração padrão 3000ms (6000ms para erros de login).

### Cores de ícones do menu (referência)

| Módulo | Cor |
|--------|-----|
| Home | `#0d6efd` |
| Gestão | `#ff6900` |
| Esteiras | `#615FFF` |
| Gestão de Esteiras | `#06b6d4` |
| Ferramentas | `#8b5cf6` |
| Dashboard | `#40b04d` |
| Auditoria | `#f56565` |

---

## 6. Tipografia

### Família

```css
--font-family-base: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
```

Em protótipos Figma, use **Segoe UI** (Windows) ou **Inter** / **System UI** como substituto cross-platform.

### Escala

| Elemento | Tamanho | Peso | Observação |
|----------|---------|------|------------|
| Body padrão | `0.875rem` (14px) | 400 | Definido em `body` |
| Texto pequeno (`.text-sm`, `.mfs`) | `0.775rem` (~12.4px) | 400 | Metadados, contadores de tabela |
| H1 (home) | `2rem` (32px) | 700 | Título de boas-vindas |
| H2 (seções) | `1.5rem` (24px) | 700 | Ex.: "Trilha de Iniciação Rápida" |
| H5 (cards) | `1.25rem` (20px) | 600 | Títulos de card |
| H5 modal | — | — | Cor `--color-h5-custom-modal` (#0073e6) |
| H6 (login) | Bootstrap default | 700 | Título do card de login |
| Badge | `13px` (md) / `11px` (sm) | 500 | `BadgeComponent` |
| Validação de erro | `0.7rem` | 500 | `.validation-message`, cor `#dc3545` |
| Semibold | — | 500 | `.fw-semibold` |

### Estilo de texto

- **Títulos:** cor `--color-heading-title` ou `--color-card-title`
- **Subtítulos / auxiliar:** `.text-muted` → `--color-text-muted`
- **Links em breadcrumb:** sublinhados
- **Links de ação (cards home):** `#005ebc`, peso 500, seta `→` como sufixo

---

## 7. Layout e espaçamento

### Grid

- **Framework:** Bootstrap 5 grid (`container-fluid`, `row`, `col-*`)
- **Largura máxima conteúdo (home):** 1200px centralizado
- **Container principal:** padding `15px` (mobile) → `22px 15px` (≥768px)

### Dimensões fixas

| Elemento | Valor |
|----------|-------|
| Sidebar expandida | 240px |
| Sidebar colapsada | 60px |
| Altura header sidebar | 60px |
| Altura navbar | ~58px |
| Área scroll conteúdo | `calc(100vh - 60px)` |
| Modal | `modal-dialog-centered`, largura Bootstrap default |
| Card border-radius (home) | 12px |
| Card border-radius (geral) | Bootstrap default + `.main-div` 8px |
| Tabela border-radius | 9px |
| Paginação item ativo | border-radius 9px |
| Input height (btn cancel) | 38px |

### Espaçamento recorrente

| Contexto | Valor |
|----------|-------|
| `.main-div` padding | 20px 24px |
| Card body (home quick-start) | 2rem |
| Gap nav-buttons | 10px |
| Gap tabs / ícones | 10px |
| Margem paginação entre items | 4px |

### Sombras

- Cards home: `0 4px 6px rgba(0,0,0,0.1)` (plan), `0 2px 4px rgba(0,0,0,0.05)` (quick-start)
- Hover quick-start: `0 8px 16px rgba(0,0,0,0.1)` + `translateY(-5px)`
- Tabela: `shadow-sm`
- Modal backdrop: `rgba(0,0,0,0.7)`

### Container de formulários

Classe `.main-div` — card com borda, padding e fundo de card. Use para blocos de formulário em páginas internas.

---

## 8. Biblioteca de componentes

Componentes globais reutilizáveis em `src/components/global/`. **Novos serviços devem reutilizar estes padrões**, não reinventar.

### Navegação e layout

| Componente | Responsabilidade |
|------------|------------------|
| `SidebarComponent` | Menu principal, collapse, ícones coloridos |
| `NavbarComponent` | Tenant badge, notificações, tema, idioma, perfil |
| `LogoComponent` | Logo adaptativo ao tema |
| `BreadcrumbComponent` | Trilha de navegação |
| `ThemeSwitchComponent` | Toggle claro/escuro (Sol/Lua) |
| `LanguageComponent` | Seletor PT / EN / ES |
| `RouteListComponent` | Lista de rotas do sidebar |

### Dados e listagens

| Componente | Padrão visual |
|------------|---------------|
| `TableComponent` | Tabela hover, header com ordenação (Lucide MoveUp/Down/ArrowDownUp), checkbox seleção, spinner loading, contador no topo |
| `PaginationComponent` | Centralizada, ícones Chevrons, página ativa azul arredondada |
| `SearchComponent` | Input de busca com ícone |
| `TabsComponent` | Pills arredondadas, ícone opcional, variante compact |
| `ActionTableListComponent` | Botões de ação inline na tabela |

### Formulários

| Componente | Padrão |
|------------|--------|
| `PasswordInputComponent` | Input com toggle visibilidade |
| `@vueform/multiselect` | Tags verdes, ring verde no focus |
| `form-control` / `form-select` | Bootstrap customizado via CSS vars |
| VeeValidate + Yup | Validação; mensagem `.validation-message` vermelha abaixo |
| Input group | Ícone Lucide à esquerda (ex.: Mail, Lock no login) |

### Feedback

| Componente | Comportamento |
|------------|---------------|
| `NotificationComponent` | Alert Bootstrap, top-right, auto-dismiss |
| `LoadingComponent` | Spinner inline |
| `FullscreenLoadingComponent` | Overlay de carregamento |
| `ModalComponent` | Header + body + footer (Cancel outline + Save primary) |
| `ConfirmModal` | Ícone outline + título + mensagem + Cancel/Confirm |
| `ConfirmModalValidationInput` | Confirmação com digitação obrigatória |

### Outros

| Componente | Uso |
|------------|-----|
| `BadgeComponent` / `BadgeOutlinedComponent` | Status, contadores |
| `AvatarComponent` | Avatar de usuário |
| `DropdownComponent` | Menus contextuais |
| `OffcanvasComponent` | Painel lateral |
| `CollapseComponent` / `AccordionComponent` | Conteúdo expansível |
| `TransferListComponent` / `SelectionListComponent` | Seleção dual-list |
| `LucideIcon` | Wrapper universal de ícones |
| `WhatsAppComponent` | FAB suporte (condicional por env) |

### Botões — matriz de uso

| Classe | Quando usar |
|--------|-------------|
| `btn btn-primary` | Ação principal (Salvar, Entrar, Confirmar) |
| `btn btn-outline-primary` | Ação secundária, SSO, ícones navbar, cancel em tabelas |
| `btn btn-outline-secondary` | Cancelar em modais padrão |
| `btn btn-danger` | Excluir, reprovar |
| `btn btn-success` | Confirmações positivas específicas |
| `btn btn-secondary` | Ações neutras |
| `btn btn-link` | Links inline em cards |
| `btn-custom-cancel` | Cancel custom (fundo `#f0f7ff`, texto `#0073e6`) |
| `btn-sm` | Compacto (login, navbar) |
| `table-btn` | Botões em células; hover scale 1.05 |

**Regras:** sem box-shadow no focus (`box-shadow: none`); spinner `spinner-grow-sm` durante loading em botões.

### Tabelas

- Classe: `table table-hover table-sm table-striped`
- Wrapper: `.table-div.shadow-sm`
- Botões de ação: `table-btn btn-outline-{variant}`
- Alinhamento: coluna `actions` à direita (`.text-end`)

### Modais

- Estrutura Bootstrap 5 modal
- Header com borda inferior
- Footer com borda superior
- Título modal confirmação: pode incluir botão-ícone desabilitado como indicador visual
- Backdrop escuro 70%

### Upload (Dropzone)

- Borda tracejada 2px
- Ícone SVG centralizado (`upload-dropzone.svg`)
- Preview com thumbnail border-radius 20px
- Remover arquivo: ícone X Font Awesome

---

## 9. Iconografia

### Biblioteca principal: Lucide

- Pacote: `lucide-vue-next`
- Componente: `<LucideIcon icon="NomeDoIcone" :size="20" color="#0d6efd" />`
- Stroke width padrão: `1.5`
- Cor padrão: `currentColor`

### Tamanhos recorrentes

| Contexto | Size |
|----------|------|
| Navbar / sidebar / tabela | 15–20 |
| Login input | 16 |
| Home cards | 48 |
| Notificações | 20 |

### Ícones mapeados por área

| Área | Ícones Lucide |
|------|---------------|
| Navegação | Home, Users, Kanban, Workflow, PocketKnife, Bot, Plug, Zap, ClipboardList, ChartColumn, ShieldUser |
| Navbar | Moon, Sun, Globe, LogOut, Bell |
| Sidebar toggle | ChevronLeft, ChevronRight |
| Paginação | ChevronsLeft, ChevronLeft, ChevronRight, ChevronsRight |
| Ordenação tabela | MoveUp, MoveDown, ArrowDownUp |
| Login | Mail, Lock, Eye, EyeClosed, LogIn, ArrowRight |
| Notificações | CircleCheck, CircleX, CircleAlert, AlertTriangle, MessageCircle |
| Home | Video, BookOpen, FileText |

### Biblioteca secundária: Font Awesome 5.15.4

Usada pontualmente (dropzone remove, spinner login legacy, theme icon). **Preferir Lucide em novos desenvolvimentos.**

### SSO

Logo Microsoft: `microsoft-log.svg` (30×15) no botão outline-primary.

---

## 10. Voz, tom e microcopy

### Personalidade

- **Clara e direta** — Frases curtas, verbos no imperativo para ações ("Salvar", "Excluir", "Aplicar").
- **Profissional em PT-BR** — Tratamento formal implícito; sem gírias.
- **Orientada a resultado** — Mensagens de sucesso confirmam o que aconteceu ("Questionário aplicado com sucesso").
- **Transparente em erros** — Explica o problema e, quando possível, a ação ("Por favor, preencha todos os campos obrigatórios").

### Padrões de redação

| Situação | Padrão | Exemplo |
|----------|--------|---------|
| Ação destrutiva | Pergunta + consequência | "Esta ação não poderá ser desfeita. Tem certeza que deseja removê-lo?" |
| Campo obrigatório vazio | Imperativo + contexto | "Por favor, selecione um questionário" |
| Loading | Gerúndio ou reticências | "Carregando...", "Processando" |
| Empty state | Neutro, sem culpa | "Nenhum dado disponível." |
| Placeholder select | "Selecione..." + contexto | "Selecione um questionário..." |
| Placeholder input | Exemplo concreto | `usuario@empresa.com` |
| Confirmação positiva | "{Entidade} {verbo} com sucesso" | "Documento reprovado com sucesso." |
| Erro genérico | "Erro desconhecido." | Fallback quando API não retorna label |
| Opcional | "(opcional)" ou label "Opcional" | Campos não obrigatórios |

### Vocabulário de ações (chaves `common.*`)

Reutilize estes labels em novos módulos:

`Salvar`, `Cancelar`, `Confirmar`, `Excluir`, `Editar`, `Criar`, `Voltar`, `Fechar`, `Aplicar`, `Enviar`, `Carregar` (upload), `Avançar`, `Finalizar`, `Reprocessar`, `Limpar`, `Consultar`, `Entendi`, `Atenção`, `Sucesso`, `Erro`

### Títulos de página

Formato preferido: **{Ação/Entidade}** como H1 ou título de seção.
Subtítulo: frase descritiva em `.text-muted` (ex.: "Gerencia análises de documentos").

### Notificações

Estrutura: **{Título traduzido}: {Mensagem traduzida}**
- Título = contexto (ex.: "Login", "Sucesso")
- Variantes: `success`, `danger`, `warning`, `primary`/`info`

---

## 11. Internacionalização (i18n)

### Idiomas suportados

| Código | Idioma | Fallback |
|--------|--------|----------|
| `pt` | Português (Brasil) | **Sim (default)** |
| `en` | English | — |
| `es` | Español | — |

Arquivos: `src/locales/translations/{pt,en,es}.js`

### Convenções de chaves

```
common.*          → Labels globais reutilizáveis
pages.*           → Nomes de módulos no menu
{modulo}.*        → Textos específicos (analyze, login, management...)
{modulo}.{sub}.*  → Subseções (analyze.rejection.*)
```

**Novos serviços:** adicionar chaves nos 3 arquivos simultaneamente. Nunca hardcodar strings na UI.

### Interpolação

```javascript
// Exemplo
"changesCount": "{count} alterações registradas"
```

### Regras para protótipos

1. Prototipe em **PT-BR** como idioma principal.
2. Indique strings traduzíveis com notation `[i18n: common.save]`.
3. Reserve espaço para textos ~30% maiores (ES/EN).
4. Não use HTML em mensagens i18n (CSP-friendly).

---

## 12. Padrões de interação

### Autenticação

1. Login email/senha **ou** SSO Microsoft
2. Se múltiplos tenants → modal de seleção
3. Redirect para `/home` após sucesso
4. Token JWT + refresh automático

### CRUD padrão

```
Listagem (tabela + busca + filtros + paginação)
  → Botão "Novo" (btn-primary)
  → Formulário (main-div ou card)
  → Salvar → toast success → redirect listagem
  → Excluir → ConfirmModal → toast
```

### Análise de documentos

- Layout split: documento + painel IA/questionário
- Estados: loading IA ("Encontrando a melhor resposta"), confirmed, rejected
- Ações de fluxo: Avançar, Reprovar (modal com justificativa), Finalizar
- Históricos via modais dedicados

### Upload

- Dropzone drag-and-drop
- Notificações de progresso por arquivo (toast upload)
- Toast consolidado ao finalizar batch

### Tempo real

- SignalR para anonimização concluída, notificações
- Indicador na navbar (`NavbarNotificationComponent`)

### Tema

- Persistido em `localStorage.theme`
- Toggle instantâneo via class no `<html>`
- Logos invertem conforme tema

---

## 13. Responsividade

### Breakpoints (Bootstrap + custom)

| Breakpoint | Comportamento |
|------------|---------------|
| `< 768px` | Sidebar colapsada por default; username oculto na navbar |
| `≥ 768px` | Padding conteúdo aumentado |
| `≥ 1025px` | Padding horizontal 70px (`.custom-padding`) |
| `< 767px` | Elementos `.d-inline-custom` ocultos |

### Regras mobile

- Sidebar: slide-in com overlay (`z-index: 1060`)
- Dropdowns de usuário: alinhados à direita
- Tabelas: `table-responsive` wrapper
- Login: card 100% width até 25rem max

---

## 14. Guia para protótipos

### Stack recomendada para alinhamento

| Ferramenta | Configuração sugerida |
|------------|----------------------|
| **Figma** | Frame 1440×900; sidebar 240px; usar tokens deste doc como Color Styles |
| **FigJam / Miro** | Fluxos de esteira e análise documental |
| **Código** | Vue 3 + Bootstrap 5 + CSS vars (espelhar vueapp) |

### Checklist de novo serviço/protótipo

- [ ] Usar layout autenticado (sidebar + navbar) salvo telas públicas
- [ ] Aplicar tokens de cor (light + dark)
- [ ] Tipografia Segoe UI 14px base
- [ ] Botão primário `#0d6efd`; seleção `#d2f4ea` (light)
- [ ] Ícones Lucide lineares, stroke 1.5
- [ ] Tabela + paginação + busca para listagens
- [ ] Modal de confirmação para ações destrutivas
- [ ] Toasts top-right para feedback
- [ ] Strings via chaves i18n (PT/EN/ES)
- [ ] Estados: loading, empty, error, success
- [ ] Badge de tenant visível
- [ ] Respeitar permissões de menu
- [ ] Border-radius cards 8–12px; inputs Bootstrap padrão

### Wireframe de página CRUD (template)

```
┌──────────────────────────────────────────────────────────┐
│ Breadcrumb: Home > Módulo > Listagem                     │
├──────────────────────────────────────────────────────────┤
│ Título da Página                          [+ Novo]      │
│ Subtítulo descritivo (.text-muted)                       │
├──────────────────────────────────────────────────────────┤
│ [🔍 Buscar...        ] [Filtro ▾] [Filtro ▾]             │
├──────────────────────────────────────────────────────────┤
│ ┌────────────────────────────────────────────────────┐   │
│ │ Tabela (N registros)                               │   │
│ │ □ | Col 1 | Col 2 | Status | Ações                 │   │
│ │───|───────|───────|────────|────────────────────────│   │
│ │ □ | ...   | ...   | Badge  | ✏️ 🗑️                  │   │
│ └────────────────────────────────────────────────────┘   │
│              « ‹  1  2  3  › »                           │
└──────────────────────────────────────────────────────────┘
```

### Wireframe de formulário (template)

```
┌──────────────────────────────────────────────────────────┐
│ Breadcrumb: Home > Módulo > Novo                         │
├──────────────────────────────────────────────────────────┤
│ Título: Novo {Entidade}                                  │
│ Subtítulo: Crie um novo...                               │
├──────────────────────────────────────────────────────────┤
│ ┌ .main-div ──────────────────────────────────────────┐  │
│ │ Label                                                │  │
│ │ [ Input                                    ]         │  │
│ │                                                      │  │
│ │ Label *                                              │  │
│ │ [ Select ▾                                 ]         │  │
│ │ mensagem de validação (vermelho, 0.7rem)             │  │
│ │                                                      │  │
│ │              [Cancelar]  [Salvar]                    │  │
│ └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```

### Export de tokens para Figma

Crie Color Styles nomeados:

```
brand/primary         #0d6efd
brand/success         #0eaa42
brand/danger          #dc3545
surface/bg            #f5f5f5 (light) | #1f2132 (dark)
surface/card          #ffffff (light) | #292f4c (dark)
surface/sidebar       #ffffff (light) | #1a1b2e (dark)
text/primary          #212529 (light) | #d5d8e0 (dark)
text/muted            #6c757d
border/default        #d3d3d3 (light) | #393e5c (dark)
state/selected        #d2f4ea (light) | #27447e (dark)
```

---

## 15. Referências técnicas

| Recurso | Caminho |
|---------|---------|
| CSS global / tokens | `front-end/vueapp/src/assets/css/global.css` |
| Componentes globais | `front-end/vueapp/src/components/global/` |
| Layout autenticado | `front-end/vueapp/src/layouts/defaultLayout.vue` |
| Traduções PT | `front-end/vueapp/src/locales/translations/pt.js` |
| Rotas / módulos | `front-end/vueapp/src/router/index.js` |
| Menu sidebar | `front-end/vueapp/src/components/layout/SidebarComponent.vue` |
| Logos | `front-end/vueapp/src/assets/img/` |
| README técnico | `README.md` |
| Bootstrap | 5.0.2 (bundled em assets) |

### Dependências UI relevantes

- Vue 3 + Vue Router 4 + Vuex 4
- Bootstrap 5.0.2
- Lucide Vue Next
- VeeValidate + Yup
- @vueform/multiselect
- ApexCharts (dashboard)
- @vue-flow/core (editor de fluxo)
- floating-vue (tooltips)

---

## Changelog

| Data | Versão | Notas |
|------|--------|-------|
| 2026-06-22 | 1.0 | Documento inicial extraído do front-end vueapp |

---

> **Manutenção:** Ao alterar tokens em `global.css`, componentes globais ou chaves i18n, atualize este documento para manter protótipos e novos serviços sincronizados com a aplicação.
