#!/usr/bin/env node
/**
 * Pre-commit hook script to format staged files (frontend only)
 * Works on Windows, Linux, and macOS
 */

const { execSync } = require("child_process");
const path = require("path");
const fs = require("fs");

// Colors for console output
const colors = {
    reset: "\x1b[0m",
    red: "\x1b[31m",
    green: "\x1b[32m",
    yellow: "\x1b[33m",
    blue: "\x1b[34m",
};

function log(message, color = "reset") {
    console.log(`${colors[color]}${message}${colors.reset}`);
}

/**
 * Post-processes Vue files to format component attributes
 * - Removes empty lines between attributes
 * - Ensures buttons and custom components have one attribute per line
 */
function formatVueAttributes(filePath) {
    try {
        const fullPath = path.resolve(filePath);
        if (!fs.existsSync(fullPath)) {
            return false;
        }

        let content = fs.readFileSync(fullPath, "utf-8");
        const originalContent = content;
        const lines = content.split("\n");
        const modifiedLines = [];

        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];
            const trimmedLine = line.trim();

            // Pattern to match button elements with multiple attributes on one line
            const buttonMatch = trimmedLine.match(/^<button\s+([^>]+)(\s*\/?>)$/);
            if (buttonMatch) {
                const attributes = buttonMatch[1];
                const closing = buttonMatch[2];

                // Skip if already formatted (has newlines)
                if (!attributes.includes("\n")) {
                    // Split attributes properly (handling Vue directives and bindings)
                    // Use regex to split on whitespace that's not inside quotes
                    const attrList = [];
                    let currentAttr = "";
                    let inQuotes = false;
                    let quoteChar = null;

                    for (let j = 0; j < attributes.length; j++) {
                        const char = attributes[j];
                        const prevChar = j > 0 ? attributes[j - 1] : null;
                        
                        if ((char === '"' || char === "'") && prevChar !== "\\") {
                            if (!inQuotes) {
                                inQuotes = true;
                                quoteChar = char;
                            } else if (char === quoteChar) {
                                inQuotes = false;
                                quoteChar = null;
                            }
                            currentAttr += char;
                        } else if (!inQuotes && /\s/.test(char)) {
                            // Check if this whitespace separates attributes
                            if (currentAttr.trim()) {
                                // Look ahead to see if next non-whitespace starts a new attribute
                                const remaining = attributes.substring(j).trim();
                                if (remaining.match(/^(@|:|v-|[\w-]+\s*=)/)) {
                                    attrList.push(currentAttr.trim());
                                    currentAttr = "";
                                } else {
                                    currentAttr += char;
                                }
                            }
                        } else {
                            currentAttr += char;
                        }
                    }
                    if (currentAttr.trim()) {
                        attrList.push(currentAttr.trim());
                    }

                    if (attrList.length > 1) {
                        const indent = line.match(/^(\s*)/)?.[1] || "";
                        const formattedAttrs = attrList.map((attr) => `${indent}    ${attr}`).join("\n");
                        modifiedLines.push(`${indent}<button`);
                        modifiedLines.push(formattedAttrs);
                        modifiedLines.push(`${indent}${closing.trim()}`);
                        continue;
                    }
                }
            }

            // Pattern to match custom Vue components (PascalCase)
            const componentMatch = trimmedLine.match(/^<([A-Z][\w-]*)\s+([^/>]+)(\s*\/?>)$/);
            if (componentMatch) {
                const componentName = componentMatch[1];
                const attributes = componentMatch[2];
                const closing = componentMatch[3];

                // Skip native HTML elements
                const nativeElements = ["Button", "Div", "Span", "A", "Li", "Ul", "Ol", "P", "H1", "H2", "H3", "H4", "H5", "H6", "Input", "Form", "Label", "Select", "Option", "Textarea", "Img", "Table", "Thead", "Tbody", "Tr", "Td", "Th"];
                if (!nativeElements.includes(componentName)) {
                    // Skip if already formatted (has newlines)
                    if (!attributes.includes("\n")) {
                        // Split attributes properly (handling Vue directives and bindings)
                        const attrList = [];
                        let currentAttr = "";
                        let inQuotes = false;
                        let quoteChar = null;

                        for (let j = 0; j < attributes.length; j++) {
                            const char = attributes[j];
                            const prevChar = j > 0 ? attributes[j - 1] : null;
                            
                            if ((char === '"' || char === "'") && prevChar !== "\\") {
                                if (!inQuotes) {
                                    inQuotes = true;
                                    quoteChar = char;
                                } else if (char === quoteChar) {
                                    inQuotes = false;
                                    quoteChar = null;
                                }
                                currentAttr += char;
                            } else if (!inQuotes && /\s/.test(char)) {
                                // Check if this whitespace separates attributes
                                if (currentAttr.trim()) {
                                    // Look ahead to see if next non-whitespace starts a new attribute
                                    const remaining = attributes.substring(j).trim();
                                    if (remaining.match(/^(@|:|v-|[\w-]+\s*=)/)) {
                                        attrList.push(currentAttr.trim());
                                        currentAttr = "";
                                    } else {
                                        currentAttr += char;
                                    }
                                }
                            } else {
                                currentAttr += char;
                            }
                        }
                        if (currentAttr.trim()) {
                            attrList.push(currentAttr.trim());
                        }

                        if (attrList.length > 1) {
                            const indent = line.match(/^(\s*)/)?.[1] || "";
                            const formattedAttrs = attrList.map((attr) => `${indent}    ${attr}`).join("\n");
                            modifiedLines.push(`${indent}<${componentName}`);
                            modifiedLines.push(formattedAttrs);
                            modifiedLines.push(`${indent}${closing.trim()}`);
                            continue;
                        }
                    }
                }
            }

            modifiedLines.push(line);
        }

        // Remove empty lines between attributes and other empty lines in Vue files
        const finalLines = [];
        for (let i = 0; i < modifiedLines.length; i++) {
            const line = modifiedLines[i];
            const trimmed = line.trim();
            const isAttribute = trimmed.match(/^(@|:|v-|[\w-]+\s*=)/);
            
            // Skip empty lines between attributes
            if (i > 0 && isAttribute) {
                const prevLine = modifiedLines[i - 1];
                const prevTrimmed = prevLine.trim();
                const prevIsAttribute = prevTrimmed.match(/^(@|:|v-|[\w-]+\s*=)/);
                
                // If previous was an attribute and current line is empty, skip it
                if (prevIsAttribute && trimmed === "") {
                    continue;
                }
            }
            
            // Remove empty lines between template and script/style sections
            if (trimmed === "") {
                const prevLine = i > 0 ? modifiedLines[i - 1].trim() : "";
                const nextLine = i < modifiedLines.length - 1 ? modifiedLines[i + 1].trim() : "";
                
                // Skip empty line if it's between </template> and <script> or <style>
                if (prevLine === "</template>" && (nextLine.startsWith("<script") || nextLine.startsWith("<style"))) {
                    continue;
                }
                
                // Skip empty line if it's between </script> and <style>
                if (prevLine === "</script>" && nextLine.startsWith("<style")) {
                    continue;
                }
                
                // Skip empty line if it's between closing tag and opening tag of different sections
                if ((prevLine === "</template>" || prevLine === "</script>" || prevLine === "</style>") &&
                    (nextLine.startsWith("<script") || nextLine.startsWith("<style") || nextLine.startsWith("<template"))) {
                    continue;
                }
            }
            
            finalLines.push(line);
        }

        const newContent = finalLines.join("\n");
        if (newContent !== originalContent) {
            fs.writeFileSync(fullPath, newContent, "utf-8");
            return true;
        }

        return false;
    } catch (error) {
        log(`Warning: Could not post-process Vue file ${filePath}: ${error.message}`, "yellow");
        return false;
    }
}

