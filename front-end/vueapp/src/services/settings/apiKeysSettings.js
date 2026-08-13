import { isMockMode, MOCK_USER_EMAIL } from "@/mock/mockConfig.js";

export const STORAGE_KEY_PREFIX = "woopi-api-keys";
const AUDIT_KEY_PREFIX = "woopi-api-keys-audit";
export const API_KEY_STATUS = {
    ACTIVE: "active",
    REVOKED: "revoked",
};

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
    const visibleLength = Math.min(12, value.length);
    if (value.length <= visibleLength) return value;
    const visible = value.slice(0, visibleLength);
    const hiddenLength = Math.max(10, value.length - visibleLength);
    return `${visible}${"*".repeat(hiddenLength)}`;
}

function buildMockApiKeys() {
    return [
        {
            id: "mock-key-1",
            name: "4194165+",
            value: "qa_yA1bvTxD8f6g7h8i9j0k1l2m3",
            status: API_KEY_STATUS.ACTIVE,
            createdBy: "Rafael Naoyuki",
            createdAt: "2026-08-05T10:00:00.000Z",
            lastUsedAt: "2026-08-12T14:30:00.000Z",
        },
        {
            id: "mock-key-2",
            name: "API",
            value: "qa_mN3pQ7rS9t1u5v9w0x4y8z2a6",
            status: API_KEY_STATUS.ACTIVE,
            createdBy: "Rafael Naoyuki",
            createdAt: "2026-08-05T11:15:00.000Z",
            lastUsedAt: "2026-08-11T09:20:00.000Z",
        },
        {
            id: "mock-key-3",
            name: "external test",
            value: "qa_kL8jH6gF4dS2aQ9wE7rT5yU3",
            status: API_KEY_STATUS.ACTIVE,
            createdBy: "Luis",
            createdAt: "2026-08-06T08:45:00.000Z",
            lastUsedAt: "2026-08-10T16:00:00.000Z",
        },
        {
            id: "mock-key-4",
            name: "Luis 5",
            value: "qa_pO9iU7yT5rE3wQ1aS2dF4gH6",
            status: API_KEY_STATUS.ACTIVE,
            createdBy: "Luis",
            createdAt: "2026-08-07T13:20:00.000Z",
            lastUsedAt: "2026-08-09T11:45:00.000Z",
        },
        {
            id: "mock-key-5",
            name: "Integração ERP",
            value: "erp_7k9m2x4p8q1w3e5r6t0y2u4i",
            status: API_KEY_STATUS.ACTIVE,
            createdBy: MOCK_USER_EMAIL,
            createdAt: "2026-03-12T10:24:00.000Z",
            lastUsedAt: "2026-08-08T07:30:00.000Z",
        },
        {
            id: "mock-key-6",
            name: "Webhook Parceiro",
            value: "whk_a1b2c3d4e5f6g7h8i9j0k1l2",
            status: API_KEY_STATUS.ACTIVE,
            createdBy: "ana.silva@prototype.local",
            createdAt: "2026-04-08T14:10:00.000Z",
            lastUsedAt: "2026-08-07T18:15:00.000Z",
        },
        {
            id: "mock-key-7",
            name: "App Mobile Homologação",
            value: "mob_n8p3q7r2s6t1u5v9w0x4y8z2",
            status: API_KEY_STATUS.ACTIVE,
            createdBy: MOCK_USER_EMAIL,
            createdAt: "2026-05-21T09:02:00.000Z",
            lastUsedAt: "2026-08-06T12:00:00.000Z",
        },
        {
            id: "mock-key-8",
            name: "Luis 3",
            value: "qa_zX9cV7bN5mK3jH1gF2dS4aQ6",
            status: API_KEY_STATUS.ACTIVE,
            createdBy: "Luis",
            createdAt: "2026-08-04T15:30:00.000Z",
            lastUsedAt: "2026-08-05T10:00:00.000Z",
        },
        {
            id: "mock-key-9",
            name: "Token Legado",
            value: "lgc_z9y8x7w6v5u4t3s2r1q0p9o8",
            status: API_KEY_STATUS.REVOKED,
            createdBy: "Celso",
            createdAt: "2025-11-03T18:45:00.000Z",
            lastUsedAt: "2026-02-10T08:00:00.000Z",
            revokedAt: "2026-02-14T16:20:00.000Z",
            revokedBy: MOCK_USER_EMAIL,
        },
        {
            id: "mock-key-10",
            name: "Sandbox QA",
            value: "sbx_c4d5e6f7g8h9i0j1k2l3m4n5",
            status: API_KEY_STATUS.REVOKED,
            createdBy: "carla.mendes@prototype.local",
            createdAt: "2026-01-19T11:30:00.000Z",
            lastUsedAt: "2026-05-28T14:20:00.000Z",
            revokedAt: "2026-06-02T09:15:00.000Z",
            revokedBy: "ana.silva@prototype.local",
        },
        {
            id: "mock-key-11",
            name: "Homologação N8N",
            value: "qa_bC4dE6fG8hI0jK2lM4nO6pQ8",
            status: API_KEY_STATUS.REVOKED,
            createdBy: "Rafael Naoyuki",
            createdAt: "2026-06-15T09:00:00.000Z",
            lastUsedAt: "2026-07-20T11:30:00.000Z",
            revokedAt: "2026-07-25T16:00:00.000Z",
            revokedBy: "Rafael Naoyuki",
        },
        {
            id: "mock-key-12",
            name: "Backup Integração",
            value: "qa_rS2tU4vW6xY8zA0bC2dE4fG6",
            status: API_KEY_STATUS.REVOKED,
            createdBy: "Celso",
            createdAt: "2026-02-01T10:00:00.000Z",
            lastUsedAt: "2026-03-15T09:45:00.000Z",
            revokedAt: "2026-03-20T12:00:00.000Z",
            revokedBy: "Celso",
        },
    ];
}

function normalizeApiKey(key) {
    return {
        ...key,
        status: key.status === API_KEY_STATUS.REVOKED ? API_KEY_STATUS.REVOKED : API_KEY_STATUS.ACTIVE,
    };
}

export function loadApiKeys() {
    const stored = readJson(storageKey(), null);
    const mockKeys = buildMockApiKeys();
    const shouldSeedMockKeys =
        isMockMode() && (!Array.isArray(stored) || stored.length < mockKeys.length);
    if (shouldSeedMockKeys) {
        writeJson(storageKey(), mockKeys);
        return mockKeys.map(normalizeApiKey);
    }
    if (stored === null || !Array.isArray(stored)) {
        writeJson(storageKey(), []);
        return [];
    }
    return stored.map(normalizeApiKey);
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
        status: API_KEY_STATUS.ACTIVE,
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

export function revokeApiKeys(ids) {
    const idSet = new Set(ids);
    const allKeys = loadApiKeys();
    const revokedBy = resolveCurrentUser();
    const revokedAt = new Date().toISOString();
    const revokedNames = [];

    const updated = allKeys.map((key) => {
        if (!idSet.has(key.id) || key.status === API_KEY_STATUS.REVOKED) {
            return key;
        }
        revokedNames.push(key.name);
        return {
            ...key,
            status: API_KEY_STATUS.REVOKED,
            revokedAt,
            revokedBy,
        };
    });

    saveApiKeys(updated);
    for (const name of revokedNames) {
        appendApiKeyAuditEvent(
            "apiKeyRevoked",
            `${revokedBy} revogou a chave de API "${name}"`,
            name,
        );
    }
    return updated;
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
