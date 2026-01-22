#!/usr/bin/env node
/**
 * Script to format only changed files (not staged)
 * Works on Windows, Linux, and macOS
 */

const path = require("path");
const { log, getRepoRoot, formatFile } = require("./utils/formatting-utils");

const repoRoot = getRepoRoot();

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

/**
 * Gets changed files matching specific patterns
 */
function getChangedFilesByPattern(patterns) {
    const changedFiles = getChangedFiles();
    const regexPatterns = patterns.map((p) => new RegExp(p, "i"));
    
    return changedFiles.filter((file) => {
        return regexPatterns.some((regex) => regex.test(file));
    });
}

function main() {
    const args = process.argv.slice(2);
    
    if (args.length === 0) {
        log("Usage: node format-changed.js <type>", "red");
        log("  type: root|frontend", "yellow");
        process.exit(1);
    }
    
    const type = args[0];
    
    if (type === "root") {
        // Format root-level files (json, yml, yaml, md) that are not in frontend/backend dirs
        const patterns = [
            /\.(json|yml|yaml|md)$/i,
        ];
        
        const files = getChangedFilesByPattern(patterns).filter((file) => {
            const normalized = file.replace(/\\/g, "/");
            return (
                !normalized.startsWith("front-end/") &&
                !normalized.startsWith("back-end/") &&
                !normalized.startsWith("external-api/") &&
                !normalized.startsWith("node_modules/")
            );
        });
        
        if (files.length === 0) {
            log("No changed root files to format.", "yellow");
            process.exit(0);
        }
        
        log(`Formatting ${files.length} changed root file(s)...`, "blue");
        for (const file of files) {
            const result = formatFile(file, repoRoot);
            if (result.success && !result.skipped) {
                log(`✓ Formatted: ${file}`, "green");
            } else if (result.error) {
                log(`✗ Error formatting ${file}: ${result.error}`, "red");
            }
        }
    } else if (type === "frontend") {
        // Format frontend files
        const patterns = [
            /\.(js|vue|ts|tsx|json|css|scss|html)$/i,
        ];
        
        const files = getChangedFilesByPattern(patterns).filter((file) => {
            const normalized = file.replace(/\\/g, "/");
            return normalized.startsWith("front-end/vueapp/");
        });
        
        if (files.length === 0) {
            log("No changed frontend files to format.", "yellow");
            process.exit(0);
        }
        
        log(`Formatting ${files.length} changed frontend file(s)...`, "blue");
        
        for (const file of files) {
            const result = formatFile(file, repoRoot);
            if (result.success && !result.skipped) {
                log(`✓ Formatted: ${file}`, "green");
            } else if (result.error) {
                log(`✗ Error formatting ${file}: ${result.error}`, "red");
            }
        }
    } else {
        log(`Unknown type: ${type}`, "red");
        process.exit(1);
    }
}

main();

