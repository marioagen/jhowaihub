import { isMockMode, MOCK_USER_NAME } from "@/mock/mockConfig.js";

const STORAGE_KEY_PREFIX = "woopi-global-variables";
export const GLOBAL_VARIABLE_TYPES = Object.freeze({ Common: "common", Environment: "environment" });
export const GLOBAL_VARIABLE_CONTEXTS = Object.freeze({
    Url: "url",
    Query: "query",
    Header: "header",
    Body: "body",
    Credential: "credential",
});

const SECRET_CONTEXTS = new Set([
    GLOBAL_VARIABLE_CONTEXTS.Header,
    GLOBAL_VARIABLE_CONTEXTS.Body,
    GLOBAL_VARIABLE_CONTEXTS.Credential,
]);

function createMockGlobalVariables(currentUser) {
    const mockDefinitions = [
        ["COMPANY_NAME", "Woopi AI", "Nome da empresa usado em comunicações", currentUser, "common", true],
        ["SUPPORT_EMAIL", "suporte@woopiai.com", "Canal de atendimento ao cliente", currentUser, "common", true],
        ["LEGAL_NAME", "Woopi Tecnologia Ltda.", "Razão social para documentos", currentUser, "common", false],
        ["DEFAULT_LANGUAGE", "pt-BR", "Idioma padrão das respostas", currentUser, "common", true],
        ["CONTRACT_YEAR", "2026", "Ano de referência dos contratos", currentUser, "common", true],
        ["PRIVACY_URL", "https://woopiai.com/privacidade", "Política de privacidade institucional", currentUser, "common", true],
        ["N8N_API_KEY", "{{global:SUPPORT_EMAIL}}", "Chave de integração do conector", currentUser, "environment", true],
        ["FINANCE_EMAIL", "financeiro@woopiai.com", "Contato do departamento financeiro", "ana.souza@woopiai.com", "common", true],
        ["API_BEARER_TOKEN", "{{global:COMPANY_NAME}}", "Token usado em APIs externas", "ana.souza@woopiai.com", "environment", true],
        ["TIMEZONE", "America/Sao_Paulo", "Fuso horário padrão do tenant", "carlos.lima@woopiai.com", "common", true],
        ["COMPLIANCE_OFFICER", "Marina Costa", "Responsável por conformidade", "marina.costa@woopiai.com", "common", false],
        ["WEBHOOK_SECRET", "{{global:DEFAULT_LANGUAGE}}", "Segredo para validação de webhooks", "marina.costa@woopiai.com", "environment", true],
    ];

    return mockDefinitions.map(([name, value, description, createdBy, valueType, availableAsEnvironment], index) => ({
        id: `mock-global-variable-${index + 1}`,
        name,
        value,
        description,
        createdBy,
        valueType,
        availableAsEnvironment,
        createdAt: new Date(Date.UTC(2026, 6, index + 1)).toISOString(),
        updatedAt: new Date(Date.UTC(2026, 7, index + 1)).toISOString(),
    }));
}

function readJson(key, fallback) {
    try {
        const value = localStorage.getItem(key);
        return value ? JSON.parse(value) : fallback;
    } catch {
        return fallback;
    }
}

function resolveTenantName() {
    const project = readJson("project", null);
    return project?.tenant || project?.amount || "default";
}

function storageKey() {
    return `${STORAGE_KEY_PREFIX}-${resolveTenantName()}`;
}

function saveGlobalVariables(variables) {
    localStorage.setItem(storageKey(), JSON.stringify(variables));
    return variables;
}

export function findCurrentGlobalVariableUser() {
    const vuex = readJson("vuex", null);
    const project = readJson("project", null);
    return vuex?.userProfile?.name
        || vuex?.userProfile?.login
        || vuex?.login
        || project?.login
        || project?.email
        || (isMockMode() ? MOCK_USER_NAME : "sistema");
}

