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
    private readonly IJwtTokenServices _jwtTokenServices;
    private readonly OpenAiSettings _openAiSettings;

    public PromptHandler(IOptions<MessageQueues> messageQueues,
                         IPromptServices promptServices,
                         ITenantCacheServices tenantCacheServices,
                         IApiTemplateServices apiTemplateServices,
                         IOptions<OpenAiSettings> openAiSettings,
                         IOptions<McpSettings> mcpSettings,
                         IJwtTokenServices jwtTokenServices)
    {
        _messageQueues = messageQueues.Value;
        _promptServices = promptServices;
        _tenantCacheServices = tenantCacheServices;
        _apiTemplateServices = apiTemplateServices;
        _openAiSettings = openAiSettings.Value;
        _mcpSettings = mcpSettings.Value;
        _jwtTokenServices = jwtTokenServices;
    }

    /// <summary>
    /// Builds an execution payload for processing prompt tasks with multiple outputs from dependent StepTools.
    /// Dependency outputs are merged into user context; configured flows restrict Prompt dependencies to API and Quiz tools.
    /// </summary>
    /// <param name="automationServicesDto"></param>
    /// <param name="input"></param>
    /// <param name="outputs">Collection of outputs from dependent StepTools.</param>
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
            throw new AppException(ErrorCode.RequiredField, "The prompt tool requires an API or Quiz dependency with output", ToolLabel.PromptApiOrQuizDependencyRequired);
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
                Model = tenantInfo!.LlmProvider == LlmProvider.AzureOpenAI ? _openAiSettings.Model : tenantInfo!.Model,
                ApiVersion = _openAiSettings.ApiVersion,
                ApplicationId = tenantInfo!.AiGatewayApplicationId.Value.ToString(),
                ApplicationKey = tenantInfo!.AiGatewayKey,
                OpenAiResponse = dto,
                Email = automationServicesDto.Email,
                LlmProvider = tenantInfo!.LlmProvider
            }
        };
    }

    /// <summary>
    /// Will create the dto to send to Open Ai response tool
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
                 },
            MaxToolCalls = _openAiSettings.MaxToolCalls,
            Temperature = _openAiSettings.Temperature
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

        var accessToken = _jwtTokenServices.GenerateTokenWithParameters(
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
                AllowedTools=["generalist"]
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
            headers = ExtractHeadersValues(api.HeaderTemplate),
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

        return _mcpSettings.Instructions.Replace("{0}", mappedApiString);
    }

    /// <summary>
    /// Method used to convert the headers from api template to a dictionary
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private static Dictionary<string, string> ExtractHeadersValues(string? item)
    {
        if (string.IsNullOrEmpty(item))
        {
            return new Dictionary<string, string>();
        }

        var doc = JsonDocument.Parse(item);

        var dict = new Dictionary<string, string>();

        foreach (var el in doc.RootElement.EnumerateArray())
        {
            var key = el.GetProperty("key").GetString();
            var value = el.GetProperty("value").GetString();

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                continue;
            }

            dict[key] = value;
        }

        return dict;
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
            switch (toolType)
            {
                case string t when string.Equals(t, HandlersTypes.Ocr, StringComparison.OrdinalIgnoreCase):
                    TryAddOcrText(value, parts);
                    break;
                case string t when string.Equals(t, HandlersTypes.Prompt, StringComparison.OrdinalIgnoreCase):
                    parts.Add(value.Trim());
                    break;
                case string t when string.Equals(t, HandlersTypes.Quiz, StringComparison.OrdinalIgnoreCase):
                    parts.Add(value.Trim());
                    break;
                case string t when string.Equals(t, HandlersTypes.API, StringComparison.OrdinalIgnoreCase):
                    parts.Add(FormatApiResponseForPromptContext(value));
                    break;
                default:
                    parts.Add(value.Trim());
                    break;
            }
        }

        return string.Join("\n\n", parts);
    }

    /// <summary>
    /// Normalizes a dependency API response for prompt context. When the value parses as a JSON object, flattens it to a single readable line
    /// (<c>Key: value, ...</c>); otherwise returns the trimmed string unchanged.
    /// </summary>
    /// <param name="value">Raw content from the API step (JSON object or plain text).</param>
    /// <returns>A single-line string for the prompt, or <see cref="string.Empty"/> when <paramref name="value"/> is null or whitespace only.</returns>
    private static string FormatApiResponseForPromptContext(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
                return FormatJsonObjectAsLabelValueString(root);
        }
        catch
        {
            throw new ArgumentException("The API response is not a valid JSON object");
        }

        return trimmed;
    }

    /// <summary>
    /// Builds a comma-separated line from a JSON object: each property is rendered as <c>Capitalized name: formatted value</c> (property order follows the JSON object).
    /// </summary>
    /// <param name="root">A JSON object element.</param>
    /// <returns>Joined <c>Label: value</c> segments, or the literal <c>{}</c> when the object has no properties.</returns>
    private static string FormatJsonObjectAsLabelValueString(JsonElement root)
    {
        var segments = new List<string>();
        foreach (var prop in root.EnumerateObject())
        {
            if (string.IsNullOrEmpty(prop.Name))
                continue;
            var label = char.ToUpperInvariant(prop.Name[0]) + (prop.Name.Length > 1 ? prop.Name[1..] : string.Empty);
            segments.Add($"{label}: {FormatJsonValueForApiDisplay(prop.Value)}");
        }
        if (segments.Count == 0)
            return "{}";
        return string.Join(", ", segments);
    }

    /// <summary>
    /// Renders a JSON value for the prompt line: strings are quoted, primitives use JSON literals, arrays are joined recursively, and nested objects use the raw JSON fragment.
    /// </summary>
    /// <param name="el">A JSON value (string, number, array, object, or literal).</param>
    /// <returns>Text to place after a property name in the flattened API output.</returns>
    private static string FormatJsonValueForApiDisplay(JsonElement el) =>
        el.ValueKind switch
        {
            JsonValueKind.String => QuotedStringForDisplay(el.GetString() ?? string.Empty),
            JsonValueKind.Number => el.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            JsonValueKind.Array => string.Join(", ", el.EnumerateArray().Select(FormatJsonValueForApiDisplay)),
            JsonValueKind.Object => el.GetRawText(),
            JsonValueKind.Undefined => string.Empty,
            _ => el.GetRawText(),
        };

    /// <summary>
    /// Wraps a string in double quotes and escapes <c>\</c> and <c>"</c> so the result is safe to embed in the flattened key/value line.
    /// </summary>
    /// <param name="s">The string to quote.</param>
    /// <returns><paramref name="s"/> as a double-quoted literal segment.</returns>
    private static string QuotedStringForDisplay(string s) =>
        "\"" + s.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

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
