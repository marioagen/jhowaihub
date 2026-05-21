/**
 * Parses a vue-router param (or similar) into a strict boolean.
 * Handles "true"/"false" strings and avoids NaN from parseInt on boolean route segments.
 */
export function parseRouteBoolean(value) {
    if (value === true || value === "true" || value === 1 || value === "1") {
        return true;
    }
    if (value === false || value === "false" || value === 0 || value === "0") {
        return false;
    }
    if (typeof value === "number" && Number.isNaN(value)) {
        return false;
    }
    return false;
}
