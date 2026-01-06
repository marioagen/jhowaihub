#!/usr/bin/env node
/**
 * Script to format only changed C# backend files
 * Works on Windows, Linux, and macOS
 */

const { execSync } = require("child_process");
const path = require("path");
const fs = require("fs");

// Get the repository root directory (where .git folder is)
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

const repoRoot = getRepoRoot();

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
 * Post-processes C# files to enforce parameter wrapping for constructors and methods
 * This ensures that constructors and methods with multiple parameters have each parameter on its own line
 */
function enforceParameterWrapping(filePath) {
    try {
        const fullPath = path.resolve(repoRoot, filePath);
        if (!fs.existsSync(fullPath)) {
            return false;
        }

        let content = fs.readFileSync(fullPath, "utf-8");
        let originalContent = content;

        // Pattern to match method/constructor declarations that span multiple lines
        // This handles cases where parameters are on multiple lines but not consistently formatted
        // Only matches declarations (with access modifiers or at class member level), not method calls
        const multiLineMethodPattern = /((?:public|private|protected|internal|static)\s+)?([\w<>\[\]\.\s]+\s+)?(\w+)\s*\(\s*([^)]*(?:\n[^)]*)*?)\s*\)(?=\s*{|\s*;|\s*=>)/g;
        
        content = content.replace(multiLineMethodPattern, (match, accessModifier, returnType, methodName, params) => {
            // Skip if it's a single line and already short
            if (!match.includes("\n") && match.length <= 120) {
                const paramCount = (params.match(/,/g) || []).length + 1;
                if (paramCount <= 3) {
                    return match;
                }
            }

            // Extract all parameters, handling multi-line cases
            const allParams = params.replace(/\n/g, " ").split(",").map(p => p.trim()).filter(p => p.length > 0);
            
            // Skip if already properly formatted (each param on its own line)
            const linesWithParams = params.split("\n").filter(l => l.trim().length > 0 && l.includes(","));
            if (linesWithParams.length === 0 && allParams.length <= 1) {
                return match;
            }

            // Check if parameters need reformatting
            // If any line has multiple parameters, reformat
            const needsReformat = params.split("\n").some(line => {
                const trimmed = line.trim();
                return trimmed.includes(",") && (trimmed.match(/,/g) || []).length > 0;
            });

            if (!needsReformat && allParams.length <= 3) {
                return match;
            }

            // Reformat: each parameter on its own line
            const indent = "        "; // 8 spaces (2 levels of indentation for class members)
            const paramIndent = indent + "    "; // 12 spaces for parameters
            
            const formattedParams = allParams.map((param, index) => {
                const isLast = index === allParams.length - 1;
                return `${paramIndent}${param}${isLast ? "" : ","}`;
            }).join("\n");

            const accessModifierStr = accessModifier || "";
            const returnTypeStr = returnType || "";
            return `${accessModifierStr}${returnTypeStr}${methodName}(\n${formattedParams}\n${indent})`;
        });

        // Handle long method call chains (like Include chains in CardRepository)
        // Process line by line for method chains
        const lines = content.split("\n");
        const modifiedLines = [];
        
        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];
            const trimmedLine = line.trim();

            // Handle long method call chains on a single line
            if (trimmedLine.includes(".") && trimmedLine.length > 120 && !trimmedLine.includes("\n")) {
                // Check if it's a method chain (multiple .Method() calls)
                const chainMatches = trimmedLine.match(/(\.[\w]+\s*\([^)]*\))/g);
                if (chainMatches && chainMatches.length >= 3) {
                    const indent = line.match(/^(\s*)/)[1];
                    const chainIndent = indent + "    ";
                    
                    // Find where the chain starts (usually after = or return)
                    const chainStart = trimmedLine.search(/\.\w+\s*\(/);
                    if (chainStart > 0) {
                        const beforeChain = trimmedLine.substring(0, chainStart);
                        const chainPart = trimmedLine.substring(chainStart);
                        
                        // Split chain by method calls
                        const parts = chainPart.split(/(\.[\w]+\s*\([^)]*\))/g).filter(p => p.trim().length > 0);
                        if (parts.length > 2) {
                            const formattedChain = parts.map((part, index) => {
                                if (index === 0) {
                                    return part.trim();
                                }
                                return `\n${chainIndent}${part.trim()}`;
                            }).join("");
                            
                            modifiedLines.push(`${indent}${beforeChain}${formattedChain}`);
                            continue;
                        }
                    }
                }
            }

            modifiedLines.push(line);
        }

        const newContent = modifiedLines.join("\n");
        if (newContent !== originalContent) {
            fs.writeFileSync(fullPath, newContent, "utf-8");
            return true;
        }

        return false;
    } catch (error) {
        log(`Warning: Could not post-process ${filePath}: ${error.message}`, "yellow");
        return false;
    }
}

