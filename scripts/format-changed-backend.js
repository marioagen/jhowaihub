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
    const formatArgs = ["dotnet", "format", "WoopiaiHub.sln"];
    
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
        log(`✓ Formatted ${csharpFiles.length} C# file(s)`, "green");
    } catch (error) {
        if (error.status === 0) {
            // Exit code 0 means success
            log(`✓ Formatted ${csharpFiles.length} C# file(s)`, "green");
        } else {
            log(`Error running dotnet format (exit code: ${error.status})`, "red");
            process.exit(1);
        }
    }
}

main();

