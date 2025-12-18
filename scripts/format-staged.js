#!/usr/bin/env node
/**
 * Pre-commit hook script to format staged files
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
 * @returns {Object} { type: 'csharp'|'frontend'|'root'|'skip', category: string }
 */
function categorizeFile(filePath) {
    // Normalize path separators
    const normalizedPath = filePath.replace(/\\/g, "/");
    
    // C# backend files - any .cs file
    if (/\.cs$/i.test(filePath)) {
        return { type: "csharp", category: "backend", tool: "dotnet format" };
    }
    
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
 * Formats a non-C# file (frontend or root files)
 */
function formatFile(filePath) {
    const fullPath = path.resolve(filePath);

    // Check if file exists
    if (!fs.existsSync(fullPath)) {
        return { success: true, skipped: true };
    }

    const fileInfo = categorizeFile(filePath);
    
    // Skip files that shouldn't be formatted
    if (fileInfo.type === "skip" || fileInfo.type === "csharp") {
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
        csharp: [],
        frontend: [],
        root: [],
        skip: [],
    };

    for (const file of stagedFiles) {
        const fileInfo = categorizeFile(file);
        fileCategories[fileInfo.type].push({ path: file, ...fileInfo });
    }

    // Log file categorization
    if (fileCategories.csharp.length > 0) {
        log(`\nBackend C# files (${fileCategories.csharp.length}):`, "blue");
        fileCategories.csharp.forEach((f) => log(`  - ${f.path}`, "blue"));
    }
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

    // Format C# backend files using dotnet format
    const csharpFiles = fileCategories.csharp.map((f) => f.path);
    let csharpFormatted = false;

    // Format C# backend files using dotnet format (only changed files)
    if (csharpFiles.length > 0) {
        try {
            const solutionFile = path.resolve("WoopiaiHub.sln");
            if (fs.existsSync(solutionFile)) {
                log(`\n[BACKEND] Formatting ${csharpFiles.length} C# file(s) using dotnet format...`, "blue");

                // Build command with multiple --include flags (one per file)
                // Use relative paths from the solution file location
                const formatArgs = ["dotnet", "format", "WoopiaiHub.sln"];
                
                for (const file of csharpFiles) {
                    // Normalize path separators to forward slashes (works on all platforms)
                    const normalizedPath = file.replace(/\\/g, "/");
                    formatArgs.push("--include", normalizedPath);
                }

                try {
                    // Log which files will be formatted
                    log(`Files to format: ${csharpFiles.join(", ")}`, "blue");
                    log(`Command: ${formatArgs.join(" ")}`, "blue");
                    
                    // Execute dotnet format with proper argument array
                    // Use array format to avoid shell interpretation issues
                    const output = execSync(formatArgs, {
                        stdio: "pipe",
                        cwd: path.resolve("."),
                        encoding: "utf-8",
                    });

                    // Check if any changes were made
                    if (output && output.trim().length > 0) {
                        const outputPreview = output.trim().substring(0, 500);
                        log(`Format output: ${outputPreview}${output.length > 500 ? "..." : ""}`, "blue");
                    } else {
                        log("Format completed (no output)", "blue");
                    }

                    // Stage all formatted C# files
                    for (const file of csharpFiles) {
                        try {
                            execSync(`git add "${file}"`, { stdio: "pipe" });
                        } catch (addError) {
                            log(`Warning: Could not stage ${file}`, "yellow");
                        }
                    }
                    csharpFormatted = true;
                    log(`✓ Formatted ${csharpFiles.length} C# file(s)`, "green");
                } catch (formatError) {
                    // Try to get more details about the error
                    const errorOutput = formatError.stdout || formatError.stderr || formatError.message;
                    if (formatError.status === 0) {
                        // Exit code 0 means success (even if --verify-no-changes found changes)
                        // Stage files anyway
                        for (const file of csharpFiles) {
                            try {
                                execSync(`git add "${file}"`, { stdio: "pipe" });
                            } catch (addError) {
                                log(`Warning: Could not stage ${file}`, "yellow");
                            }
                        }
                        csharpFormatted = true;
                        log(`✓ Formatted ${csharpFiles.length} C# file(s)`, "green");
                    } else {
                        log(`Error running dotnet format (exit code: ${formatError.status})`, "red");
                        if (errorOutput) {
                            log(`Error details: ${errorOutput.substring(0, 500)}`, "red");
                        }
                        log(`Command: ${formatArgs.join(" ")}`, "yellow");
                        
                        // Don't fail the commit if formatting fails, just warn
                        log("Continuing with commit (formatting had issues)", "yellow");
                    }
                }
            } else {
                log("Solution file WoopiaiHub.sln not found", "yellow");
            }
        } catch (error) {
            // dotnet might not be available, continue with other files
            log(`Note: dotnet format error - ${error.message}`, "yellow");
            log("Skipping C# formatting", "yellow");
        }
    }

    let formattedCount = csharpFormatted ? csharpFiles.length : 0;
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
        log(`Errors: ${errorCount}`, "red");
        process.exit(1);
    }

    log("Pre-commit formatting completed successfully!", "green");
    process.exit(0);
}

main();
