# 02 — Casos de Teste (referência)

> Parte de [`../README.md`](../README.md) · Testes

Exemplos de cenários por domínio. **Novas features** devem adicionar casos equivalentes.

---

## Padrão mínimo por método Service

| # | Cenário | Assert |
|---|---------|--------|
| 1 | Input válido | Retorno esperado + Verify Once |
| 2 | Entidade não encontrada | `AppException` NotFound + labelError |
| 3 | Campo obrigatório ausente | `AppException` RequiredField |
| 4 | Duplicidade | `AppException` Duplicated |
| 5 | Conflito de estado | `AppException` Conflict |

---

## Domínio: Document

| Caso | DisplayName sugerido |
|------|---------------------|
| FindAllPaged retorna lista | `FindAllPaged_ValidFilter_ReturnsPagedResult` |
| Delete ids vazios falha | `Delete_EmptyIds_ThrowsAppException` |
| Upload chunk intermediário | `ProcessChunks_NotLastChunk_ReturnsAccepted` |

Referência: `DocumentServicesTests`, `DocumentUploadServicesTests`

---

## Domínio: Card / Workflow

| Caso | DisplayName sugerido |
|------|---------------------|
| Avanço com OCR pendente bloqueia | `UpdateStep_OcrPending_ThrowsAppException` |
| Card não encontrado | `UpdateStepAndStatus_CardNotFound_ThrowsAppException` |
| Workflow inválido na validação | `Create_InvalidSteps_ThrowsAppException` |

Referência: `CardServicesTests`, `WorkflowServicesTests`

---

## Domínio: Automation / Handlers

| Caso | DisplayName sugerido |
|------|---------------------|
| BuildPayload substitui {{ocr}} | `BuildPayload_WithOcrOutput_ReplacesPlaceholder` |
| Tool type inválido | `BuildPayload_InvalidToolType_ThrowsArgumentException` |
| ApiHandler template não encontrado | `BuildPayload_TemplateNotFound_ThrowsAppException` |

Referência: `ApiHandlerTests`, `AutomationServicesTests`

---

## Domínio: Account

| Caso | DisplayName sugerido |
|------|---------------------|
| Login credenciais inválidas | `Login_InvalidPassword_ThrowsAppException` |
| SSO token inválido | `LoginSSO_InvalidToken_ThrowsAppException` |

Referência: `AccountServicesTests`

---

## Fixture — padrão

```csharp
// tests/WoopiAiHub.UnitTests/Fixture/MeuFixture.cs
public static class MeuFixture
{
    public static MeuEntity FindValidEntity() => new(...);
    public static MeuCreateDto FindValidCreateDto() => new(...);
}
```

**Nunca** instanciar modelos Domain complexos inline no teste — usar Fixture.

---

## Collection

```csharp
[CollectionDefinition(nameof(MeuCollection))]
public class MeuCollection { }
```

Agrupa testes que compartilham Fixture/state.

---

## Template para nova feature

```markdown
### {Feature} — {Metodo}

| ID | Cenário | Input | Esperado |
|----|---------|-------|----------|
| TC-01 | Happy path | ... | ... |
| TC-02 | Not found | id=999 | ErrorCode.NotFound |
| TC-03 | ... | ... | ... |
```

Preencher em spec antes de implementar testes.

---

## Documentação relacionada

- Estratégia → [`01-estrategia-testes.md`](./01-estrategia-testes.md)
- AGENTS.md → esqueleto completo de teste
