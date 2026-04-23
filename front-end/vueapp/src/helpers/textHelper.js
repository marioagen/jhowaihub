const TEMPLATE_VAR = "___WOOPI_TPL___";
const MUSTACHE_RE = /^\{\{[^}]+\}\}/;

const tryMustacheLen = (s, i) => {
    const m = s.slice(i).match(MUSTACHE_RE);
    return m ? m[0].length : 0;
};

const isUnquotedValuePosition = (s, startIdx) => {
    let k = startIdx - 1;
    while (k >= 0 && /\s/.test(s[k])) k -= 1;
    if (k < 0) {
        return false;
    }
    const ch = s[k];
    return ch === ":" || ch === "," || ch === "[";
};

const sanitizeForJsonParse = (value) => {
    const s = value;
    let out = "";
    let i = 0;
    let inString = false;
    let escape = false;
    const n = s.length;

    while (i < n) {
        if (inString) {
            if (escape) {
                out += s[i];
                escape = false;
                i += 1;
                continue;
            }
            if (s[i] === "\\") {
                out += s[i];
                escape = true;
                i += 1;
                continue;
            }
            if (s[i] === '"') {
                inString = false;
                out += s[i];
                i += 1;
                continue;
            }
            const mLen = tryMustacheLen(s, i);
            if (mLen) {
                out += TEMPLATE_VAR;
                i += mLen;
                continue;
            }
            out += s[i];
            i += 1;
        } else {
            const mLen = tryMustacheLen(s, i);
            if (mLen) {
                if (isUnquotedValuePosition(s, i)) {
                    out += '"';
                    out += TEMPLATE_VAR;
                    out += '"';
                } else {
                    out += s.slice(i, i + mLen);
                }
                i += mLen;
                continue;
            }
            if (s[i] === '"') {
                inString = true;
                out += s[i];
                i += 1;
            } else {
                out += s[i];
                i += 1;
            }
        }
    }
    return out;
};

export const validateJSON = (value) => {
    if (!value || value.trim() === "") {
        return { isValid: true, error: "" };
    }
    try {
        const sanitizedValue = sanitizeForJsonParse(value);
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
