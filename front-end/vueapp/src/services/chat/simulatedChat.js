const DEMO_DOCUMENT_MARKDOWN = `# Análise do Contrato de Prestação de Serviços

## Partes
- **Contratante:** WOOPI Tecnologia Ltda.
- **Contratada:** Fornecedor Alpha S.A.

## Resumo executivo
Contrato de 24 meses para suporte de infraestrutura cloud, com SLA de **99,5%** e revisão trimestral de preços.

## Cláusulas críticas
| Cláusula | Risco | Observação |
|----------|-------|------------|
| 4.2 – Rescisão | Médio | Multa de 20% sobre saldo remanescente |
| 7.1 – LGPD | Baixo | DPA anexo conforme exigido |
| 9.3 – SLA | Alto | Penalidade de 5% da mensalidade por incidente |

## Recomendações
1. Negociar redução da multa rescisória para 10%.
2. Incluir cláusula de auditoria anual de segurança.
3. Exportar versão final em **PDF** após homologação jurídica.`;

const DEMO_EXPORT_DOCUMENT = {
    title: "Relatório_Estruturado_Contrato_Alpha.docx",
    format: "docx",
    preview: "Documento gerado com seções: Resumo, Partes, Cláusulas críticas e Recomendações.",
};

export function buildDemoConversation(agentId) {
    const now = Date.now();
    const ts = (offsetMs) => new Date(now - offsetMs).toISOString();

    return [
        {
            id: crypto.randomUUID(),
            role: "user",
            content:
                "Preciso analisar o contrato anexo e receber um relatório estruturado para exportação.",
            attachments: [
                {
                    name: "Contrato_Prestacao_Servicos_Alpha.pdf",
                    type: "application/pdf",
                    size: 2840000,
                },
            ],
            timestamp: ts(3600000),
        },
        {
            id: crypto.randomUUID(),
            role: "assistant",
            content:
                "Recebi o documento **Contrato_Prestacao_Servicos_Alpha.pdf** (2,7 MB). Iniciei a leitura estruturada e identifiquei 12 cláusulas relevantes. Segue o relatório em Markdown pronto para exportação:",
            documentResponse: {
                title: "Relatorio_Analise_Contrato_Alpha.md",
                format: "markdown",
                content: DEMO_DOCUMENT_MARKDOWN,
            },
            timestamp: ts(3540000),
        },
        {
            id: crypto.randomUUID(),
            role: "user",
            content: "Quais são os três principais riscos e qual a penalidade de SLA?",
            timestamp: ts(1800000),
        },
        {
            id: crypto.randomUUID(),
            role: "assistant",
            content:
                "Com base no documento analisado:\n\n**Principais riscos**\n1. Penalidade de SLA elevada (cláusula 9.3)\n2. Multa rescisória de 20% (cláusula 4.2)\n3. Revisão trimestral de preços sem teto definido\n\n**Penalidade de SLA:** 5% da mensalidade por incidente que violar o acordo de 99,5% de disponibilidade.",
            timestamp: ts(1740000),
        },
        {
            id: crypto.randomUUID(),
            role: "user",
            content: "Gere a versão final para envio ao jurídico.",
            attachments: [
                {
                    name: "checklist_homologacao.xlsx",
                    type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    size: 42000,
                },
            ],
            timestamp: ts(600000),
        },
        {
            id: crypto.randomUUID(),
            role: "assistant",
            content:
                "Consolidei a análise com o checklist recebido. O documento final foi estruturado para exportação:",
            documentResponse: DEMO_EXPORT_DOCUMENT,
            timestamp: ts(540000),
        },
    ];
}

export function simulateAssistantReply({ userMessage, attachments, agent, model }) {
    const hasDocument = attachments?.length > 0;
    const lower = userMessage.toLowerCase();

    if (hasDocument && (lower.includes("analise") || lower.includes("analis") || lower.includes("documento"))) {
        return {
            content: `Processei **${attachments[0].name}** com o agente *${agent.name}* (modelo ${model}). Segue a leitura estruturada:`,
            documentResponse: {
                title: `Analise_${attachments[0].name.replace(/\.[^.]+$/, "")}.md`,
                format: "markdown",
                content: DEMO_DOCUMENT_MARKDOWN,
            },
            delayMs: 1800,
        };
    }

    if (lower.includes("export") || lower.includes("pdf") || lower.includes("doc")) {
        return {
            content: "Documento consolidado e pronto para exportação:",
            documentResponse: DEMO_EXPORT_DOCUMENT,
            delayMs: 1500,
        };
    }

    if (lower.includes("resumo") || lower.includes("summary")) {
        return {
            content:
                `**Resumo simulado** (${agent.name} · ${model})\n\n` +
                "Esta é uma resposta simulada. Em produção, o backend Node.js encaminharia sua mensagem ao LLM configurado, mantendo as chaves de API no servidor.\n\n" +
                `- Contexto do agente: ${agent.systemPrompt.slice(0, 120)}…`,
            delayMs: 1200,
        };
    }

    return {
        content:
            `**Resposta simulada** · ${agent.name} · ${model}\n\n` +
            "Recebi sua mensagem. Como não há um modelo LLM conectado neste ambiente, esta resposta é gerada localmente para demonstração da interface.\n\n" +
            `> ${userMessage}\n\n` +
            "Você pode usar **Gerar conversa simulada** para ver um fluxo completo com upload e retorno de documentos.",
        delayMs: 1000,
    };
}

export function buildDemoSessionTitle(agentId) {
    if (agentId === "doc-analyst") return "Análise — Contrato Alpha";
    if (agentId === "legal-assistant") return "Revisão — Cláusulas LGPD";
    if (agentId === "copywriter") return "Campanha — Release Q2";
    return "Conversa simulada WOOPI AI";
}
