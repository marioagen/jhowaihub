# Documentação - Templates de API

## Índice

1. [Visão Geral](#visão-geral)
2. [Modelo de Dados](#modelo-de-dados)
3. [CRUD de Templates](#crud-de-templates)
4. [Configuração em Workflows](#configuração-em-workflows)
5. [Execução e Processamento](#execução-e-processamento)
6. [Fluxo Completo](#fluxo-completo)
7. [Exemplos de Uso](#exemplos-de-uso)

---

## Visão Geral

A funcionalidade de **Templates de API** permite que usuários criem, gerenciem e executem chamadas HTTP parametrizáveis dentro de Workflows (Esteiras) de automação. Esta feature possibilita a integração com APIs externas de forma padronizada e reutilizável.

### Principais Componentes

- **ApiTemplate**: Entidade que armazena configurações de requisições HTTP
- **ApiHandler**: Responsável por construir e preparar requisições para execução
- **ApiOutputConsumer**: Processa as respostas das requisições executadas
- **ApiOutputServices**: Gerencia o salvamento e notificação de resultados

---

## Modelo de Dados

### Entidade: ApiTemplate

```csharp
public class ApiTemplate
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public string Name { get; set; }              // Nome identificador do template
    public string Method { get; set; }            // Método HTTP (GET, POST, PUT, DELETE, PATCH)
    public string Url { get; set; }               // URL base do endpoint
    public string? QueryTemplate { get; set; }    // Template de query parameters (opcional)
    public string? HeaderTemplate { get; set; }   // Template de headers (opcional)
    public string? BodyTemplate { get; set; }     // Template do corpo da requisição (opcional)
}
```

### Validações

- **Name**: Obrigatório, não pode ser vazio
- **Method**: Obrigatório, deve ser um dos seguintes valores:
    - `GET`
    - `POST`
    - `PUT`
    - `DELETE`
    - `PATCH`
- **Url**: Obrigatória, não pode ser vazia
- **QueryTemplate, HeaderTemplate, BodyTemplate**: Opcionais

---

## CRUD de Templates

### 1. Criar Template

**Endpoint**: `POST /api/ApiTemplate`

**Request Body**:

```json
{
    "name": "Webhook Notificação",
    "method": "POST",
    "url": "https://api.exemplo.com/webhooks/notificar",
    "queryTemplate": "{\"token\": \"{{token}}\"}",
    "headerTemplate": "{\"Content-Type\": \"application/json\", \"Authorization\": \"Bearer {{auth_token}}\"}",
    "bodyTemplate": "{\"message\": \"{{prompt}}\", \"data\": {{ocr}}}"
}
```

**Response**: `200 OK`

```json
true
```

### 2. Listar Templates

#### Listagem Simples

**Endpoint**: `GET /api/ApiTemplate`

**Query Parameters**:

- `input` (opcional): Filtro por nome do template
- `method` (opcional): Filtro por método HTTP
- `orderBy` (opcional): Ordenação (`created asc`, `created desc`, `name asc`, `name desc`)

**Response**: `200 OK`

```json
[
    {
        "id": 1,
        "created": "2024-01-15T10:30:00",
        "name": "Webhook Notificação",
        "method": "POST",
        "url": "https://api.exemplo.com/webhooks/notificar",
        "queryTemplate": "{\"token\": \"{{token}}\"}",
        "headerTemplate": "{\"Content-Type\": \"application/json\"}",
        "bodyTemplate": "{\"message\": \"{{prompt}}\"}"
    }
]
```

#### Listagem Paginada

**Endpoint**: `GET /api/ApiTemplate/paged`

**Query Parameters**:

- `page`: Número da página (obrigatório, > 0)
- `pageSize`: Tamanho da página (padrão: 10)
- `input` (opcional): Filtro por nome
- `method` (opcional): Filtro por método HTTP
- `orderBy` (opcional): Ordenação

**Response**: `200 OK`

```json
{
  "items": [...],
  "totalCount": 50,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

### 3. Buscar Template por ID

**Endpoint**: `GET /api/ApiTemplate/{id}`

**Response**: `200 OK`

```json
{
    "id": 1,
    "created": "2024-01-15T10:30:00",
    "name": "Webhook Notificação",
    "method": "POST",
    "url": "https://api.exemplo.com/webhooks/notificar",
    "queryTemplate": null,
    "headerTemplate": "{\"Content-Type\": \"application/json\"}",
    "bodyTemplate": "{\"message\": \"{{prompt}}\"}"
}
```

### 4. Atualizar Template

**Endpoint**: `PUT /api/ApiTemplate`

**Request Body**:

```json
{
    "id": 1,
    "name": "Webhook Notificação Atualizado",
    "method": "POST",
    "url": "https://api.exemplo.com/webhooks/v2/notificar",
    "queryTemplate": null,
    "headerTemplate": "{\"Content-Type\": \"application/json\"}",
    "bodyTemplate": "{\"message\": \"{{prompt}}\", \"timestamp\": \"{{timestamp}}\"}"
}
```

**Response**: `200 OK`

```json
true
```

### 5. Excluir Template

**Endpoint**: `DELETE /api/ApiTemplate/{id}`

**Response**: `200 OK`

```json
true
```

---

## Configuração em Workflows

### Como Adicionar uma Ferramenta API em uma Esteira

1. **Selecionar o Tool Type**: `API`
2. **Configurar o StepTool**: Associar um template de API existente
3. **Definir Parâmetros**: Os parâmetros são armazenados criptografados e incluem:
    - `TemplateId`: ID do template a ser usado
    - Configurações específicas da requisição

### Estrutura de Parâmetros no StepTool

Os parâmetros do StepTool são salvos criptografados no formato:

```json
{
    "templateId": 1,
    "url": "https://api.exemplo.com/endpoint",
    "method": "POST",
    "query": {
        "param1": "value1"
    },
    "headers": {
        "Authorization": "Bearer token"
    },
    "body": "{\"data\": \"{{ocr}}\", \"prompt\": \"{{prompt}}\"}"
}
```

### Placeholders Suportados

Os templates podem utilizar placeholders que serão substituídos durante a execução:

- `{{ocr}}`: Resultado da ferramenta OCR (texto extraído de documentos)
- `{{embeddings}}`: Resultado da ferramenta Embeddings (textos vetorizados)
- `{{prompt}}`: Resultado da ferramenta Prompt (resposta do LLM)

Estes placeholders são substituídos pelos valores reais provenientes das ferramentas anteriores no workflow.

---

## Execução e Processamento

### Fluxo de Execução

```
┌─────────────────────────────────────────────────────────────────────┐
│                        FLUXO DE EXECUÇÃO API                        │
└─────────────────────────────────────────────────────────────────────┘

1. PREPARAÇÃO (ApiHandler.BuildPayload)
   ├─ Busca StepTool e Template
   ├─ Descriptografa parâmetros
   ├─ Substitui placeholders com outputs anteriores
   └─ Monta ApiRequestDto

2. ENVIO (Via RabbitMQ)
   ├─ Envia para fila: ApiRequestQueue
   └─ Aguarda processamento externo

3. PROCESSAMENTO EXTERNO
   └─ Worker externo executa requisição HTTP

4. RETORNO (Via RabbitMQ)
   └─ Resposta enviada para: ApiRequestQueueResponse

5. CONSUMO (ApiOutputConsumer)
   ├─ Consome mensagem da fila de resposta
   └─ Chama ApiOutputServices.ProcessMessage

6. PROCESSAMENTO DE RESPOSTA (ApiOutputServices)
   ├─ Salva output do StepTool
   ├─ Registra histórico no documento
   ├─ Atualiza status de execução
   ├─ Notifica progresso via Hub
   └─ Continua workflow (próxima ferramenta)
```

### 1. Preparação da Requisição (ApiHandler)

O `ApiHandler` implementa a interface `IToolHandler` e é responsável por:

#### Método: BuildPayload

```csharp
public async Task<ExecutionMessageDto> BuildPayload(
    AutomationServicesDto automationServicesDto,
    StepToolParameter? input,
    ICollection<StepToolOutput> outputs,
    StepToolExecution? execution = null)
```

**Processo**:

1. **Busca o StepTool**: Recupera as configurações do banco de dados
2. **Validações**:
    - Verifica se o tool type é `API`
    - Garante que há parâmetros configurados
3. **Descriptografa Parâmetros**: Usa `IEncryptionService` para descriptografar
4. **Busca Template**: Recupera o template de API pelo ID
5. **Processa Body**: Substitui placeholders pelos valores reais dos outputs
6. **Monta ApiRequestDto** com:
    - Dados do template
    - Metadados da automação (CardId, StepToolId)
    - Informações de contexto (Email, Tenant)
    - ID da execução
    - Fila de resposta

**Output**: `ExecutionMessageDto` contendo:

- `Queue`: Nome da fila de destino (`ApiRequestQueue`)
- `Message`: Objeto `ApiRequestDto` serializado

### 2. Substituição de Placeholders

#### Método: ConvertOutputsToJson

Processa o body do template substituindo placeholders:

```csharp
private string ConvertOutputsToJson(
    ICollection<StepToolOutput> outputs,
    string inputValue)
```

**Lógica**:

1. **Itera sobre outputs anteriores** do workflow
2. **Identifica o tipo de ferramenta**:
    - `OCR` → Placeholder `{{ocr}}`
    - `Embeddings` → Placeholder `{{embeddings}}`
    - `Prompt` → Placeholder `{{prompt}}`
3. **Extrai o valor**:
    - Para OCR e Embeddings: Extrai textos do JSON `DocumentEmbeddings`
    - Para Prompt: Usa o valor direto
4. **Substitui no template**: Replace case-insensitive
5. **Serializa JSON**: Garante formato válido se necessário

**Exemplo**:

Template:

```json
{
    "extractedText": "{{ocr}}",
    "analysis": "{{prompt}}"
}
```

Após substituição:

```json
{
    "extractedText": "Texto extraído do documento via OCR",
    "analysis": "Análise gerada pelo modelo de linguagem"
}
```

### 3. Estrutura da Mensagem de Requisição

#### ApiRequestDto

```csharp
public record class ApiRequestDto
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; }
    public string Url { get; set; }
    public string Method { get; set; }
    public Dictionary<string, string>? Query { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
    public MetaDataAutomationDto Data { get; set; }
    public string Tenant { get; set; }
    public string Email { get; set; }
    public int? ExecutionId { get; set; }
    public string ResponseQueue { get; set; }
    public string? ReferenceFile { get; set; }
}
```

**Campos**:

- **TemplateId/TemplateName**: Identificação do template usado
- **Url/Method**: Endpoint e método HTTP
- **Query/Headers/Body**: Dados da requisição
- **Data**: Metadados (CardId, StepToolId)
- **Tenant/Email**: Contexto do usuário
- **ExecutionId**: ID da execução atual
- **ResponseQueue**: Fila para retorno da resposta
- **ReferenceFile**: Arquivo de referência do documento

### 4. Envio para Fila RabbitMQ

A mensagem é enviada para a fila configurada em `MessageQueues.ApiRequestQueue`.

**Configuração**:

```csharp
public class MessageQueues
{
    public string ApiRequestQueue { get; set; }
    public string ApiRequestQueueResponse { get; set; }
}
```

### 5. Processamento Externo

Um worker externo (não implementado neste projeto) é responsável por:

1. Consumir mensagens da `ApiRequestQueue`
2. Executar a requisição HTTP real
3. Enviar a resposta para `ApiRequestQueueResponse`

### 6. Consumo de Resposta (ApiOutputConsumer)

#### Serviço: ApiOutputConsumer

```csharp
public class ApiOutputConsumer : BaseConsumer
```

**Processo**:

1. **Escuta a fila**: `ApiRequestQueueResponse`
2. **Para cada mensagem**:
    - Cria um scope de serviços
    - Chama `IApiOutputServices.ProcessMessage`
    - Registra uso diário (métrica)
    - Continua a execução do workflow
    - Trata exceções e loga erros

### 7. Processamento de Resposta (ApiOutputServices)

#### Método: ProcessMessage

```csharp
public async Task<AutomationServicesDto> ProcessMessage(ApiOutputDto outputDto)
```

**Estrutura da Resposta (ApiOutputDto)**:

```csharp
public record class ApiOutputDto
{
    public string TemplateName { get; set; }
    public string Tenant { get; set; }
    public string Email { get; set; }
    public int ExecutionId { get; set; }
    public int StatusCode { get; set; }      // HTTP Status Code
    public string? Content { get; set; }      // Corpo da resposta
}
```

**Processo**:

1. **Busca a Execução**:

    ```csharp
    var execution = await _stepToolExecutionRepository
        .FindByIdAsync(outputDto.ExecutionId);
    ```

2. **Monta Conteúdo do Output**:

    ```csharp
    var content = JsonSerializer.Serialize(new {
        outputDto.TemplateName,
        outputDto.StatusCode,
        outputDto.Content
    });
    ```

3. **Cria StepToolOutput**:

    ```csharp
    var stepToolOutput = new StepToolOutput(
        0,
        DateTime.Now,
        execution.StepToolId,
        execution.CardId,
        content
    );
    await _stepToolOutputRepository.CreateAsync(stepToolOutput);
    ```

4. **Registra Histórico do Documento**:

    ```csharp
    var documentHistory = new DocumentHistory(
        execution.Card.DocumentId,
        "API",
        content,
        0,
        DateTime.Now
    );
    _documentHistoryRepository.Create(documentHistory);
    ```

5. **Atualiza Status da Execução**:

    ```csharp
    private async Task UpdateExecutionAsync(
        StepToolExecution execution,
        string email)
    {
        // Calcula progresso
        var count = await _stepToolExecutionRepository
            .ExecutionsByStepIdCountAsync(
                execution.StepTool.StepId,
                execution.CardId
            );
        var percent = (count / execution.StepTool.Order) * 100;

        // Atualiza status
        execution.UpdateStatusExecution(StatusExecution.Ready);
        await _stepToolExecutionRepository.UpdateAsync(execution);

        // Notifica progresso via Hub
        var tool = await _workflowRepository
            .FindToolByStepToolId(execution.StepToolId);
        await _hubNotifier.CardProgessAsync(
            email,
            execution.CardId,
            percent,
            execution.StepTool.StepId,
            tool?.Name ?? string.Empty
        );
    }
    ```

6. **Retorna DTO para Continuar Workflow**:
    ```csharp
    return new AutomationServicesDto(
        execution.StepToolId,
        execution.CardId,
        outputDto.Tenant,
        outputDto.Email,
        execution.Card.Document.ReferenceFile,
        0
    );
    ```

---

## Fluxo Completo

### Diagrama de Sequência

```
Usuario          Frontend         Backend         RabbitMQ        Worker        Backend
  |                |                |                |              |              |
  |  Cria Template |                |                |              |              |
  |--------------->|  POST /template|                |              |              |
  |                |--------------->|                |              |              |
  |                |                | Save DB        |              |              |
  |                |                |-------------   |              |              |
  |                |<---------------|                |              |              |
  |<---------------|                |                |              |              |
  |                |                |                |              |              |
  |  Executa       |                |                |              |              |
  |  Workflow      |                |                |              |              |
  |--------------->|  Trigger       |                |              |              |
  |                |--------------->|                |              |              |
  |                |                | ApiHandler     |              |              |
  |                |                | BuildPayload   |              |              |
  |                |                |-------------   |              |              |
  |                |                |                |              |              |
  |                |                | Publish        |              |              |
  |                |                |--------------->|              |              |
  |                |                |             (ApiRequestQueue) |              |
  |                |                |                |              |              |
  |                |                |                | Consume      |              |
  |                |                |                |------------->|              |
  |                |                |                |              | Execute HTTP |
  |                |                |                |              |------------- |
  |                |                |                |              |              |
  |                |                |                |  Publish     |              |
  |                |                |                |<-------------|              |
  |                |                |       (ApiRequestQueueResponse)|              |
  |                |                |                |              |              |
  |                |                | Consume        |              |              |
  |                |                |<---------------|              |              |
  |                |                | ApiOutputConsumer             |              |
  |                |                |-------------                  |              |
  |                |                | ProcessMessage |              |              |
  |                |                |--------------->|              |              |
  |                |                |             ApiOutputServices |              |
  |                |                |                |              |              |
  |                | Notificação    |                |              |              |
  |<--------------------------------------- Hub -----|              |              |
  |                |                |                |              |              |
  |                |                | Continue       |              |              |
  |                |                | Workflow       |              |              |
  |                |                |-------------   |              |              |
```

### Exemplo Passo a Passo

#### Cenário: Enviar Texto Extraído de PDF para API Externa

**1. Criar Template**

```http
POST /api/ApiTemplate
Content-Type: application/json

{
  "name": "Enviar Texto Extraído",
  "method": "POST",
  "url": "https://api.external.com/process",
  "headerTemplate": "{\"Content-Type\": \"application/json\", \"X-Api-Key\": \"secret123\"}",
  "bodyTemplate": "{\"extractedText\": \"{{ocr}}\", \"metadata\": {\"source\": \"pdf\"}}"
}
```

**2. Configurar Workflow**

Criar um workflow com os seguintes steps:

- **Step 1**: OCR Tool (extrai texto do documento)
- **Step 2**: API Tool (envia texto extraído)

**3. Execução**

Quando um documento é processado:

1. **OCR Tool executa** → Gera output:

    ```json
    {
        "DocumentEmbeddings": [{ "Text": "Linha 1 do documento" }, { "Text": "Linha 2 do documento" }]
    }
    ```

2. **API Tool inicia** → `ApiHandler.BuildPayload`:
    - Busca template "Enviar Texto Extraído"
    - Substitui `{{ocr}}` por: `"Linha 1 do documento\n\nLinha 2 do documento"`
    - Gera `ApiRequestDto`:
        ```json
        {
            "templateId": 1,
            "templateName": "Enviar Texto Extraído",
            "url": "https://api.external.com/process",
            "method": "POST",
            "headers": {
                "Content-Type": "application/json",
                "X-Api-Key": "secret123"
            },
            "body": "{\"extractedText\": \"Linha 1 do documento\\n\\nLinha 2 do documento\", \"metadata\": {\"source\": \"pdf\"}}",
            "executionId": 123,
            "responseQueue": "api-response-queue",
            "tenant": "client-tenant",
            "email": "user@example.com"
        }
        ```

3. **Mensagem enviada para RabbitMQ** → `ApiRequestQueue`

4. **Worker externo consome e executa**:

    ```http
    POST https://api.external.com/process
    Content-Type: application/json
    X-Api-Key: secret123

    {
      "extractedText": "Linha 1 do documento\n\nLinha 2 do documento",
      "metadata": {
        "source": "pdf"
      }
    }
    ```

5. **Worker recebe resposta**:

    ```json
    {
        "status": "success",
        "processed": true,
        "id": "abc123"
    }
    ```

6. **Worker envia para fila de resposta**:

    ```json
    {
        "templateName": "Enviar Texto Extraído",
        "tenant": "client-tenant",
        "email": "user@example.com",
        "executionId": 123,
        "statusCode": 200,
        "content": "{\"status\": \"success\", \"processed\": true, \"id\": \"abc123\"}"
    }
    ```

7. **ApiOutputConsumer processa**:
    - Salva output no banco
    - Registra histórico
    - Atualiza execução para "Ready"
    - Notifica usuário via Hub
    - Continua workflow (se houver próximo step)

---

## Exemplos de Uso

### Exemplo 1: Webhook Simples (GET)

**Template**:

```json
{
    "name": "Verificar Status",
    "method": "GET",
    "url": "https://api.status.com/check",
    "queryTemplate": "{\"service\": \"processing\"}",
    "headerTemplate": "{\"Authorization\": \"Bearer token123\"}"
}
```

**Requisição Gerada**:

```http
GET https://api.status.com/check?service=processing
Authorization: Bearer token123
```

### Exemplo 2: POST com Dados de OCR e Prompt

**Template**:

```json
{
    "name": "Análise Completa",
    "method": "POST",
    "url": "https://api.analytics.com/analyze",
    "headerTemplate": "{\"Content-Type\": \"application/json\"}",
    "bodyTemplate": "{\"document\": \"{{ocr}}\", \"analysis\": \"{{prompt}}\", \"timestamp\": \"2024-01-15T10:00:00Z\"}"
}
```

**Workflow**:

1. OCR → Extrai texto
2. Prompt → Analisa texto
3. API → Envia ambos para sistema externo

**Requisição Gerada**:

```http
POST https://api.analytics.com/analyze
Content-Type: application/json

{
  "document": "Texto extraído do documento",
  "analysis": "Análise do LLM sobre o documento",
  "timestamp": "2024-01-15T10:00:00Z"
}
```

### Exemplo 3: PUT para Atualizar Recurso

**Template**:

```json
{
    "name": "Atualizar Registro",
    "method": "PUT",
    "url": "https://api.crm.com/contacts/123",
    "headerTemplate": "{\"Content-Type\": \"application/json\", \"X-Api-Key\": \"key456\"}",
    "bodyTemplate": "{\"notes\": \"{{prompt}}\", \"lastUpdate\": \"2024-01-15\"}"
}
```

**Requisição Gerada**:

```http
PUT https://api.crm.com/contacts/123
Content-Type: application/json
X-Api-Key: key456

{
  "notes": "Notas geradas pelo LLM",
  "lastUpdate": "2024-01-15"
}
```

### Exemplo 4: DELETE Simples

**Template**:

```json
{
    "name": "Remover Cache",
    "method": "DELETE",
    "url": "https://api.cache.com/entries/temp",
    "headerTemplate": "{\"Authorization\": \"Bearer token789\"}"
}
```

**Requisição Gerada**:

```http
DELETE https://api.cache.com/entries/temp
Authorization: Bearer token789
```

---

## Tratamento de Erros

### Possíveis Erros

1. **Template não encontrado**:

    ```json
    {
        "errorCode": "NotFound",
        "message": "Template not found"
    }
    ```

2. **Tool type inválido**:

    ```json
    {
        "errorCode": "InvalidValue",
        "message": "Invalid tool type for API handler"
    }
    ```

3. **Parâmetros não configurados**:

    ```json
    {
        "errorCode": "NotFound",
        "message": "No API was found configured for the specified step tool."
    }
    ```

4. **ExecutionId não definido**:

    ```json
    {
        "errorCode": "InvalidValue",
        "message": "ExecutionId not defined"
    }
    ```

5. **Erro no processamento**:
    - Logs são registrados via `ILogger`
    - Exceção não interrompe outros consumidores
    - Mensagem pode ser reprocessada (dependendo da configuração do RabbitMQ)

---

## Considerações de Segurança

1. **Criptografia de Parâmetros**:
    - Parâmetros sensíveis são criptografados usando `IEncryptionService`
    - Descriptografia ocorre apenas durante execução

2. **Autenticação**:
    - Todos os endpoints requerem autenticação JWT
    - Header: `Authorization: Bearer {token}`

3. **Isolamento por Tenant**:
    - Cada requisição carrega informação de tenant
    - Garantia de isolamento de dados

4. **Validação de Inputs**:
    - Validação de método HTTP
    - Validação de campos obrigatórios
    - Proteção contra injeção JSON

---

## Métricas e Monitoramento

### Métricas Registradas

1. **Uso Diário**:

    ```csharp
    await usageDailyServices.AddByValuesAsync(
        MetricNames.Automation,
        message.Email,
        1
    );
    ```

2. **Progresso de Cards**:
    - Notificação em tempo real via SignalR Hub
    - Percentual calculado baseado em ferramentas completadas

### Logs

- **ApiOutputConsumer**: Registra falhas no processamento
- **ApiTemplateServices**: Registra exceções de validação
- Todos os logs incluem contexto (ExecutionId, Queue, Email, etc.)

---

## Configuração

### Filas RabbitMQ

Configurar no `appsettings.json`:

```json
{
    "MessageQueues": {
        "ApiRequestQueue": "api-request-queue",
        "ApiRequestQueueResponse": "api-response-queue"
    }
}
```

### Registrar Serviços

```csharp
// IServiceCollection
services.AddScoped<IApiTemplateServices, ApiTemplateServices>();
services.AddScoped<IApiTemplateRepository, ApiTemplateRepository>();
services.AddScoped<IApiOutputServices, ApiOutputServices>();
services.AddScoped<IToolHandler, ApiHandler>();
services.AddHostedService<ApiOutputConsumer>();
```

---

## Testes

### Testes Unitários

- **ApiHandlerTests**: Testa construção de payloads e substituição de placeholders
- **ApiOutputServicesTests**: Testa processamento de respostas
- **ApiTemplateServicesTests**: Testa CRUD de templates
- **WorkflowServicesTests**: Testa integração com workflows

### Exemplo de Teste

```csharp
[Fact]
public async Task BuildPayload_Should_Replace_Ocr_Placeholder()
{
    // Arrange
    var outputs = new List<StepToolOutput>
    {
        new StepToolOutput
        {
            StepTool = new StepTool { Tool = new Tool { ToolType = new ToolType { Name = "OCR" } } },
            Value = "{\"DocumentEmbeddings\":[{\"Text\":\"Test\"}]}"
        }
    };

    // Act
    var result = await _apiHandler.BuildPayload(automationDto, null, outputs, execution);

    // Assert
    Assert.Contains("Test", result.Message.Body);
}
```

---

## Roadmap / Melhorias Futuras

1. **Retry Automático**: Implementar retry em caso de falhas HTTP
2. **Timeout Configurável**: Permitir definir timeout por template
3. **Autenticação OAuth2**: Suporte para fluxos OAuth2
4. **Rate Limiting**: Controle de taxa de requisições
5. **Cache de Templates**: Cache para templates frequentemente usados
6. **Versionamento de Templates**: Manter histórico de alterações
7. **Validação de Schema**: Validar response contra schema esperado
8. **Hooks**: Pré e pós-processamento customizável

---

## Referências

### Arquivos Principais

- `back-end/WoopiAiHub.Application/ToolsHandler/ApiHandler.cs`
- `back-end/WoopiAiHub.Application/Messaging/ApiOutputConsumer.cs`
- `back-end/WoopiAiHub.Application/Services/Automation/ApiOutputServices.cs`
- `back-end/WoopiAiHub.Application/Services/ApiTemplateServices.cs`
- `back-end/WoopiAiHub.Api/Controllers/ApiTemplateController.cs`
- `back-end/WoopiAiHub.Domain/Models/ApiTemplate.cs`

### Interfaces

- `IToolHandler`
- `IApiTemplateServices`
- `IApiTemplateRepository`
- `IApiOutputServices`
- `IEncryptionService`

### DTOs

- `ApiTemplateDto`
- `ApiTemplateCreateDto`
- `ApiTemplateUpdateDto`
- `ApiRequestDto`
- `ApiOutputDto`
- `AutomationServicesDto`

---

**Versão**: 1.0  
**Data**: Janeiro 2024  
**Autores**: Equipe Woopi AI Hub
