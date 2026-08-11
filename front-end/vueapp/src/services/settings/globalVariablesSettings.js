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

export function findCurrentGlobalVariableUser() {
    const vuex = readJson("vuex", null);
    const project = readJson("project", null);
    return vuex?.userProfile?.login || vuex?.login || project?.login || project?.email || "sistema";
}

export function loadGlobalVariables() {
    const variables = readJson(storageKey(), []);
    if (!Array.isArray(variables)) return [];

    const currentUser = findCurrentGlobalVariableUser();
    let hasLegacyEntries = false;
    const normalizedVariables = variables.map((variable) => {
        if (variable.createdBy) return variable;
        hasLegacyEntries = true;
        return {
            ...variable,
            createdBy: currentUser,
            createdAt: variable.updatedAt || new Date().toISOString(),
        };
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

export function saveGlobalVariable({ id, name, value, description }) {
    const variables = loadGlobalVariables();
    const existingVariable = variables.find((variable) => variable.id === id);
    if (existingVariable && !canEditGlobalVariable(existingVariable)) return null;

    const currentUser = findCurrentGlobalVariableUser();
    const entry = {
        id: id || crypto.randomUUID(),
        name: name.trim(),
        value,
        description: description.trim(),
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