export function translateIfExists(te, t, key) {
    if (key == null || key === "") {
        return "";
    }
    const normalized = String(key);
    if (te(normalized)) {
        return t(normalized);
    }
    return normalized;
}

export function permissionGroupI18nKey(groupValue) {
    if (!groupValue) {
        return "";
    }
    let slug = String(groupValue);
    if (slug.startsWith("permissions.groups.")) {
        slug = slug.slice("permissions.groups.".length);
    }
    slug = slug.toLowerCase().replace(/-/g, "");
    if (slug === "team") {
        slug = "teams";
    }
    return `permissions.groups.${slug}`;
}