function getStagedFiles() {
    try {
        const output = execSync("git diff --cached --name-only --diff-filter=ACM", {
            encoding: "utf-8",
            stdio: "pipe",
        });
        return output
            .split("\n")
            .map((line) => line.trim())
            .filter((line) => line.length > 0);
    } catch (error) {
        log("Error getting staged files", "red");
        return [];
    }
}

/**
 * Determines the file type and category
 * @returns {Object} { type: 'frontend'|'root'|'skip', category: string }
 */
function categorizeFile(filePath) {
    // Normalize path separators
    const normalizedPath = filePath.replace(/\\/g, "/");
    
    // Frontend files - must be in front-end/vueapp/ directory
    const isInFrontendDir = normalizedPath.startsWith("front-end/vueapp/");
    const frontendExtensions = /\.(js|vue|ts|tsx|json|css|scss|html)$/i;
    
    if (isInFrontendDir && frontendExtensions.test(filePath)) {
        return { type: "frontend", category: "frontend", tool: "prettier (frontend)" };
    }
    
    // Root-level formattable files (JSON, YAML, MD) - not in frontend or backend specific dirs
    const rootExtensions = /\.(json|yml|yaml|md)$/i;
    const isInBackendDir = normalizedPath.startsWith("back-end/");
    const isInExternalApiDir = normalizedPath.startsWith("external-api/");
    
    if (!isInFrontendDir && !isInBackendDir && !isInExternalApiDir && rootExtensions.test(filePath)) {
        return { type: "root", category: "root", tool: "prettier (root)" };
    }
    
    // Skip all other files
    return { type: "skip", category: "other", tool: "none" };
}

/**
 * Formats a file (frontend or root files)
 */
