export const STORAGE_KEY_PREFIX = "woopi-api-keys";
const AUDIT_KEY_PREFIX = "woopi-api-keys-audit";

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

function auditStorageKey() {
    return `${AUDIT_KEY_PREFIX}-${resolveTenantName()}`;
}

function resolveCurrentUser() {
    try {
        const raw = localStorage.getItem("vuex");
        if (raw) {
            const state = JSON.parse(raw);
            const login = state?.userProfile?.login || state?.login;
            if (login) return login;
        }
    } catch { /* ignore */ }
    try {
        const project = readJson("project", null);
        return project?.login || project?.email || "sistema";
    } catch {
        return "sistema";
    }
}

export function loadApiKeyAuditLog() {
    return readJson(auditStorageKey(), []);
}

function appendApiKeyAuditEvent(eventType, detail, keyName) {
    const log = loadApiKeyAuditLog();
    log.unshift({
        eventId: crypto.randomUUID(),
        eventType,
        userName: resolveCurrentUser(),
        detail,
        keyName,
        createdAt: new Date().toISOString(),
        endpoint: null,
        method: null,
        statusCode: null,
        ipAddress: null,
    });
    writeJson(auditStorageKey(), log.slice(0, 500));
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
    const createdBy = resolveCurrentUser();
    const entry = {
        id: crypto.randomUUID(),
        name: name.trim(),
        value,
        createdAt: new Date().toISOString(),
        createdBy,
    };
    keys.unshift(entry);
    saveApiKeys(keys);
    appendApiKeyAuditEvent(
        "apiKeyCreated",
        `${createdBy} criou a chave de API "${entry.name}"`,
        entry.name,
    );
    return entry;
}

export function deleteApiKeys(ids) {
    const idSet = new Set(ids);
    const allKeys = loadApiKeys();
    const deletedBy = resolveCurrentUser();
    const deletedNames = allKeys
        .filter((k) => idSet.has(k.id))
        .map((k) => k.name);
    const remaining = allKeys.filter((key) => !idSet.has(key.id));
    saveApiKeys(remaining);
    for (const name of deletedNames) {
        appendApiKeyAuditEvent(
            "apiKeyDeleted",
            `${deletedBy} excluiu a chave de API "${name}"`,
            name,
        );
    }
    return remaining;
}
