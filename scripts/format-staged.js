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

function formatFile(filePath) {
    const fullPath = path.resolve(filePath);

    // Check if file exists
    if (!fs.existsSync(fullPath)) {
        return { success: true, skipped: true };
    }

    // Check if file is a C# file
    const isCSharpFile = /\.cs$/i.test(filePath);

    // Check if file is in frontend Vue app
    const isFrontendFile = filePath.startsWith("front-end/vueapp/");
    const isFormattable = /\.(js|vue|ts|tsx|json|css|scss|html|yml|yaml|md)$/i.test(filePath);

    // Handle C# files
    if (isCSharpFile) {
        try {
            // Find the solution file or project file
            const backendDir = path.resolve("back-end");
            const solutionFile = path.resolve("WoopiaiHub.sln");

            // Use dotnet format to format the specific file
            // Note: dotnet format works on solution/project level, so we format the whole solution
            // but only staged files will be affected
            if (fs.existsSync(solutionFile)) {
                // Format the solution (dotnet format will respect .editorconfig)
                execSync("dotnet format WoopiaiHub.sln --include-generated", {
                    stdio: "pipe",
                    cwd: path.resolve("."),
                });
                // Stage all formatted files
                execSync(`git add "${filePath}"`, { stdio: "pipe" });
                return { success: true };
            } else if (fs.existsSync(backendDir)) {
                // Try to find any .sln file in the backend directory
                const files = fs.readdirSync(backendDir);
                const slnFile = files.find((f) => f.endsWith(".sln"));
                if (slnFile) {
                    execSync(`dotnet format "${path.join(backendDir, slnFile)}" --include-generated`, {
                        stdio: "pipe",
                        cwd: path.resolve("."),
                    });
                    execSync(`git add "${filePath}"`, { stdio: "pipe" });
                    return { success: true };
                }
            }
            // If dotnet format is not available or solution not found, skip
            return { success: true, skipped: true };
        } catch (error) {
            // dotnet might not be installed or available, that's okay
            return { success: true, skipped: true };
        }
    }

    // Handle frontend and other files
    if (!isFormattable) {
        return { success: true, skipped: true };
    }

    try {
        if (isFrontendFile) {
            // Format using frontend prettier
            const frontendDir = path.resolve("front-end/vueapp");
            if (fs.existsSync(path.join(frontendDir, "package.json"))) {
                execSync(`npx prettier --write "${fullPath}"`, {
                    cwd: frontendDir,
                    stdio: "pipe",
                });
                // Stage the file again after formatting
                execSync(`git add "${filePath}"`, { stdio: "pipe" });
                return { success: true };
            }
        } else {
            // Format using root prettier (if prettier is available)
            try {
                execSync(`npx prettier --write "${fullPath}"`, {
                    stdio: "pipe",
                });
                // Stage the file again after formatting
                execSync(`git add "${filePath}"`, { stdio: "pipe" });
                return { success: true };
            } catch (error) {
                // Prettier might not be installed at root, that's okay
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

    // Check if there are any C# files to format
    const csharpFiles = stagedFiles.filter((file) => /\.cs$/i.test(file));
    let csharpFormatted = false;

    // Format C# files using dotnet format (more efficient to do all at once)
    if (csharpFiles.length > 0) {
        try {
            const solutionFile = path.resolve("WoopiaiHub.sln");
            if (fs.existsSync(solutionFile)) {
                log(`Formatting ${csharpFiles.length} C# file(s) using dotnet format...`, "blue");
                execSync("dotnet format WoopiaiHub.sln --include-generated", {
                    stdio: "pipe",
                    cwd: path.resolve("."),
                });
                // Stage all C# files
                for (const file of csharpFiles) {
                    execSync(`git add "${file}"`, { stdio: "pipe" });
                }
                csharpFormatted = true;
                log(`✓ Formatted ${csharpFiles.length} C# file(s)`, "green");
            }
        } catch (error) {
            // dotnet might not be available, continue with other files
            log("Note: dotnet format not available, skipping C# formatting", "yellow");
        }
    }

    let formattedCount = csharpFormatted ? csharpFiles.length : 0;
    let errorCount = 0;
    let skippedCount = 0;

    // Format other files (frontend, JSON, etc.)
    for (const file of stagedFiles) {
        // Skip C# files as they're already formatted
        if (/\.cs$/i.test(file)) {
            continue;
        }

        const result = formatFile(file);

        if (result.skipped) {
            skippedCount++;
        } else if (result.success) {
            log(`✓ Formatted: ${file}`, "green");
            formattedCount++;
        } else {
            log(`✗ Error formatting ${file}: ${result.error}`, "red");
            errorCount++;
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
