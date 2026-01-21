# Boas Práticas de Git Hooks - Implementadas ✅

Este documento descreve as boas práticas implementadas para os Git hooks do projeto.

## ✅ 1. Hook Falha em Erros de Formatação

**Implementado:** O hook `pre-commit` agora falha (sai com código ≠ 0) se algum formatador falhar, impedindo commits quebrados.

### Como Funciona:

- O script `format-staged.js` retorna código de saída `1` se houver erros
- O hook `.githooks/pre-commit` verifica o código de saída e falha se ≠ 0
- Mensagens de erro claras são exibidas ao desenvolvedor

### Exemplo de Saída em Erro:

```
✗ Error formatting file.vue: Syntax error
✗ Errors: 1
Pre-commit hook failed due to formatting errors.
Please fix the errors above and try again.
❌ Pre-commit hook failed!
```

## ✅ 2. Hooks Versionados e Compartilhados

**Implementado:** Os hooks estão versionados em `.githooks/` e podem ser compartilhados com toda a equipe.

### Estrutura:

```
.githooks/
├── pre-commit          # Hook de formatação
└── README.md           # Documentação

scripts/
├── setup-git-hooks.sh  # Setup para Linux/macOS
└── setup-git-hooks.ps1 # Setup para Windows
```

### Configuração:

Cada desenvolvedor executa uma vez:

```bash
# Windows
.\scripts\setup-git-hooks.ps1

# Linux/macOS
./scripts/setup-git-hooks.sh

# Ou via npm
npm run setup:hooks
```

Isso configura `git config core.hooksPath .githooks` automaticamente.

### Vantagens:

- ✅ Hooks versionados no repositório
- ✅ Todos os desenvolvedores usam os mesmos hooks
- ✅ Mudanças nos hooks são rastreadas pelo Git
- ✅ Fácil de configurar (um comando)

## ✅ 3. Verificação no CI/CD

**Implementado:** Workflow GitHub Actions que executa a mesma rotina de formatação.

### Arquivo: `.github/workflows/format-check.yml`

O workflow:

1. Formata todos os arquivos alterados
2. Verifica se há mudanças não commitadas
3. Falha se encontrar arquivos não formatados
4. Exibe diff das mudanças necessárias

### Execução:

- Automático em Pull Requests para `main` e `develop`
- Automático em pushes para `main` e `develop`
- Pode ser executado manualmente

### Comando Local Equivalente:

```bash
npm run format:changed
```

## Resumo das Implementações

| Boa Prática         | Status | Localização                          |
| ------------------- | ------ | ------------------------------------ |
| Hook falha em erros | ✅     | `scripts/format-staged.js`           |
| Hooks versionados   | ✅     | `.githooks/`                         |
| Setup automatizado  | ✅     | `scripts/setup-git-hooks.*`          |
| Verificação no CI   | ✅     | `.github/workflows/format-check.yml` |
| Documentação        | ✅     | `.githooks/README.md`, `README.md`   |

## Próximos Passos (Opcional)

- [ ] Adicionar mais hooks (pre-push, commit-msg, etc.)
- [ ] Integrar com lint-staged para melhor performance
- [ ] Adicionar hooks para validação de commits (conventional commits)
- [ ] Configurar Husky como alternativa (se necessário)