/**
 * Gets changed files from git (unstaged + staged)
 */
function getChangedFiles() {
    try {
        // Get both staged and unstaged files (run from repo root)
        const staged = execSync("git diff --cached --name-only --diff-filter=ACM", {
            encoding: "utf-8",
            stdio: "pipe",
            cwd: repoRoot,
        });
        const unstaged = execSync("git diff --name-only --diff-filter=ACM", {
            encoding: "utf-8",
            stdio: "pipe",
            cwd: repoRoot,
        });
        
        const allFiles = new Set();
        
        staged.split("\n").forEach((line) => {
            const trimmed = line.trim();
            if (trimmed.length > 0) allFiles.add(trimmed);
        });
        
        unstaged.split("\n").forEach((line) => {
            const trimmed = line.trim();
            if (trimmed.length > 0) allFiles.add(trimmed);
        });
        
        return Array.from(allFiles);
    } catch (error) {
        log("Error getting changed files", "red");
        return [];
    }
}

function main() {
    // Get changed C# files
    const changedFiles = getChangedFiles();
    const csharpFiles = changedFiles.filter((file) => /\.cs$/i.test(file));
    
    if (csharpFiles.length === 0) {
        log("No changed C# files to format.", "yellow");
        process.exit(0);
    }
    
    const solutionFile = path.resolve(repoRoot, "WoopiaiHub.sln");
    if (!fs.existsSync(solutionFile)) {
        log("Solution file WoopiaiHub.sln not found", "yellow");
        process.exit(0);
    }
    
    log(`Formatting ${csharpFiles.length} changed C# file(s)...`, "blue");
    
    // Build command with multiple --include flags (one per file)
    const formatArgs = ["dotnet", "format", "WoopiaiHub.sln", "--verbosity", "normal"];
    
    for (const file of csharpFiles) {
        // Normalize path separators to forward slashes (works on all platforms)
        const normalizedPath = file.replace(/\\/g, "/");
        formatArgs.push("--include", normalizedPath);
    }
    
    try {
        execSync(formatArgs, {
            stdio: "inherit",
            cwd: repoRoot,
            encoding: "utf-8",
        });
        
        // Post-process files to enforce parameter wrapping
        log("Post-processing C# files to enforce parameter wrapping...", "blue");
        for (const file of csharpFiles) {
            try {
                if (enforceParameterWrapping(file)) {
                    log(`  Post-processed: ${file}`, "blue");
                }
            } catch (error) {
                log(`  Warning: Could not post-process ${file}: ${error.message}`, "yellow");
            }
        }
        
        log(`✓ Formatted ${csharpFiles.length} C# file(s)`, "green");
    } catch (error) {
        if (error.status === 0) {
            // Exit code 0 means success
            // Post-process files even if format had warnings
            log("Post-processing C# files to enforce parameter wrapping...", "blue");
            for (const file of csharpFiles) {
                try {
                    if (enforceParameterWrapping(file)) {
                        log(`  Post-processed: ${file}`, "blue");
                    }
                } catch (error) {
                    log(`  Warning: Could not post-process ${file}: ${error.message}`, "yellow");
                }
            }
            log(`✓ Formatted ${csharpFiles.length} C# file(s)`, "green");
        } else {
            log(`Error running dotnet format (exit code: ${error.status})`, "red");
            process.exit(1);
        }
    }
}

main();

