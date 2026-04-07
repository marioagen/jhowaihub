using System.Text.Json;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.DTOs.Response.OpenAiResponses;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
namespace WoopiAiHub.Application.ToolsHandler;

public class PromptHandler : IToolHandler
{
    public string Type => HandlersTypes.Prompt;
    private readonly MessageQueues _messageQueues;
    private readonly IPromptServices _promptServices;
    private readonly ITenantCacheServices _tenantCacheServices;
    private readonly ResponseOpenAiSettings _responseOpenAiSettings;
    private readonly IApiTemplateServices _apiTemplateServices;
    private readonly IAccountServices _accountServices;

    public PromptHandler(IOptions<MessageQueues> messageQueues,
                         IPromptServices promptServices,
                         ITenantCacheServices tenantCacheServices,
                         IOptions<ResponseOpenAiSettings> responseOpenAiSettings,
                         IApiTemplateServices apiTemplateServices,
                         IAccountServices accountServices)
    {
        _messageQueues = messageQueues.Value;
        _promptServices = promptServices;
        _tenantCacheServices = tenantCacheServices;
        _responseOpenAiSettings = responseOpenAiSettings.Value;
        _apiTemplateServices = apiTemplateServices;
        _accountServices = accountServices;
    }

    /// <summary>
    /// Builds an execution payload for processing prompt tasks with multiple outputs from dependent StepTools.
    /// Dependencies can be OCR (document embeddings) or another Prompt (previous prompt response); the combined text is sent to the AI Gateway.
    /// </summary>
    /// <param name="automationServicesDto"></param>
    /// <param name="input"></param>
    /// <param name="outputs">Collection of outputs from dependent StepTools (OCR and/or Prompt)</param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                        StepToolParameter? input,
                                                        ICollection<StepToolOutput> outputs,
                                                        StepToolExecution? execution = null)
    {
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(automationServicesDto.Tenant);
        if (tenantInfo!.AiGatewayApplicationId.HasValue is false || string.IsNullOrEmpty(tenantInfo.AiGatewayKey))
        {
            throw new ArgumentException("AiGateway ApplicationId not found");
        }

        var fullText = ExtractFullTextFromOutputs(outputs);
        if (string.IsNullOrWhiteSpace(fullText))
        {
            throw new AppException(ErrorCode.RequiredField, "The prompt tool requires an OCR or Prompt dependency with output", ToolLabel.OcrOrPromptDependencyRequired);
        }

        var promptId = int.Parse(input!.Value);
        var promptDto = _promptServices.FindById(promptId);

        ResponseOpenAiRequestDto dto = await generateTheOpenAiResponseRequestDto(promptDto, fullText);

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.OpenAiResponseQueue,
            Message = new OpenAiResponseQueryDto
            {
                ResponseQueue = _messageQueues.OpenAiResponseQueueAiHubResponse,
                Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
                ReferenceFile = automationServicesDto.ReferenceFile!,
                Tenant = automationServicesDto.Tenant,
                Model = _responseOpenAiSettings.Model,
                ApiVersion = _responseOpenAiSettings.ApiVersion,
                ApplicationId = tenantInfo!.AiGatewayApplicationId.Value.ToString(),
                ApplicationKey = tenantInfo!.AiGatewayKey,
                OpenAiResponse = dto,
                Email = automationServicesDto.Email
            }
        };
    }

    private async Task<ResponseOpenAiRequestDto> generateTheOpenAiResponseRequestDto(PromptDto? promptDto, string fullText)
    {

        var dto = new ResponseOpenAiRequestDto
        {
            Model = "gpt-4.1",
            Input = new List<ResponseOpenAiRequestInputDto> {
                    new ResponseOpenAiRequestInputDto {
                        Type = OpenAiResponsesTypes.Message,
                        Role = OpenAiResponseInputRole.User,
                        Content = new List<ResponseOpenAiRequestInputContentDto> {
                            new ResponseOpenAiRequestInputContentDto {
                                Type = OpenAiResponseInputContentType.InputText,
                                Text = string.Concat("Baseado no: \"", fullText, "\" e seguindo as orientações a seguir: ", promptDto!.Text)
                            }
                        }
                    }
                 }
        };

        await VerifyAndAddOrNotMcpSupport(promptDto, dto);

        return dto;
    }

    private async Task VerifyAndAddOrNotMcpSupport(PromptDto promptDto, ResponseOpenAiRequestDto dto)
    {
        if (!promptDto.EnableAccessToMcp)
            return;

        var apis = await _apiTemplateServices.FindAll(new ApiTemplateFilterDto() { EnableAccessFromMcp = true, PromptId = promptDto.Id });

        var mappedApis = apis.Select(api => new
        {
            id = api.Id,
            address = api.Url,
            protocol = api.Method switch 
            {
                "GET" => 0,
                "POST" => 1, 
                "PUT" => 2,
                _ => 3
            },
            description = api.Description,
            payload_schema =  api.Method switch 
            {
                "GET" => null,
                _ => $"PAYLOAD_API_{api.Id}"
            },
        });

        var mappedApiString = System.Text.Json.JsonSerializer.Serialize(mappedApis);

        foreach (var item in apis.Where(a => a.Method != "GET"))
        {
            mappedApiString = mappedApiString.Replace($"PAYLOAD_API_{item.Id}", System.Text.Json.JsonSerializer.Serialize(JsonDocument.Parse(item.BodyTemplate).RootElement));
        }

        var accessToken = _accountServices.GenerateToken("MCP_SERVER", 5);

        var instructions = string.IsNullOrEmpty(mappedApiString) ? "" : @"
Para atender o prompt acima  use a tool generalista. e siga as instruções abaixo.
Assinatura: generalista(request: GeneralistaRequestDTO).
a estrutura do GeneralistaRequestDTO é a seguinte:
{
Protocolo: GeneralistaProtocolMetodo, // O protocolo define a estrutura do payload e o endpoint a ser chamado.
BaseRequestURL: string, // URL usado na request do MCP, ex: 'https://localhost:7115/api/usuario/' ou 'https://localhost:7115/api/produto/'
RequestData: string, // dados em json em formato de string, que serão enviados no corpo da requisição para o endpoint definido pelo protocolo.
}

GeneralistaProtocolMetodo {
    GET=0,
    POST=1,
    PUT=2,
    DELETE=3
}

com a estrutura definida 

CATALOGO_DE_ENDPOINTS: " + mappedApiString + @"


REGRAS DE ROTEAMENTO:
- Se a resposta depender de dados externos, chame generalista antes de responder.
- Escolha o address somente se estiver presente no CATALOGO_DE_ENDPOINTS, sem inventar.
- Monte payload_json como string JSON válida, conforme payload_schema do endpoint escolhido.
- Se mais de um endpoint servir, escolha o de maior especificidade e menor número de campos.
- Analise se a URL não possui parametros customiavies, os paraetros serão reconhecidos pela presença de chaves ({}), caso exista parametros no endereço realize a substitução pelo valor devido.
- Para parametros na URL caso o valor possua algum simbolo entre os cochetes [.,/:;] rever os simolos. Ex: cpf 000.000.000-00 no parametro da URL deverá passar 00000000000. Isso só deve ser aplicado aos parametros de URL, se o dado pesquisador pode permanecer com os caracteres se forem enviados em consultas dentro do corpo da request
- Limite de chamadas: no máximo 10.

REGRAS DE RESPOSTA:
- Não sugerir ações após o rultado
- Não indicar como os dados foram obtidos apenas apresentar os dados
                ";

        dto.Instructions = instructions;
        dto.MaxToolCalls = 10;
        dto.Tools = new List<ResponseOpenAiRequestToolsDto> {
            new ResponseOpenAiRequestToolsDto {
                Type = OpenAiResponseToolsType.Mcp,
                ServerLabel = "dmcp",
                ServerUrl=_responseOpenAiSettings.McpAddress,
                Headers= new Dictionary<string, string>{
                        {"x-session-id", "d354301e-6b4b-4a3f-beef-1f9715dd2dfd"},
                        {"x-api-key", "d354301e-6b4b-4a3f-beef-1f9715dd2dfd"},
                        {"Authorization", $"Bearer {accessToken}"}
                    },
                RequireApproval="never",
                AllowedTools=["generalista"]
            }
        };
}

    /// <summary>
    /// Extracts and concatenates text from dependency outputs. OCR output is parsed from DocumentEmbeddings; Prompt output is used as plain text.
    /// When StepTool/ToolType is not available (e.g. tests), tries OCR format first, then falls back to plain text.
    /// </summary>
    private static string ExtractFullTextFromOutputs(ICollection<StepToolOutput> outputs)
    {
        if (outputs == null || outputs.Count == 0) return string.Empty;

        var parts = new List<string>();
        foreach (var output in outputs)
        {
            var value = output.Value;
            if (string.IsNullOrWhiteSpace(value)) continue;

            var toolType = output.StepTool?.Tool?.ToolType?.Name;
            if (string.Equals(toolType, HandlersTypes.Ocr, StringComparison.OrdinalIgnoreCase))
            {
                TryAddOcrText(value, parts);
            }
            else if (string.Equals(toolType, HandlersTypes.Prompt, StringComparison.OrdinalIgnoreCase))
            {
                parts.Add(value.Trim());
            }
        }

        return string.Join("\n\n", parts);
    }

    /// <summary>
    /// Tries to parse OCR output in DocumentEmbeddings format and extract text. If parsing fails, returns false to allow fallback to plain text.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parts"></param>
    /// <returns></returns>
    private static bool TryAddOcrText(string value, List<string> parts)
    {
       var documents = JsonConvert.DeserializeObject<DocumentEmbeddingsDataDto>(value);
       if (documents?.DocumentEmbeddings != null && documents.DocumentEmbeddings.Count > 0)
       {
           parts.Add(string.Join("\n", documents.DocumentEmbeddings.Select(d => d.Text)));
           return true;
       }
        return false;
    }
}
