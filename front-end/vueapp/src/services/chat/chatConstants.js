import { DEFAULT_LLM_MODELS } from "@/services/settings/llmModelsConstants";

export const STORAGE_KEYS = {
    sessions: "woopi-chat-sessions",
    agents: "woopi-chat-agents",
    settings: "woopi-chat-settings",
    activeSession: "woopi-chat-active-session",
};

export {
    DEFAULT_MODELS,
    DEFAULT_LLM_MODELS,
} from "@/services/settings/llmModelsConstants";

export const DEFAULT_AGENTS = [
    {
        id: "doc-analyst",
        name: "Analista de Documentos Senior",
        description: "Especialista em extração, classificação e resumo de documentos corporativos.",
        icon: "FileSearch",
        color: "#0d6efd",
        systemPrompt: "Você é um analista sênior de documentos. Responda com clareza, cite trechos e entregue saídas estruturadas em Markdown.",
        isBuiltin: true,
    },
    {
        id: "copywriter",
        name: "Copywriter",
        description: "Cria textos persuasivos, e-mails e comunicações alinhadas à marca.",
        icon: "PenLine",
        color: "#8b5cf6",
        systemPrompt: "Você é um copywriter experiente. Adapte tom e formato ao público-alvo.",
        isBuiltin: true,
    },
    {
        id: "legal-assistant",
        name: "Assistente Jurídico",
        description: "Apoia revisão de contratos, cláusulas e conformidade regulatória.",
        icon: "Scale",
        color: "#06b6d4",
        systemPrompt: "Você é um assistente jurídico. Destaque riscos, prazos e obrigações sem substituir parecer legal formal.",
        isBuiltin: true,
    },
    {
        id: "general",
        name: "Assistente Geral",
        description: "Conversação aberta para tarefas diversas da plataforma WOOPI AI.",
        icon: "Bot",
        color: "#40b04d",
        systemPrompt: "Você é o assistente central WOOPI AI. Seja objetivo, profissional e útil.",
        isBuiltin: true,
    },
];

export const DEFAULT_SETTINGS = {
    models: { ...DEFAULT_LLM_MODELS },
};
