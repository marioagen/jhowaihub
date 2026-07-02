import api from "@/services/api";
import { DEFAULT_LLM_MODELS } from "@/services/settings/llmModelsConstants";

function normalizeModels(models = {}) {
    return { ...DEFAULT_LLM_MODELS, ...models };
}

function normalizeAvailableModels(availableModels = []) {
    return availableModels.map((model) => ({
        id: model.id,
        label: model.label || model.id,
    }));
}

export async function fetchLlmModelsSettings() {
    const { data } = await api.get("/Settings/llm-models");
    return {
        models: normalizeModels(data?.models),
        availableModels: normalizeAvailableModels(data?.availableModels),
        canEdit: Boolean(data?.canEdit),
    };
}

export async function updateLlmModelsSettings(models) {
    const { data } = await api.put("/Settings/llm-models", {
        models: normalizeModels(models),
    });
    return {
        models: normalizeModels(data?.models),
        availableModels: normalizeAvailableModels(data?.availableModels),
        canEdit: Boolean(data?.canEdit),
    };
}
