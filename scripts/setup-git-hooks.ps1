# Script para configurar os Git hooks versionados no Windows (PowerShell)

Write-Host "Configurando Git hooks..." -ForegroundColor Cyan

# Verificar se estamos em um repositorio Git
if (-not (Test-Path ".git")) {
    Write-Host "Erro: Este diretorio nao e um repositorio Git." -ForegroundColor Red
    exit 1
}

# Configurar o caminho dos hooks
git config core.hooksPath .githooks

if (Test-Path ".githooks/pre-commit") {
    Write-Host "Hook pre-commit configurado" -ForegroundColor Green
}

Write-Host ""
Write-Host "Git hooks configurados com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "Para verificar:" -ForegroundColor Yellow
Write-Host "  git config core.hooksPath" -ForegroundColor Yellow
Write-Host ""
Write-Host "Os hooks agora serao executados automaticamente em cada commit." -ForegroundColor Cyan
