# Exemplos

> Parte de [`../README.md`](../README.md) · Anexos

Referências rápidas e links para implementações canônicas no repositório.

---

## Documentação estendida

| Documento | Quando consultar |
|-----------|------------------|
| [`../../PRODUCT_DESIGN.md`](../../PRODUCT_DESIGN.md) | Cores, componentes, i18n, wireframes UI |
| [`../../BACKEND_ARCHITECTURE.md`](../../BACKEND_ARCHITECTURE.md) | Camadas, filas, multitenancy, checklist módulo |
| [`../../API_TEMPLATES.md`](../../API_TEMPLATES.md) | Templates HTTP, placeholders, ApiHandler |
| [`../../AGENTS.md`](../../AGENTS.md) | Find*, AppException, testes |
| [`../../README.md`](../../README.md) | Setup local, scripts npm/dotnet |

---

## Código canônico — Backend

| Padrão | Arquivo referência |
|--------|-------------------|
| Controller REST | `Api/Controllers/DocumentController.cs` |
| Service com negócio | `Application/Services/CardServices.cs` |
| Login/auth | `Application/Services/AccountServices.cs` |
| Automação | `Application/Services/Automation/AutomationServices.cs` |
| Tool handler API | `Application/ToolsHandler/ApiHandler.cs` |
| Consumer fila | `Application/Messaging/OcrConsumer.cs` |
| Repository | `Repository/CardRepository.cs` |
| Mapping EF | `Repository/Mappings/CardMap.cs` |
| Exception handler | `Api/Exceptions/GlobalExceptionHandler.cs` |
| Multitenancy | `Repository/Middleware/MultiTenant.cs` |
| DI Application | `Application/DependencyInjection/Extension.cs` |

---

## Código canônico — Frontend

| Padrão | Arquivo referência |
|--------|-------------------|
| Layout autenticado | `layouts/defaultLayout.vue` |
| Sidebar/menu | `components/layout/SidebarComponent.vue` |
| Tabela CRUD | `components/global/TableComponent.vue` |
| Modal confirmação | `components/global/ConfirmModal.vue` |
| Login | `components/authentication/LoginComponent.vue` |
| Service API | `services/documents/DocumentsServices.js` |
| Router + permissão | `router/index.js` |
| i18n PT | `locales/translations/pt.js` |
| Design tokens | `assets/css/global.css` |
| Home/onboarding | `pages/home.vue` |

---

## Código canônico — Testes

| Padrão | Arquivo referência |
|--------|-------------------|
| Service tests | `tests/.../Services/CardServicesTests.cs` |
| Fixture | `tests/.../Fixture/CardFixture.cs` |
| Handler tests | `tests/.../ToolHandlers/ApiHandlerTests.cs` |

---

## Exemplo — lançar erro de negócio

```csharp
throw new AppException(
    ErrorCode.NotFound,
    "Document not found.",
    DocumentLabel.NotFound);
```

Frontend:

```javascript
this.$notify({
    title: 'common.error',
    message: labelKey,  // veio da API
    variant: 'danger',
    icon: 'CircleX',
});
```

---

## Exemplo — endpoint paginado

```csharp
[HttpGet("Paged")]
public IActionResult FindAllPaged([FromQuery] MeuPagedDto dto,
                                  [FromHeader] HeadersDto headers)
{
    return Ok(_services.FindAllPaged(dto, headers.EmailCreator));
}
```

---

## Exemplo — chave i18n (3 idiomas)

```javascript
// pt.js
meuModulo: { title: 'Meu Módulo', save: 'Salvar' }
// en.js
meuModulo: { title: 'My Module', save: 'Save' }
// es.js
meuModulo: { title: 'Mi Módulo', save: 'Guardar' }
```

---

## Exemplo — item de menu com permissão

```javascript
{
    permission: 'MeuModulo',
    activeKey: 'MeuModulo',
    to: '/meu-modulo',
    icon: { name: 'Box', color: '#0d6efd' },
    labelKey: 'pages.meuModulo',
}
```

---

## Template nova funcionalidade

→ [`template-nova-funcionalidade.md`](./template-nova-funcionalidade.md)

---

## Prompt compacto para agente

```text
Contexto: docs/SDD/README.md
UI: docs/PRODUCT_DESIGN.md | Backend: docs/BACKEND_ARCHITECTURE.md
Código: AGENTS.md
Tarefa: {descrever feature}
Antes de codar: preencher template-nova-funcionalidade.md
```
