# AGENTS.md

Instruções para qualquer assistente de IA (Cursor, Codex, Copilot, Claude Code, etc.) que gere ou modifique código neste repositório. Cursor também lê estas regras automaticamente via `.cursor/rules/*.mdc` — manter sincronizado.

---

## 1. Padrões de código

- Clean Code sempre. Métodos com mais de **20 linhas** (sem contar chaves e linha em branco) devem ser quebrados em métodos privados menores com nome descritivo.
- **Nomes descritivos** em variáveis e métodos. Sem abreviações obscuras (`usr`, `tmp`, `x1`). Prefira `userPermissions` a `perms`, `FindActiveUserByEmail` a `Get`.
- **Single Responsibility**: um método faz uma coisa. Se o nome usa "And"/"Or" no meio, geralmente são dois métodos.
- `async/await` end-to-end. Propague `CancellationToken` quando o método já o recebe ou quando chama algo que aceita.

### Nomenclatura de métodos de leitura (GET)

- **Nunca use o prefixo `Get` em métodos de leitura.** Use sempre `Find`.
- Isso se aplica a `Services`, `Repository`, `Interfaces` e qualquer outra camada.

```csharp
// BAD
GetUserById(Guid id)
GetAllActiveUsers()

// GOOD
FindUserById(Guid id)
FindAllActiveUsers()
```

### Comentários

- **Não adicione comentários explicando o que o código faz**. O nome deve explicar.
- Comentário só é aceitável quando explica **por que** algo não óbvio existe (workaround, decisão de negócio, restrição externa). Escreva o motivo, não a tradução do código.
- Nunca deixe comentário narrando uma mudança ("// agora valida X", "// adicionado conforme HU-123").

```csharp
// BAD
// incrementa contador
counter++;

// GOOD (explica o porquê)
// SDK do SendGrid retorna 202 com body vazio em rate-limit; tratamos como sucesso parcial.
if (response.StatusCode == HttpStatusCode.Accepted) { ... }
```

### XML `<summary>` em métodos novos

- Todo **método público novo** em `Services`, `Repository`, `Controller` e `Interfaces` deve ter `/// <summary>` no mesmo padrão dos métodos vizinhos (veja `back-end/WoopiAiHub.Application/Services/AccountServices.cs` e `RefreshTokenServices.cs`).
- Inclua `<param>` para cada parâmetro relevante e `<returns>` quando há retorno.
- Não duplique a assinatura no summary; descreva o **propósito**, não o "o quê".

```csharp
/// <summary>
/// Revokes every active refresh token associated with the given user email.
/// Used when the password is reset to log the user out from all devices.
/// </summary>
/// <param name="userEmail">Email of the user whose tokens must be revoked.</param>
public Task RevokeAllByUserAsync(string userEmail) { ... }
```

---

## 2. Testes de unidade

A referência canônica é `tests/WoopiAiHub.UnitTests/Services/CardServicesTests.cs` + `tests/WoopiAiHub.UnitTests/Fixture/CardFixture.cs`. Todo teste novo deve seguir esse padrão.

### Quando criar testes

- **Todo método novo (público) em `Services` exige teste de unidade** cobrindo o caminho feliz e os principais caminhos de erro.
- Alteração de método existente exige atualizar/adicionar cenários para o novo comportamento.

### Estrutura obrigatória

1. **xUnit** (`[Fact]` com `DisplayName` descritivo + `[Trait("Method", "Success|Fail")]`).
2. **Moq + Moq.AutoMock**: instanciar via `new AutoMocker()` e `_mocker.CreateInstance<TServices>()`. Mocks individuais via `_mocker.GetMock<TInterface>()`.
3. **Fixture estática** em `tests/WoopiAiHub.UnitTests/Fixture/<Entity>Fixture.cs` para construir DTOs/entidades de teste (`FindValidXxx()`). Nunca instancie modelos do Domain diretamente no teste — adicione/reaproveite no Fixture.
4. **`[Collection(nameof(XxxCollection))]`** na classe de teste para agrupar pelo `CollectionDefinition` do Fixture correspondente.
5. **AAA explícito** com comentários `// Arrange`, `// Act`, `// Assert`.
6. **Nome do método de teste**: `Metodo_Cenario_ResultadoEsperado` (ex: `UpdateStepAndStatus_CardNotFound_ThrowsAppException`).

### Esqueleto de referência

```csharp
[Collection(nameof(MyCollection))]
public class MyServicesTests
{
    private readonly AutoMocker _mocker;
    private readonly Mock<IMyRepository> _myRepositoryMock;
    private readonly MyServices _myServices;

    public MyServicesTests()
    {
        _mocker = new AutoMocker();
        _myRepositoryMock = _mocker.GetMock<IMyRepository>();
        _myServices = _mocker.CreateInstance<MyServices>();
    }

    [Fact(DisplayName = "Tests DoSomething and returns true when input is valid")]
    [Trait("DoSomething", "Success")]
    public async Task DoSomething_ValidInput_ReturnsTrue()
    {
        // Arrange
        var dto = MyFixture.FindValidDto();
        _myRepositoryMock.Setup(r => r.FindById(dto.Id)).ReturnsAsync(MyFixture.FindValidEntity());

        // Act
        var result = await _myServices.DoSomething(dto);

        // Assert
        Assert.True(result);
        _myRepositoryMock.Verify(r => r.FindById(dto.Id), Times.Once);
    }
}
```

### Cobertura mínima

- Caminho feliz + cada `throw` do método (use `Assert.ThrowsAsync<AppException>` validando `ErrorCode` e `LabelError`).
- Verificar interações relevantes com `.Verify(..., Times.Once)`.
