# 02 — Objetivos

> Parte de [`../README.md`](../README.md) · Visão geral do SDD

---

## Objetivos de produto

1. **Automatizar** processamento documental com IA configurável por esteira
2. **Orquestrar** ferramentas (OCR, Embeddings, Agentes, Questionários, API, N8N) em sequência com dependências
3. **Governar** acesso via perfis, permissões e auditoria
4. **Isolar** clientes em tenants com banco dedicado
5. **Medir** consumo (páginas, tokens, automações) para billing/quotas
6. **Notificar** usuários em tempo real (SignalR) sobre progresso e eventos

---

## Objetivos técnicos (para implementação)

| # | Objetivo | Como medir |
|---|----------|------------|
| T1 | Extensibilidade por camadas | Nova feature segue checklist BACKEND_ARCHITECTURE §21 |
| T2 | Consistência UI | Novas telas usam tokens e componentes globais |
| T3 | Contratos estáveis | DTOs + labelError i18n; Swagger documentado |
| T4 | Testabilidade | Services com cobertura xUnit + Moq |
| T5 | Observabilidade | Logs, health check, métricas de uso |
| T6 | Segurança | JWT + tenant binding + criptografia de parâmetros sensíveis |

---

## Requisitos de alto nível

- RF-01: Usuário autentica via email/senha ou SSO Microsoft
- RF-02: Usuário opera dentro de um tenant selecionado
- RF-03: Usuário faz upload e acompanha documentos em esteiras
- RF-04: Gestor configura esteiras, etapas e ferramentas
- RF-05: Sistema executa ferramentas assincronamente via filas
- RF-06: Sistema registra histórico e auditoria de ações
- RF-07: Interface disponível em PT, EN, ES com tema claro/escuro

Detalhamento → [`../02-requisitos/`](../02-requisitos/)

---

## Fora de escopo (default)

Salvo spec explícita contrária:

- Mobile nativo (apenas web responsiva)
- Multi-região automática de banco
- Edição colaborativa em tempo real de documentos
- Substituição completa do RabbitMQ por outro broker

---

## Premissas

- SQL Server, Redis e RabbitMQ disponíveis no ambiente
- File Repository operacional para armazenamento de arquivos
- Workers externos consomem filas de OCR/LLM/API quando aplicável
- Frontend consome API em `VUE_APP_BASE_URL_API`

---

## Documentação relacionada

- Contexto → [`01-contexto.md`](./01-contexto.md)
- Requisitos funcionais → [`../02-requisitos/01-requisitos-funcionais.md`](../02-requisitos/01-requisitos-funcionais.md)
