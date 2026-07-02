export const STORAGE_KEY_PREFIX = "woopi-api-keys";

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

function resolveTenantName() {
    try {
        const project = readJson("project", null);
        return project?.tenant || project?.amount || "default";
    } catch {
        return "default";
    }
}

function storageKey() {
    return `${STORAGE_KEY_PREFIX}-${resolveTenantName()}`;
}

export function generateApiKeyValue() {
    const chars = "abcdefghijklmnopqrstuvwxyz0123456789";
    let value = "";
    for (let i = 0; i < 43; i += 1) {
        value += chars[Math.floor(Math.random() * chars.length)];
    }
    return value;
}

export function maskApiKeyValue(value) {
    if (!value) return "";
    if (value.length <= 3) return value;
    const visible = value.slice(0, 3);
    const hiddenLength = Math.max(12, value.length - 3);
    return `${visible}${"*".repeat(hiddenLength)}`;
}

export function loadApiKeys() {
    const stored = readJson(storageKey(), null);
    if (!Array.isArray(stored)) {
        writeJson(storageKey(), []);
        return [];
    }
    return stored;
}

export function saveApiKeys(keys) {
    writeJson(storageKey(), keys);
    return keys;
}

export function createApiKey({ name, value }) {
    const keys = loadApiKeys();
    const entry = {
        id: crypto.randomUUID(),
        name: name.trim(),
        value,
        createdAt: new Date().toISOString(),
    };
    keys.unshift(entry);
    saveApiKeys(keys);
    return entry;
}

export function deleteApiKeys(ids) {
    const idSet = new Set(ids);
    const keys = loadApiKeys().filter((key) => !idSet.has(key.id));
    saveApiKeys(keys);
    return keys;
}
