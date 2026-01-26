#!/usr/bin/env node
/**
 * Shared utilities for formatting scripts
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

/**
 * Logs a message with color
 */
function log(message, color = "reset") {
    console.log(`${colors[color]}${message}${colors.reset}`);
}

/**
 * Gets the repository root directory
 */
function getRepoRoot() {
    try {
        const gitRoot = execSync("git rev-parse --show-toplevel", {
            encoding: "utf-8",
            stdio: "pipe",
        }).trim();
        return path.resolve(gitRoot);
    } catch (error) {
        // Fallback to current working directory
        return process.cwd();
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
 * @param {string} filePath - Relative path to the file
 * @param {string} repoRoot - Repository root directory
 * @returns {Object} { success: boolean, skipped?: boolean, error?: string }
 */
function formatFile(filePath, repoRoot = process.cwd()) {
    const fullPath = path.resolve(repoRoot, filePath);

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
            const frontendDir = path.resolve(repoRoot, "front-end/vueapp");
            if (fs.existsSync(path.join(frontendDir, "package.json"))) {
                log(`  Formatting frontend file: ${filePath}`, "blue");
                execSync(`npx prettier --write "${fullPath}"`, {
                    cwd: frontendDir,
                    stdio: "pipe",
                });
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
                    cwd: repoRoot,
                });
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

/**
 * Splits Vue component attributes properly, handling quoted strings and directives
 * @param {string} attributes - The attributes string to split
 * @returns {string[]} Array of individual attributes
 */
function splitVueAttributes(attributes) {
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
    
    return attrList;
}

module.exports = {
    log,
    colors,
    getRepoRoot,
    categorizeFile,
    formatFile,
    splitVueAttributes,
};

