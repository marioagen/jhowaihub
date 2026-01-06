# WoopiAI Hub

## Configuração Inicial

### 1. Instalar Dependências

```bash
# Dependências root
npm install

# Dependências frontend
cd front-end/vueapp
npm install
```

### 2. Configurar Git Hooks

Os hooks do Git estão versionados em `.githooks/`. Para configurá-los:

**Windows (PowerShell):**

```powershell
.\scripts\setup-git-hooks.ps1
```

**Linux/macOS:**

```bash
chmod +x scripts/setup-git-hooks.sh
./scripts/setup-git-hooks.sh
```

**Ou usando npm:**

```bash
npm run setup:hooks
```

Isso configurará o Git para usar os hooks versionados automaticamente em cada commit.

## Formatação de Código

### Pre-commit Hook

O hook `pre-commit` formata automaticamente os arquivos staged antes de cada commit:

- ✅ Formata arquivos frontend (Vue, JS, TS, CSS, etc.)
- ✅ Formata arquivos root (JSON, YAML, MD)
- ✅ Formata componentes Vue (atributos em linhas separadas)
- ✅ Remove linhas vazias desnecessárias
- ❌ **Falha e impede o commit se houver erros de formatação**

### Comandos de Formatação

```bash
# Formatar apenas arquivos alterados
npm run format:changed

# Formatar arquivos staged (simula pre-commit)
npm run format:staged

# Formatar todos os arquivos
npm run format:all
npm run format:frontend:all
```

### Pular o Hook (Não Recomendado)

```bash
git commit --no-verify -m "mensagem"
```

⚠️ **Use apenas em casos excepcionais!**

## CI/CD

O pipeline CI verifica a formatação automaticamente. Se falhar, você precisará formatar o código localmente e fazer um novo commit.

## Estrutura do Projeto

```
├── back-end/          # Backend .NET
├── front-end/         # Frontend Vue.js
├── external-api/      # APIs externas
├── scripts/           # Scripts de formatação e utilitários
├── .githooks/         # Git hooks versionados
└── tests/             # Testes
```

## Desenvolvimento

Consulte `.githooks/README.md` para mais informações sobre os hooks do Git.
