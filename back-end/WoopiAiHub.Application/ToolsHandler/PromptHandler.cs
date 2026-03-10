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
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
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
    private readonly ChatCompletionSettings _chatCompletionSettings;
    private readonly IResponseApi _responseApi;
    private readonly ResponseOpenAiSettings _responseOpenAiSettings;
    private readonly IApiTemplateServices _apiTemplateServices;

    public PromptHandler(IOptions<MessageQueues> messageQueues,
                         IPromptServices promptServices,
                         IOptions<ChatCompletionSettings> chatCompletionSettings,
                         ITenantCacheServices tenantCacheServices,
                         IOptions<ResponseOpenAiSettings> responseOpenAiSettings,
                         IResponseApi responseApi,
                         IApiTemplateServices apiTemplateServices)
    {
        _messageQueues = messageQueues.Value;
        _promptServices = promptServices;
        _chatCompletionSettings = chatCompletionSettings.Value;
        _tenantCacheServices = tenantCacheServices;
        _responseApi = responseApi;
        _responseOpenAiSettings = responseOpenAiSettings.Value;
        _apiTemplateServices = apiTemplateServices;
    }

    /// <summary>
    /// Builds an execution payload for processing prompt tasks with multiple outputs from dependent StepTools.
    /// This allows combining multiple document embeddings from different sources.
    /// </summary>
    /// <param name="automationServicesDto"></param>
    /// <param name="input"></param>
    /// <param name="outputs">Collection of outputs from dependent StepTools</param>
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

        var output = outputs.FirstOrDefault()?.Value ?? throw new AppException(ErrorCode.RequiredField, "The prompt tool requires a OCR dependency", ToolLabel.OcrDependencyRequired);
        var promptId = int.Parse(input!.Value);
        var promptDto = _promptServices.FindById(promptId);
        var documents = JsonConvert.DeserializeObject<DocumentEmbeddingsDataDto>(output);
        var fullText = string.Join("\n", documents!.DocumentEmbeddings.Select(d => d.Text));


        var apis = await _apiTemplateServices.FindAll(new ApiTemplateFilterDto());

        var mappedApis = apis.Select(api => new
        {
            id = api.Id,
            address = api.Url,
            protocol = api.Method == "GET" ? 0 : api.Method == "POST" ? 1 : api.Method == "PUT" ? 2 : 3,
            description = api.Description,
            // payload_schema = System.Text.Json.JsonSerializer.Serialize(JsonDocument.Parse(api.BodyTemplate).RootElement)
            payload_schema = $"PAYLOAD_API_{api.Id}"
        });

        var mappedApiString = System.Text.Json.JsonSerializer.Serialize(mappedApis);

        foreach (var item in apis)
        {
            mappedApiString = mappedApiString.Replace($"PAYLOAD_API_{item.Id}", System.Text.Json.JsonSerializer.Serialize(JsonDocument.Parse(item.BodyTemplate).RootElement));
        }

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
- Escolha address somente do CATALOGO_DE_ENDPOINTS, sem inventar.
- Monte payload_json como string JSON válida conforme payload_schema do endpoint escolhido.
- Se mais de um endpoint servir, escolha o de maior especificidade e menor número de campos.
- Analise se a URL não possui parametros customiavies, os paraetros serão reconhecidos pela presença de chaves ({}), caso exista parametros no endereço realize a substitução pelo valor devido.
- Para parametros na URL caso o valor possua algum simbolo entre os cochetes [.,/:;] rever os simolos. Ex: cpf 000.000.000-00 no parametro da URL deverá passar 00000000000
- Limite de chamadas: no máximo 2.

REGRAS DE RESPOSTA:
- Não sugerir ações após o rultado
- Não indicar como os dados foram obtidos apenas apresentar os dados
                    ";
        var dto = new ResponseOpenAiRequestDto
        {
            // Temperature = _responseOpenAiSettings.Temperature,
            Instructions = instructions,
            Model = "gpt-4.1",
            MaxToolCalls = 2,
            Tools = new List<ResponseOpenAiRequestToolsDto> {
                new ResponseOpenAiRequestToolsDto {
                    Type = OpenAiResponseToolsType.Mcp,
                    ServerLabel = "dmcp",
                    ServerUrl=_responseOpenAiSettings.McpAddress,
                    Headers= new Dictionary<string, string>{
                            {"x-session-id", "d354301e-6b4b-4a3f-beef-1f9715dd2dfd"},
                            {"x-api-key", "d354301e-6b4b-4a3f-beef-1f9715dd2dfd"}
                        },
                    RequireApproval="never",
                    AllowedTools=["generalista"]
                }
            },
            Input = new List<ResponseOpenAiRequestInputDto> {
                    new ResponseOpenAiRequestInputDto {
                        Type = OpenAiResponsesTypes.Message,
                        Role = OpenAiResponseInputRole.User,
                        Content = new List<ResponseOpenAiRequestInputContentDto> {
                            new ResponseOpenAiRequestInputContentDto {
                                Type = OpenAiResponseInputContentType.InputText,
                                // Text = "busque dados do usuário com o cpf 123.456.789-00"
                                Text = string.Concat("Baseado no: \"", fullText, "\" e seguindo as orientações a seguir: ", promptDto!.Text)
                            }
                        }
                    }
                 }
        };

        // try
        // {
        //     var response2 = await _responseApi.GetResponseOpenAi(
        //         // tenantInfo.AiGatewayApplicationId.Value.ToString(),
        //         "85032382-3b50-4b4c-e757-08de641e689e",
        //         "gpt-4.1",
        //         // _responseOpenAiSettings.ApiVersion,
        //         // Guid.NewGuid().ToString(),
        //         "d354301e-6b4b-4a3f-beef-1f9715dd2dfd",
        //         // tenantInfo.AiGatewayKey,
        //         "5759793d457c3554be2a4269d0a0c3e5a52d6668201c05099829b993aacbfe8",
        //         dto);
        // }
        // catch (Exception ex)
        // {
        //     // TODO
        //     System.Console.WriteLine(ex.Message);
        // }

        var x = new ExecutionMessageDto
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
        return x;
        // return new ExecutionMessageDto
        // {
        //     Queue = _messageQueues.ChatCompletionQueue,
        //     Message = new ChatCompletionQueryDto
        //     {
        //         ResponseQueue = _messageQueues.ChatCompletionQueueAiHubResponse,
        //         Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
        //         ReferenceFile = automationServicesDto.ReferenceFile!,
        //         Tenant = automationServicesDto.Tenant,
        //         Model = _chatCompletionSettings.Model,
        //         ApiVersion = _chatCompletionSettings.ApiVersion,
        //         ApplicationId = tenantInfo!.AiGatewayApplicationId.Value.ToString(),
        //         ApplicationKey = tenantInfo!.AiGatewayKey,
        //         ChatCompletion = new ChatCompletionDto
        //         {
        //             Temperature = _chatCompletionSettings.Temperature,
        //             MaxTokens = _chatCompletionSettings.MaxTokens,
        //             Messages = new List<ChatMessageDto>
        //                 {
        //                     new ChatMessageDto
        //                     {
        //                         Role = "system",
        //                         Content = string.Concat("Baseado no: \"", fullText, "\" e seguindo as orientações a seguir: ", promptDto!.Text)
        //                     }
        //                 }
        //         },
        //         Email = automationServicesDto.Email
        //     }
        // };
    }
}