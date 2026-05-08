/**
 * Formats a decimal value for display without scientific notation.
 *
 * - When the value is a string (the API returns unit values as strings so that
 *   trailing zeros stored in the database are preserved, e.g. "0.000000790"),
 *   the string is returned as-is.
 * - When the value is a JavaScript number (e.g. a calculated total), it is
 *   formatted with up to `maxDecimalPlaces` places, trimming trailing zeros.
 * - NaN and null/undefined return "0" so templates never show "NaN".
 *
 * @param {number|string|null|undefined} value
 * @param {number} maxDecimalPlaces  max digits for number formatting (default 9)
 * @returns {string}
 */
export function formatDecimalValue(value, maxDecimalPlaces = 9) {
    if (value == null || value === "") return "0";
    if (typeof value === "string") {
        return value.trim() || "0";
    }
    const num = Number(value);
    if (isNaN(num)) return "0";
    if (num === 0) return "0";
    return num.toFixed(maxDecimalPlaces).replace(/\.?0+$/, "");
}
