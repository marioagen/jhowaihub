# Git Hooks

Este diretório contém os hooks do Git versionados para o projeto.

## Configuração

### Configuração Inicial (apenas uma vez por desenvolvedor)

Execute o script de setup para configurar o Git para usar os hooks versionados:

```bash
# Windows (PowerShell)
.\scripts\setup-git-hooks.ps1

# Linux/macOS
chmod +x scripts/setup-git-hooks.sh
./scripts/setup-git-hooks.sh
```

Ou configure manualmente:

```bash
git config core.hooksPath .githooks
```

### Verificar Configuração

Para verificar se os hooks estão configurados corretamente:

```bash
git config core.hooksPath
```

Deve retornar: `.githooks`

## Hooks Disponíveis

### pre-commit

Formata automaticamente os arquivos staged antes do commit:

- Arquivos frontend (Vue, JS, TS, CSS, etc.) usando Prettier
- Arquivos root (JSON, YAML, MD) usando Prettier
- Formatação especial de componentes Vue (atributos em linhas separadas)
- Remove linhas vazias desnecessárias

**O hook falha (impede o commit) se:**

- Algum formatador retornar erro
- Algum arquivo não puder ser formatado
- Ocorrer qualquer erro durante o processo

## Desabilitar Temporariamente

Para pular os hooks em um commit específico:

```bash
git commit --no-verify -m "mensagem"
```

⚠️ **Atenção:** Use apenas em casos excepcionais. O código deve sempre estar formatado.

## CI/CD

No CI, execute a mesma rotina de formatação para garantir consistência:

```bash
# Verificar formatação (falha se houver diferenças)
node scripts/format-staged.js

# Ou formatar todos os arquivos
npm run format:changed
```
