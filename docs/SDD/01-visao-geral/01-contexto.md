# 01 — Contexto

> Parte de [`../README.md`](../README.md) · Visão geral do SDD

---

## Motivação

Organizações processam grandes volumes de documentos em fluxos repetitivos que exigem extração de dados, validação humana, integração com sistemas externos e uso de modelos de linguagem. Ferramentas isoladas (OCR, chat, automação) não resolvem o ciclo completo.

O **WOOPI AI Hub** unifica esse ciclo em uma plataforma enterprise: upload → processamento IA → esteira configurável → auditoria → consumo medido — com isolamento por **tenant**.

---

## Problema que resolve

| Dor | Solução no produto |
|-----|-------------------|
| Documentos em múltiplos sistemas | Hub central com File Repository |
| Processos manuais repetitivos | Esteiras com etapas, cards e automação |
| IA sem contexto documental | OCR + Embeddings + Agentes encadeados |
| Falta de rastreabilidade | Histórico de documento + auditoria |
| Gestão de acesso complexa | Perfis, permissões, times, multi-tenant |
| Integrações ad hoc | Templates de API, Conectores N8N |

---

## Contexto técnico

```
┌──────────────┐     HTTPS/JWT      ┌─────────────────┐
│  Vue 3 SPA   │ ◄────────────────► │  WoopiAiHub.Api │
│  (vueapp)    │     SignalR        │  (.NET 8)        │
└──────────────┘                    └────────┬─────────┘
                                             │
                    ┌────────────────────────┼────────────────────────┐
                    ▼                        ▼                        ▼
             SQL Server              RabbitMQ + Workers           APIs externas
             (por tenant)            (OCR, LLM, API…)            (Refit)
                    │
             Redis (cache tenant)
```

---

## Usuários e jornadas principais

### Operador
Upload de documento → documento entra na esteira → analisa (questionário / chat IA) → avança ou reprova etapa.

### Gestor de processo
Cria esteira → define etapas → associa ferramentas (OCR, Agente, API…) → associa times e perfis.

### Administrador
Gerencia usuários, perfis, permissões; consulta auditoria e painel de consumo.

---

## Restrições de contexto

- **Multi-tenant obrigatório** — dados nunca cruzam tenants
- **Processamento assíncrono** — OCR/LLM/API não bloqueiam HTTP por longos períodos
- **Enterprise** — SSO Microsoft, auditoria, permissões granulares
- **i18n** — PT (primário), EN, ES

---

## Documentação relacionada

- Objetivos → [`02-objetivos.md`](./02-objetivos.md)
- Design UX completo → [`../../PRODUCT_DESIGN.md`](../../PRODUCT_DESIGN.md) §1–2
- Arquitetura → [`../03-arquitetura/01-visao-arquitetural.md`](../03-arquitetura/01-visao-arquitetural.md)
