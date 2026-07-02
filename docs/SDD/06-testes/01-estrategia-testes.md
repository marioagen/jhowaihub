# 01 — Estratégia de Testes

> Parte de [`../README.md`](../README.md) · Testes

Referência obrigatória: [`../../AGENTS.md`](../../AGENTS.md) §2 + [`../../BACKEND_ARCHITECTURE.md`](../../BACKEND_ARCHITECTURE.md) §22

---

## Pirâmide de testes

```
        ┌─────────┐
        │  E2E    │  Manual / futuro — fora escopo default agente
        ├─────────┤
        │ Integr. │  Poucos — API + DB local quando necessário
        ├─────────┤
        │ Unitário│  ★ Obrigatório para Services
        └─────────┘
```

**Foco do repositório:** testes unitários xUnit em `tests/WoopiAiHub.UnitTests/`.

---

## Stack

| Ferramenta | Uso |
|------------|-----|
| xUnit | Framework `[Fact]`, `[Trait]` |
| Moq | Mocks de interfaces |
| Moq.AutoMock | `AutoMocker` + `CreateInstance<T>()` |
| Fixture estáticas | Construção de entidades/DTOs de teste |

---

## O que testar (obrigatório)

| Camada | Obrigatório? | Como |
|--------|--------------|------|
| **Services (público novo)** | **Sim** | Caminho feliz + cada AppException |
| Repository | Opcional | Preferir testar via Service mockando repo |
| Controller | Opcional | Preferir testar Service |
| ToolHandler | Sim (se novo) | BuildPayload + placeholders |
| Consumer | Opcional | Mock scope + service |
| Frontend | Manual/default | ESLint + Prettier CI |

---

## Estrutura de teste (padrão canônico)

Referência: `CardServicesTests.cs` + `Fixture/CardFixture.cs`

```csharp
[Collection(nameof(MeuCollection))]
public class MeuServicesTests
{
    private readonly AutoMocker _mocker;
    private readonly Mock<IMeuRepository> _repoMock;
    private readonly MeuServices _service;

    public MeuServicesTests()
    {
        _mocker = new AutoMocker();
        _repoMock = _mocker.GetMock<IMeuRepository>();
        _service = _mocker.CreateInstance<MeuServices>();
    }

    [Fact(DisplayName = "...")]
    [Trait("Metodo", "Success")]
    public async Task Metodo_Cenario_Resultado()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

---

## Naming

- Método: `Metodo_Cenario_ResultadoEsperado`
- DisplayName descritivo em inglês ou português claro
- Trait: `"Metodo", "Success"` ou `"Fail"`

---

## Assert de AppException

```csharp
var ex = await Assert.ThrowsAsync<AppException>(() => _service.MeuMetodo(...));
Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
Assert.Equal(DocumentLabel.NotFound, ex.LabelError);
_repoMock.Verify(r => r.FindByIdAsync(1), Times.Once);
```

---

## Executar testes

```bash
dotnet test
# ou
dotnet test tests/WoopiAiHub.UnitTests/
```

CI: workflow `build.yml` — .NET 6 e 8.

---

## Critérios de aceite (DoD) para agente

Antes de considerar feature pronta:

- [ ] Testes unitários passando para Services alterados/criados
- [ ] ESLint ok se alterou frontend (`npm run lint:frontend`)
- [ ] Formatação ok (`npm run format:check` / Prettier)
- [ ] Migration aplicável se alterou modelo
- [ ] i18n pt/en/es para strings novas
- [ ] SDD atualizado se contrato/comportamento mudou

---

## Documentação relacionada

- Casos de teste → [`02-casos-testes.md`](./02-casos-testes.md)
