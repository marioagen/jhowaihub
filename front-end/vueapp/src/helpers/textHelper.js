export const validateJSON = (value) => {
    if (!value || value.trim() === "") {
        return { isValid: true, error: "" };
    }
    try {
        const sanitizedValue = value.replace(/\{\{[^}]+\}\}/g, '"PLACEHOLDER"');
        JSON.parse(sanitizedValue);
        return { isValid: true, error: "" };
    } catch (e) {
        return { isValid: false, error: e.message || this.$t("template.invalidJsonFormat") };
    }
};

export const sanitizeToolNameForVariable = (name) => {
    if (!name) return "";
    return name
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .toLowerCase()
        .replace(/[^a-z0-9_]+/g, "_")
        .replace(/^_+|_+$/g, "");
};

export default {
    validateJSON,
    sanitizeToolNameForVariable,
};
