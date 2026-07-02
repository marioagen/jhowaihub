import {

    DEFAULT_AGENTS,

    STORAGE_KEYS,

} from "@/services/chat/chatConstants";

import {

    loadLlmModelsSettingsFromApi,

    saveLlmModelsSettingsToApi,

} from "@/services/settings/llmModelsSettings";



function readJson(key, fallback) {

    try {

        const raw = localStorage.getItem(key);

        if (!raw) return fallback;

        return JSON.parse(raw);

    } catch {

        return fallback;

    }

}



function writeJson(key, value) {

    localStorage.setItem(key, JSON.stringify(value));

}



export function loadAgents() {

    const stored = readJson(STORAGE_KEYS.agents, null);

    if (!stored?.length) {

        writeJson(STORAGE_KEYS.agents, DEFAULT_AGENTS);

        return [...DEFAULT_AGENTS];

    }

    return stored;

}



export function saveAgents(agents) {

    writeJson(STORAGE_KEYS.agents, agents);

}



export async function loadSettings() {

    return loadLlmModelsSettingsFromApi();

}



export async function saveSettings(settings) {

    return saveLlmModelsSettingsToApi(settings);

}



export function loadSessions() {

    return readJson(STORAGE_KEYS.sessions, []);

}



export function saveSessions(sessions) {

    writeJson(STORAGE_KEYS.sessions, sessions);

}



export function loadActiveSessionId() {

    return localStorage.getItem(STORAGE_KEYS.activeSession) || null;

}



export function saveActiveSessionId(sessionId) {

    if (sessionId) {

        localStorage.setItem(STORAGE_KEYS.activeSession, sessionId);

    } else {

        localStorage.removeItem(STORAGE_KEYS.activeSession);

    }

}



export function createSession({ agentId, model, title }) {

    const now = new Date().toISOString();

    return {

        id: crypto.randomUUID(),

        title: title || "Nova conversa",

        agentId,

        model,

        messages: [],

        createdAt: now,

        updatedAt: now,

    };

}


