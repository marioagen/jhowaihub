/**
 * Builds and triggers a browser download for a CSV file.
 *
 * Prepends a UTF-8 BOM (\uFEFF) so that Excel and other readers correctly
 * interpret accents, emojis and JSON special characters.
 *
 * @param {Object[]} rows     - Array of data objects.
 * @param {Array<{key: string, header: string}>} columns
 *   - Column definitions: `key` maps to the object property, `header` is the CSV column name.
 * @param {string} filename   - Desired filename for the downloaded file (without extension).
 */
export function downloadCsv(rows, columns, filename) {
    const escape = (value) => {
        const str = value === null || value === undefined ? "" : String(value);
        // Wrap in double-quotes and escape any inner double-quotes
        return `"${str.replace(/"/g, '""')}"`;
    };

    const header = columns.map((c) => escape(c.header)).join(",");
    const body = rows
        .map((row) => columns.map((c) => escape(row[c.key])).join(","))
        .join("\r\n");

    // UTF-8 BOM + header + rows
    const csv = "\uFEFF" + header + "\r\n" + body;

    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);

    const link = document.createElement("a");
    link.setAttribute("href", url);
    link.setAttribute("download", `${filename}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}
