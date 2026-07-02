import { DEFAULT_LLM_MODELS } from "@/services/settings/llmModelsConstants";
import { fetchLlmModelsSettings, updateLlmModelsSettings } from "@/services/settings/llmModelsApi";

export function loadLlmModelsSettings() {
    return {
        models: { ...DEFAULT_LLM_MODELS },
    };
}

export async function loadLlmModelsSettingsFromApi() {
    return fetchLlmModelsSettings();
}

export function saveLlmModelsSettings(settings) {
    return settings;
}

export async function saveLlmModelsSettingsToApi(settings) {
    return updateLlmModelsSettings(settings.models);
}

export function resetLlmModelsSettings() {
    return {
        models: { ...DEFAULT_LLM_MODELS },
    };
}
