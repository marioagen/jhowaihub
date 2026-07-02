export const STORAGE_KEY_PREFIX = "woopi-llm-models";
export const LEGACY_CHAT_SETTINGS_KEY = "woopi-chat-settings";

export const DEFAULT_MODELS = [
    { id: "gpt-4o", label: "GPT-4o" },
    { id: "gpt-4.1", label: "GPT-4.1" },
    { id: "gemini-2.5-pro", label: "Gemini 2.5 Pro" },
    { id: "gemini-flash-latest", label: "Gemini Flash" },
    { id: "deepseek-r1", label: "DeepSeek R1" },
    { id: "claude-sonnet", label: "Claude Sonnet" },
];

export const DEFAULT_LLM_MODELS = {
    agents: "gpt-4o",
    questionnaires: "gpt-4o",
    mcp: "deepseek-r1",
    documents: "gemini-2.5-pro",
    chat: "gpt-4o",
};

/** Escopos principais exibidos na tela de configurações (sem chat e sem MCP). */
export const PLATFORM_LLM_SCOPES = [
    { key: "agents", icon: "Bot", hasAdvancedMcp: true },
    { key: "questionnaires", icon: "ClipboardList" },
    { key: "documents", icon: "FileSearch" },
];

export const MCP_ADVANCED_SCOPE = { key: "mcp", icon: "Plug" };

/**
 * Tags por modelo conhecido — alinhadas ao perfil real de cada LLM na plataforma.
 * - gpt-4o: multimodal equilibrado, boa latência
 * - gpt-4.1: sucessor com foco em precisão
 * - gemini-2.5-pro: contexto longo, ideal para documentos extensos
 * - gemini-flash-latest: variante rápida e econômica
 * - deepseek-r1: raciocínio em cadeia
 * - claude-sonnet: escrita e análise precisa
 */
export const MODEL_TAGS_BY_ID = {
    "gpt-4o": ["balanced", "fast"],
    "gpt-4.1": ["precise", "balanced"],
    "gemini-2.5-pro": ["longContext", "precise"],
    "gemini-flash-latest": ["fast", "economical"],
    "deepseek-r1": ["reasoning", "precise"],
    "claude-sonnet": ["precise", "balanced"],
};

export const MODEL_TAG_TONES = {
    balanced: "neutral",
    precise: "primary",
    fast: "success",
    economical: "warning",
    longContext: "info",
    reasoning: "accent",
    toolCalling: "accent",
};

export function findModelTags(modelId) {
    if (!modelId) return [];

    const exact = MODEL_TAGS_BY_ID[modelId];
    if (exact) return exact;

    const lower = modelId.toLowerCase();
    if (lower.includes("flash")) return ["fast", "economical"];
    if (lower.includes("deepseek") && lower.includes("r1")) return ["reasoning", "precise"];
    if (lower.includes("deepseek")) return ["economical", "toolCalling"];
    if (lower.includes("claude")) return ["precise", "balanced"];
    if (lower.includes("gemini") && lower.includes("pro")) return ["longContext", "precise"];
    if (lower.includes("gemini")) return ["fast", "economical"];
    if (lower.includes("gpt-4.1")) return ["precise", "balanced"];
    if (lower.includes("gpt-4o") || lower.includes("gpt-4")) return ["balanced", "fast"];

    return [];
}
