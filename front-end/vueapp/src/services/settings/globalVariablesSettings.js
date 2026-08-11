const STORAGE_KEY_PREFIX = "woopi-global-variables";

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

export function loadGlobalVariables() {
    const variables = readJson(storageKey(), []);
    return Array.isArray(variables) ? variables : [];
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

export function saveGlobalVariable({ id, name, value, description }) {
    const variables = loadGlobalVariables();
    const entry = {
        id: id || crypto.randomUUID(),
        name: name.trim(),
        value,
        description: description.trim(),
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