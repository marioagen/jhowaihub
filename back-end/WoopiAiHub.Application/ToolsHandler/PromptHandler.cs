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
    private readonly McpSettings _mcpSettings;
    private readonly IApiTemplateServices _apiTemplateServices;
    private readonly IAccountServices _accountServices;
    private readonly OpenAiSettings _openAiSettings;

    public PromptHandler(IOptions<MessageQueues> messageQueues,
                         IPromptServices promptServices,
                         ITenantCacheServices tenantCacheServices,
                         IApiTemplateServices apiTemplateServices,
                         IAccountServices accountServices,
                         IOptions<OpenAiSettings> openAiSettings,
                         IOptions<McpSettings> mcpSettings)
    {
        _messageQueues = messageQueues.Value;
        _promptServices = promptServices;
        _tenantCacheServices = tenantCacheServices;
        _apiTemplateServices = apiTemplateServices;
        _accountServices = accountServices;
        _openAiSettings = openAiSettings.Value;
        _mcpSettings = mcpSettings.Value;
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

        ResponseOpenAiRequestDto dto = await GenerateOpenAiResponseRequestDto(promptDto, fullText);

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.OpenAiResponseQueue,
            Message = new OpenAiResponseQueryDto
            {
                ResponseQueue = _messageQueues.OpenAiResponseQueueAiHubResponse,
                Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
                ReferenceFile = automationServicesDto.ReferenceFile!,
                Tenant = automationServicesDto.Tenant,
                Model = _openAiSettings.Model,
                ApiVersion = _openAiSettings.ApiVersion,
                ApplicationId = tenantInfo!.AiGatewayApplicationId.Value.ToString(),
                ApplicationKey = tenantInfo!.AiGatewayKey,
                OpenAiResponse = dto,
                Email = automationServicesDto.Email
            }
        };
    }

    /// <summary>
    /// ill create the dto to send to Open Ai response tool
    /// </summary>
    /// <param name="promptDto"></param>
    /// <param name="fullText"></param>
    /// <returns></returns>
    private async Task<ResponseOpenAiRequestDto> GenerateOpenAiResponseRequestDto(PromptDto? promptDto, string fullText)
    {
        var dto = new ResponseOpenAiRequestDto
        {
            Model = _openAiSettings.Model,
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

    /// <summary>
    /// method to check if the selected prompt has the MCP flag checked and validate if all the necessary data is
    /// available to create the instructions and mcp connection to OpenAi response tool 
    /// </summary>
    /// <param name="promptDto"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task VerifyAndAddOrNotMcpSupport(PromptDto promptDto, ResponseOpenAiRequestDto dto)
    {
        if (!promptDto.EnableAccessToMcp)
            return;

        if (string.IsNullOrEmpty(_mcpSettings.Instructions))
            throw new ArgumentException("The agent with a external access enabled need has the instructions filled in the appSettings");

        var instructions = await GenerateInstructionsWithMappedApisToAgent(promptDto);

        var accessToken = _accountServices.GenerateTokenWithParameters(
            _mcpSettings.JWTKey,
            _mcpSettings.JWTIssuer,
            _mcpSettings.JWTAudience,
            _mcpSettings.JWTUser,
            _mcpSettings.JWTExpirationTime);

        dto.Instructions = instructions;
        dto.MaxToolCalls = _mcpSettings.MaxToolCalls;
        dto.Tools = new List<ResponseOpenAiRequestToolsDto> {
            new ResponseOpenAiRequestToolsDto {
                Type = OpenAiResponseToolsType.Mcp,
                ServerLabel = "dmcp",
                ServerUrl= _mcpSettings.McpAddress,
                Headers= new Dictionary<string, string> {
                        {"Authorization", $"Bearer {accessToken}"}
                    },
                RequireApproval="never",
                AllowedTools=["generalista"]
            }
        };
    }

    /// <summary>
    /// Method to map the api templates linked to a prompt to the instruction property
    /// </summary>
    /// <param name="promptDto"></param>
    /// <returns></returns>
    private async Task<string> GenerateInstructionsWithMappedApisToAgent(PromptDto promptDto)
    {
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
            headers = api.HeaderTemplate,
            payload_schema = api.Method switch
            {
                "GET" => null,
                _ => $"PAYLOAD_API_{api.Id}"
            },
        });

        var mappedApiString = System.Text.Json.JsonSerializer.Serialize(mappedApis);

        foreach (var item in apis.Where(a => a.Method != "GET"))
        {
            var bodyContent = string.IsNullOrEmpty(item.BodyTemplate) ? "{}" : item.BodyTemplate;
            mappedApiString = mappedApiString.Replace($"PAYLOAD_API_{item.Id}", System.Text.Json.JsonSerializer.Serialize(JsonDocument.Parse(bodyContent).RootElement));
        }

        var instructions = string.IsNullOrEmpty(mappedApiString) ? "" : _mcpSettings.Instructions.Replace("{0}", mappedApiString);
        return instructions;
    }

    /// <summary>
    /// Extracts and concatenates text from dependency outputs. OCR output is parsed from DocumentEmbeddings; Prompt output is used as plain text.
    /// When StepTool/ToolType is not available (e.g. tests), tries OCR format first, then falls back to plain text.
    /// </summary>
    private static string ExtractFullTextFromOutputs(ICollection<StepToolOutput> outputs)
    {
        if (outputs == null || outputs.Count == 0)
            return string.Empty;

        var parts = new List<string>();
        foreach (var output in outputs)
        {
            var value = output.Value;
            if (string.IsNullOrWhiteSpace(value))
                continue;

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