function formatFile(filePath) {
    const fullPath = path.resolve(filePath);

    // Check if file exists
    if (!fs.existsSync(fullPath)) {
        return { success: true, skipped: true };
    }

    const fileInfo = categorizeFile(filePath);
    
    // Skip files that shouldn't be formatted
    if (fileInfo.type === "skip") {
        return { success: true, skipped: true };
    }

    try {
        if (fileInfo.type === "frontend") {
            // Format using frontend prettier (uses front-end/vueapp/.prettierrc)
            const frontendDir = path.resolve("front-end/vueapp");
            if (fs.existsSync(path.join(frontendDir, "package.json"))) {
                log(`  Formatting frontend file: ${filePath}`, "blue");
                execSync(`npx prettier --write "${fullPath}"`, {
                    cwd: frontendDir,
                    stdio: "pipe",
                });
                // Stage the file again after formatting
                execSync(`git add "${filePath}"`, { stdio: "pipe" });
                return { success: true };
            } else {
                log(`  Warning: Frontend package.json not found, skipping ${filePath}`, "yellow");
                return { success: true, skipped: true };
            }
        } else if (fileInfo.type === "root") {
            // Format using root prettier (uses .prettierrc)
            try {
                log(`  Formatting root file: ${filePath}`, "blue");
                execSync(`npx prettier --write "${fullPath}"`, {
                    stdio: "pipe",
                    cwd: path.resolve("."),
                });
                // Stage the file again after formatting
                execSync(`git add "${filePath}"`, { stdio: "pipe" });
                return { success: true };
            } catch (error) {
                // Prettier might not be installed at root, that's okay
                log(`  Note: Prettier not available at root, skipping ${filePath}`, "yellow");
                return { success: true, skipped: true };
            }
        }
    } catch (error) {
        return { success: false, error: error.message };
    }

    return { success: true, skipped: true };
}

function main() {
    log("Running pre-commit formatting...", "green");

    const stagedFiles = getStagedFiles();

    if (stagedFiles.length === 0) {
        log("No staged files to format.", "yellow");
        process.exit(0);
    }

    // Categorize all files
    const fileCategories = {
        frontend: [],
        root: [],
        skip: [],
    };

    for (const file of stagedFiles) {
        const fileInfo = categorizeFile(file);
        fileCategories[fileInfo.type].push({ path: file, ...fileInfo });
    }

    // Log file categorization
    if (fileCategories.frontend.length > 0) {
        log(`\nFrontend files (${fileCategories.frontend.length}):`, "blue");
        fileCategories.frontend.forEach((f) => log(`  - ${f.path}`, "blue"));
    }
    if (fileCategories.root.length > 0) {
        log(`\nRoot files (${fileCategories.root.length}):`, "blue");
        fileCategories.root.forEach((f) => log(`  - ${f.path}`, "blue"));
    }
    if (fileCategories.skip.length > 0) {
        log(`\nSkipped files (${fileCategories.skip.length}):`, "yellow");
        fileCategories.skip.forEach((f) => log(`  - ${f.path}`, "yellow"));
    }

    let formattedCount = 0;
    let errorCount = 0;
    let skippedCount = fileCategories.skip.length;

    // Format frontend files
    if (fileCategories.frontend.length > 0) {
        log(`\n[FRONTEND] Formatting ${fileCategories.frontend.length} file(s) using prettier (frontend config)...`, "blue");
        for (const fileInfo of fileCategories.frontend) {
            const result = formatFile(fileInfo.path);
            if (result.skipped) {
                skippedCount++;
            } else if (result.success) {
                // Post-process Vue files to format component attributes
                if (fileInfo.path.endsWith(".vue")) {
                    try {
                        if (formatVueAttributes(fileInfo.path)) {
                            log(`  Post-processed Vue attributes: ${fileInfo.path}`, "blue");
                            // Re-stage the file after post-processing
                            execSync(`git add "${fileInfo.path}"`, { stdio: "pipe" });
                        }
                    } catch (error) {
                        log(`  ✗ Error post-processing Vue file ${fileInfo.path}: ${error.message}`, "red");
                        errorCount++;
                    }
                }
                log(`✓ Formatted: ${fileInfo.path}`, "green");
                formattedCount++;
            } else {
                log(`✗ Error formatting ${fileInfo.path}: ${result.error}`, "red");
                errorCount++;
            }
        }
    }

    // Format root-level files
    if (fileCategories.root.length > 0) {
        log(`\n[ROOT] Formatting ${fileCategories.root.length} file(s) using prettier (root config)...`, "blue");
        for (const fileInfo of fileCategories.root) {
            const result = formatFile(fileInfo.path);
            if (result.skipped) {
                skippedCount++;
            } else if (result.success) {
                log(`✓ Formatted: ${fileInfo.path}`, "green");
                formattedCount++;
            } else {
                log(`✗ Error formatting ${fileInfo.path}: ${result.error}`, "red");
                errorCount++;
            }
        }
    }

    log("\n--- Summary ---", "blue");
    log(`Formatted: ${formattedCount}`, "green");
    log(`Skipped: ${skippedCount}`, "yellow");
    
    if (errorCount > 0) {
        log(`\n✗ Errors: ${errorCount}`, "red");
        log("Pre-commit hook failed due to formatting errors.", "red");
        log("Please fix the errors above and try again.", "red");
        process.exit(1);
    }

    log("\n✓ Pre-commit formatting completed successfully!", "green");
    process.exit(0);
}

main();
