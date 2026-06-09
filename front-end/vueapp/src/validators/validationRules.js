import { defineRule } from "vee-validate";
import { required } from "@vee-validate/rules";
import i18n from "@/locales/i18n";
import textHelper from "@/helpers/textHelper";

defineRule("required", (value) => {
    if (required(value)) return true;
    return i18n.global.t("validation.required");
});

defineRule("email", (value) => {
    if (!value || !value.length) {
        return true;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(value)) {
        return i18n.global.t("validation.email");
    }
    return true;
});

defineRule("custom_password", (value) => {
    if (!value || !value.length) {
        return true;
    }
    if (value.length < 6) {
        return i18n.global.t("validation.password_min");
    }
    if (!/[a-z]/.test(value)) {
        return i18n.global.t("validation.password_lowercase");
    }
    if (!/[A-Z]/.test(value)) {
        return i18n.global.t("validation.password_uppercase");
    }
    if (!/[0-9]/.test(value)) {
        return i18n.global.t("validation.password_number");
    }
    if (!/[^A-Za-z0-9]/.test(value)) {
        return i18n.global.t("validation.password_special");
    }
    return true;
});

defineRule("max", (value, [limit]) => {
    if (!value || !value.length) {
        return true;
    }
    if (value.length > limit) {
        return i18n.global.t("validation.max", { limit: limit });
    }
    return true;
});

defineRule("min", (value, [limit]) => {
    if (!value || !value.length) {
        return true;
    }
    if (value.length < limit) {
        return i18n.global.t("validation.min", { limit: limit });
    }
    return true;
});

defineRule("confirmed", (value, [target], ctx) => {
    const targetValue = ctx.form[target];
    if (value !== targetValue) {
        return i18n.global.t("validation.password_confirmed");
    }
    return true;
});

defineRule("requiredArray", (value) => {
    if (Array.isArray(value) && value.length > 0) {
        return true;
    }
    return i18n.global.t("validation.oneElementArray");
});

defineRule("jsonValidation", (value) => {
    const result = textHelper.validateJSON(value);
    if (!result.isValid) {
        return i18n.global.t("template.invalidJsonFormat");
    }
    return true;
});