export function loadGlobalVariables() {
    const currentUser = findCurrentGlobalVariableUser();
    const mockVariables = createMockGlobalVariables(currentUser);
    const mockVariablesByName = new Map(mockVariables.map((variable) => [variable.name, variable]));
    const storedVariables = readJson(storageKey(), null);
    const variables = Array.isArray(storedVariables) ? storedVariables : [];
    if (!Array.isArray(variables)) return [];

    if (variables.length < 12) {
        const existingNames = new Set(variables.map((variable) => variable.name));
        const missingMocks = mockVariables.filter(
            (variable) => !existingNames.has(variable.name),
        );
        variables.push(...missingMocks.slice(0, 12 - variables.length));
        saveGlobalVariables(variables);
    }

    let hasLegacyEntries = false;
    const normalizedVariables = variables.map((variable) => {
        const mockVariable = mockVariablesByName.get(variable.name);
        const normalizedVariable = {
            ...variable,
            createdBy: variable.createdBy === "sistema" ? currentUser : variable.createdBy || currentUser,
            createdAt: variable.createdAt || variable.updatedAt || new Date().toISOString(),
            valueType:
                variable.valueType === "secret"
                    ? GLOBAL_VARIABLE_TYPES.Environment
                    : variable.valueType || mockVariable?.valueType || GLOBAL_VARIABLE_TYPES.Common,
            value:
                variable.valueType === "secret" && mockVariable
                    ? mockVariable.value
                    : variable.value,
            availableAsEnvironment: true,
        };
        if (JSON.stringify(normalizedVariable) !== JSON.stringify(variable)) hasLegacyEntries = true;
        return normalizedVariable;
    });

    if (hasLegacyEntries) saveGlobalVariables(normalizedVariables);
    return normalizedVariables;
}

export function isValidGlobalVariableName(name) {
    return /^[A-Za-z][A-Za-z0-9_]*$/.test(name.trim());
}

export function globalVariableNameExists(name, ignoredId = null) {
    const normalizedName = name.trim().toLowerCase();
    return loadGlobalVariables().some(
        (variable) => variable.id !== ignoredId && variable.name.toLowerCase() === normalizedName,
    );
}

export function canEditGlobalVariable(variable) {
    return variable?.createdBy === findCurrentGlobalVariableUser();
}

export function findCommonGlobalVariables(ignoredId = null) {
    return loadGlobalVariables().filter(
        (variable) => variable.id !== ignoredId && variable.valueType === GLOBAL_VARIABLE_TYPES.Common,
    );
}

export function findAvailableGlobalVariables(context) {
    return loadGlobalVariables().filter((variable) => {
        if (!variable.availableAsEnvironment) return false;
        return variable.valueType !== GLOBAL_VARIABLE_TYPES.Environment || SECRET_CONTEXTS.has(context);
    });
}

export function resolveGlobalVariables(input, context) {
    const variables = new Map(findAvailableGlobalVariables(context).map((variable) => [variable.name, variable]));
    const missingVariables = new Set();
    let containsSecret = false;
    const value = String(input || "").replace(/\{\{global:([A-Za-z][A-Za-z0-9_]*)\}\}/g, (placeholder, name) => {
        const variable = variables.get(name);
        if (!variable) {
            missingVariables.add(name);
            return placeholder;
        }
        containsSecret ||= variable.valueType === GLOBAL_VARIABLE_TYPES.Environment;
        return variable.value;
    });
    return { value, missingVariables: [...missingVariables], containsSecret };
}

export function saveGlobalVariable({ id, name, value, description, valueType }) {
    const variables = loadGlobalVariables();
    const existingVariable = variables.find((variable) => variable.id === id);
    if (existingVariable && !canEditGlobalVariable(existingVariable)) return null;

    const currentUser = findCurrentGlobalVariableUser();
    const entry = {
        id: id || crypto.randomUUID(),
        name: name.trim(),
        value,
        description: description.trim(),
        valueType: valueType || GLOBAL_VARIABLE_TYPES.Common,
        availableAsEnvironment: true,
        createdBy: existingVariable?.createdBy || currentUser,
        createdAt: existingVariable?.createdAt || new Date().toISOString(),
        updatedAt: new Date().toISOString(),
    };
    const existingIndex = variables.findIndex((variable) => variable.id === id);

    if (existingIndex >= 0) variables.splice(existingIndex, 1, entry);
    else variables.push(entry);

    saveGlobalVariables(variables);
    return entry;
}

export function deleteGlobalVariable(id) {
    return saveGlobalVariables(loadGlobalVariables().filter((variable) => variable.id !== id));
}