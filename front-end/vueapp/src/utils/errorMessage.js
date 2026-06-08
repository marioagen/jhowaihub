export function resolveErrorMessageKey(error) {
    if (typeof error === "string") {
        return error;
    }

    const labelKey = error?.response?.data?.labelError;
    if (labelKey && typeof labelKey === "string") {
        return labelKey;
    }

    return "unexpectedError";
}
