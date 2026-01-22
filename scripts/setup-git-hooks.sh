#!/bin/bash
# Script para configurar os Git hooks versionados
# Funciona no Linux, macOS e Git Bash no Windows

set -e

echo "🔧 Configurando Git hooks..."

# Verificar se estamos em um repositório Git
if [ ! -d ".git" ]; then
    echo "❌ Erro: Este diretório não é um repositório Git."
    exit 1
fi

# Configurar o caminho dos hooks
git config core.hooksPath .githooks

# Tornar o hook executável (Linux/macOS)
if [ -f ".githooks/pre-commit" ]; then
    chmod +x .githooks/pre-commit
    echo "✓ Hook pre-commit configurado e tornando executável"
fi

echo ""
echo "✅ Git hooks configurados com sucesso!"
echo ""
echo "Para verificar:"
echo "  git config core.hooksPath"
echo ""
echo "Os hooks agora serão executados automaticamente em cada commit."

