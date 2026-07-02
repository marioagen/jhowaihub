# vueapp

## Project setup

```
npm install
```

### Desenvolvimento normal (requer backend)

```
npm run dev
```

### Protótipo local com dados mockados (sem backend)

```
npm run dev:mock
```

Acesse: **http://localhost:3000**

- Login simulado: qualquer e-mail válido + senha (ex.: `demo@prototype.local` / `demo`)
- Sessão admin automática com acesso a todas as páginas do menu
- API inteiramente mockada no frontend (sem .NET, Docker ou SQL)

### Build produção (backend real)

```
npm run build
```

### Build protótipo mockado (Vercel / static hosting)

```
npm run build:mock
```

Gera `dist/` com:

- `VITE_MOCK_MODE=true` embutido no bundle (via `.env.mock`)
- `dist/config/appsettings.js` substituído por `appsettings.prototype.js`
- Nenhuma dependência de API .NET, Docker ou SQL

Validar localmente:

```
npm run build:mock
npm run preview:mock
```

## Deploy na Vercel (protótipo mockado)

1. Importe o repositório na Vercel.
2. Defina **Root Directory**: `front-end/vueapp`
3. A Vercel detecta o `vercel.json` com:
   - **Build Command**: `npm run build:mock`
   - **Output Directory**: `dist`
4. (Opcional) Environment Variable: `VITE_MOCK_MODE=true`
5. Recomendado: branch dedicada (`prototype`) e **Deployment Protection** ativada.

O protótipo usa hash router (`/#/...`), compatível com hosting estático.

**Importante:** não publique `build:mock` no ambiente de produção real — o mock libera todas as permissões.

### Lint

```
npm run lint
```
