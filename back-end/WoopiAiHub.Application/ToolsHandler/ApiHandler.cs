using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.Application.ToolsHandler
{
    public class ApiHandler(IOptions<MessageQueues> messageQueues,
        IStepToolRepository stepToolRepository,
        IApiTemplateRepository apiTemplateRepository,
        IEncryptionService encryptationService) : IToolHandler
    {
        public string Type => HandlersTypes.API;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        private readonly MessageQueues _messageQueues = messageQueues.Value;
        private readonly IStepToolRepository _stepToolRepository = stepToolRepository;
        private readonly IApiTemplateRepository _apiTemplateRepository = apiTemplateRepository;
        private readonly IEncryptionService _encryptationService = encryptationService;

        /// <summary>
        /// Builds an execution payload message for the specified automation service and step tool parameters.
        /// </summary>
        /// <param name="automationServicesDto">The automation service data transfer object containing information about the automation context and the step
        /// tool to be executed. Cannot be null.</param>
        /// <param name="input">An optional input parameter for the step tool. May be null if the step tool does not require input.</param>
        /// <param name="outputs">A collection of expected outputs for the step tool execution. Cannot be null.</param>
        /// <param name="execution">An optional execution context for the step tool. If not provided, the default execution context is used.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see
        /// cref="ExecutionMessageDto"/> representing the constructed execution payload message.</returns>
        /// <exception cref="AppException">Thrown if the step tool specified in <paramref name="automationServicesDto"/> cannot be found.</exception>
        public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                            StepToolParameter? input,
                                                            ICollection<StepToolOutput> outputs,
                                                            StepToolExecution? execution = null)
        {
            var stepTool = await _stepToolRepository.FindById(automationServicesDto!.StepToolId)
                ?? throw new AppException(ErrorCode.NotFound, "StepTool not found", null);

            var request = await BuildApiRequest(stepTool, automationServicesDto, outputs, execution?.Id);

            return new ExecutionMessageDto
            {
                Queue = _messageQueues.ApiRequestQueue,
                Message = request
            };
        }

        /// <summary>
        /// Builds an API request object from the specified step tool parameters.
        /// The method validates that the step tool is configured with the correct tool type and contains the necessary parameters for constructing the API request.
        /// If the validation fails, an <see cref="AppException"/> is thrown with an appropriate error message.
        /// </summary>
        /// <param name="stepTool"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task<ApiRequestDto> BuildApiRequest(
            StepToolDto stepTool,
            AutomationServicesDto automation,
            ICollection<StepToolOutput> outputs,
            int? executionId)
        {
            if (stepTool.Tool?.ToolType != HandlersTypes.API)
            {
                throw new AppException(ErrorCode.InvalidValue, "Invalid tool type for API handler", null);
            }

            if (stepTool.Parameters.Count <= 0)
            {
                throw new AppException(ErrorCode.NotFound, "No API was found configured for the specified step tool.", null);
            }

            var param = stepTool.Parameters.First().Value;
            var requestStr = _encryptationService.Decrypt(param);

            var request = JsonSerializer.Deserialize<ApiRequestDto>(requestStr, _jsonOptions)
                ?? throw new AppException(ErrorCode.InvalidValue, "Invalid API request configuration", null);

            var template = await _apiTemplateRepository.FindById(request.TemplateId) ?? throw new AppException(ErrorCode.NotFound, "API template not found", null);

            request.Body = AddReferenceFileToBody(request.Body, automation.ReferenceFile ?? string.Empty);
            request.Body = ConvertOutputsToJson(outputs, request.Body);
            request.Url = ConvertOutputsToUrl(outputs, request.Url);
            request.Email = automation.Email;
            request.Tenant = automation.Tenant;
            request.Data = new MetaDataAutomationDto(automation.CardId, automation.StepToolId);
            request.ResponseQueue = _messageQueues.ApiRequestQueueResponse;
            request.ExecutionId = executionId ?? throw new AppException(ErrorCode.InvalidValue, "ExecutionId not defined", null);
            request.TemplateName = template.Name;

            return request;
        }

        /// <summary>
        /// Add reference file information to the body of the API request. If the body is null or empty, a new JSON object is created to hold the reference file information.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="referenceFile"></param>
        /// <returns></returns>
        private static string AddReferenceFileToBody(string? body, string referenceFile)
        {
            if (string.IsNullOrEmpty(body))
            {
                return string.Concat("{ \"referenceFile\": \"", referenceFile, "\" }");
            }

            return string.Concat(
                "{ \"referenceFile\": \"",
                referenceFile,
                "\", ",
                body.AsSpan(1)
            );
        }

        /// <summary>
        /// Replaces specific placeholders in the input string with corresponding values from the provided tool outputs
        /// and returns the resulting JSON string.
        /// </summary>
        /// <remarks>Placeholders are replaced in a case-insensitive manner. Only outputs with tool types
        /// of OCR, Prompt, N8N, API, or Quiz are processed; other tool types are ignored. Embeddings is explicitly excluded.</remarks>
        /// <param name="outputs">A collection of tool output objects whose values are used to replace placeholders in the input string. Only
        /// outputs with recognized tool types are considered.</param>
        /// <param name="inputValue">The input string containing placeholders to be replaced with output values. Placeholders must match the
        /// expected format (e.g., "{{ocr}}", "{{prompt}}", "{{n8n}}", "{{api}}", or "{{quiz}}").</param>
        /// <returns>A string representing the input value with recognized placeholders replaced by the corresponding output
        /// values. If no placeholders are matched, the original input string is returned.</returns>
        private string ConvertOutputsToJson(ICollection<StepToolOutput> outputs, string inputValue)
        {
            if (string.IsNullOrEmpty(inputValue)) return inputValue;

            try
            {
                var root = JsonNode.Parse(inputValue);
                if (root is JsonValue jv && jv.TryGetValue<string>(out var rootString))
                {
                    var replaced = SubstituteInStructuredString(rootString, outputs);
                    return JsonSerializer.Serialize(replaced, _jsonOptions);
                }

                SubstitutePlaceholdersInJsonNodes(root, outputs);
                return root.ToJsonString(_jsonOptions);
            }
            catch (JsonException)
            {
                return ConvertOutputsToJsonLegacy(outputs, inputValue);
            }
        }

        /// <summary>
        /// Legacy path for bodies that are not valid JSON until placeholders are replaced (e.g. <c>{"text": {{prompt}}}</c>).
        /// </summary>
        private string ConvertOutputsToJsonLegacy(ICollection<StepToolOutput> outputs, string inputValue)
        {
            var result = inputValue;
            var groups = outputs
                .Where(o => o.StepTool?.Tool?.ToolType != null)
                .GroupBy(o => o.StepTool!.Tool!.ToolType!.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                if (!TryGetToolConfig(group.Key, out var placeholder, out var isJsonNode)) continue;
                if (!result.Contains(placeholder, StringComparison.OrdinalIgnoreCase)) continue;

                var processedValues = group
                    .Select(o => ProcessOutputValue(group.Key, o.Value, isJsonNode))
                    .ToList();

                var replacement = BuildReplacementString(processedValues, isJsonNode);
                result = result.Replace(placeholder, replacement, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        private void SubstitutePlaceholdersInJsonNodes(JsonNode? node, ICollection<StepToolOutput> outputs)
        {
            switch (node)
            {
                case JsonObject obj:
                    foreach (var kvp in obj.ToList())
                    {
                        var key = kvp.Key;
                        var child = kvp.Value;
                        if (child is JsonValue jv && jv.TryGetValue<string>(out var str))
                        {
                            var newStr = SubstituteInStructuredString(str, outputs);
                            if (newStr != str)
                                obj[key] = JsonValue.Create(newStr);
                        }
                        else
                            SubstitutePlaceholdersInJsonNodes(child, outputs);
                    }
                    break;
                case JsonArray arr:
                    for (var i = 0; i < arr.Count; i++)
                    {
                        var child = arr[i];
                        if (child is JsonValue jv && jv.TryGetValue<string>(out var str))
                        {
                            var newStr = SubstituteInStructuredString(str, outputs);
                            if (newStr != str)
                                arr[i] = JsonValue.Create(newStr);
                        }
                        else
                            SubstitutePlaceholdersInJsonNodes(child, outputs);
                    }
                    break;
            }
        }

        private string SubstituteInStructuredString(string input, ICollection<StepToolOutput> outputs)
        {
            var result = input;
            var groups = outputs
                .Where(o => o.StepTool?.Tool?.ToolType != null)
                .GroupBy(o => o.StepTool!.Tool!.ToolType!.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                if (!TryGetToolConfig(group.Key, out var placeholder, out _)) continue;
                if (!result.Contains(placeholder, StringComparison.OrdinalIgnoreCase)) continue;

                var fragment = BuildStructuredFragmentReplacement(group);
                result = result.Replace(placeholder, fragment, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        /// <summary>
        /// Builds replacement text for placeholders inside an existing JSON string value. Single Prompt/OCR values are raw text
        /// so the final serializer can escape; other cases mirror legacy JSON-token composition.
        /// </summary>
        private string BuildStructuredFragmentReplacement(IGrouping<string, StepToolOutput> group)
        {
            var toolType = group.Key;
            if (!TryGetToolConfig(toolType, out _, out var isJsonNode))
                return string.Empty;

            var outputsList = group.ToList();
            if (outputsList.Count == 0)
                return isJsonNode ? "null" : string.Empty;

            if (outputsList.Count == 1 && IsPlainTextToolTypeForStructuredSingle(toolType))
                return GetRawSingleOutputText(toolType, outputsList[0].Value);

            var processedValues = outputsList
                .Select(o => ProcessOutputValue(toolType, o.Value, isJsonNode))
                .ToList();

            return BuildReplacementString(processedValues, isJsonNode);
        }

        private static bool IsPlainTextToolTypeForStructuredSingle(string toolType) =>
            string.Equals(toolType, HandlersTypes.Ocr, StringComparison.OrdinalIgnoreCase)
            || string.Equals(toolType, HandlersTypes.Prompt, StringComparison.OrdinalIgnoreCase);

        private string GetRawSingleOutputText(string toolType, string? rawValue)
        {
            if (string.Equals(toolType, HandlersTypes.Ocr, StringComparison.OrdinalIgnoreCase))
                return ExtractOcrText(rawValue ?? string.Empty);
            return rawValue ?? string.Empty;
        }

        /// <summary>
        /// Replaces specific placeholders in a URL string with URL-encoded plain text values from the provided tool outputs.
        /// </summary>
        /// <remarks>Only OCR and Prompt tool types are processed for URL replacement; JSON-node types (N8N, API, Quiz)
        /// are skipped as they are not suitable for URL embedding. Values are URL-encoded using <see cref="Uri.EscapeDataString"/>.</remarks>
        /// <param name="outputs">A collection of tool output objects whose values are used to replace placeholders in the URL.</param>
        /// <param name="url">The URL string containing placeholders to be replaced (e.g., "{{prompt}}", "{{ocr}}").</param>
        /// <returns>The URL with recognized placeholders replaced by URL-encoded output values. If no placeholders are matched, the original URL is returned.</returns>
        private const int MaxUrlLength = 2048;

        private string ConvertOutputsToUrl(ICollection<StepToolOutput> outputs, string url)
        {
            if (string.IsNullOrEmpty(url)) return url;

            var result = url;
            var groups = outputs
                .Where(o => o.StepTool?.Tool?.ToolType != null)
                .GroupBy(o => o.StepTool!.Tool!.ToolType!.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                if (!TryGetToolConfig(group.Key, out var placeholder, out var isJsonNode)) continue;
                if (isJsonNode) continue;
                if (!result.Contains(placeholder, StringComparison.OrdinalIgnoreCase)) continue;

                var processedValues = group
                    .Select(o => Uri.EscapeDataString(ExtractPlainTextValue(group.Key, o.Value)))
                    .ToList();

                var replacement = processedValues.Count switch
                {
                    0 => string.Empty,
                    1 => processedValues[0],
                    _ => string.Join(",", processedValues)
                };

                result = result.Replace(placeholder, replacement, StringComparison.OrdinalIgnoreCase);
            }

            if (result.Length > MaxUrlLength)
                throw new AppException(
                    ErrorCode.InvalidValue,
                    $"The prompt response is too long to be used in a URL (current size: {result.Length} characters, limit: {MaxUrlLength}). Please refine the prompt to get a shorter response.",
                    "workflow.errors.urlTooLong");

            return result;
        }

        /// <summary>
        /// Extracts a plain-text value from a raw output, without JSON serialization.
        /// Used for URL context where values must be plain text before encoding.
        /// </summary>
        private string ExtractPlainTextValue(string toolType, string? rawValue)
        {
            var raw = rawValue ?? string.Empty;

            if (string.Equals(toolType, HandlersTypes.Ocr, StringComparison.OrdinalIgnoreCase))
                return ExtractOcrText(raw);

            return raw;
        }

        /// <summary>
        /// Tries to get the placeholder string and JSON node flag for a given tool type.
        /// </summary>
        /// <param name="toolType">The tool type to get the placeholder and JSON node flag for.</param>
        /// <param name="placeholder">The placeholder string for the given tool type.</param>
        /// <param name="isJsonNode">A boolean indicating whether the given tool type is a JSON node.</param>
        /// <returns>True if the tool type is found, false otherwise.</returns>
        private static bool TryGetToolConfig(string toolType, out string placeholder, out bool isJsonNode)
        {
            isJsonNode = false;
            if (string.Equals(toolType, HandlersTypes.Ocr, StringComparison.OrdinalIgnoreCase))
            {
                placeholder = "{{ocr}}";
                return true;
            }
            if (string.Equals(toolType, HandlersTypes.Prompt, StringComparison.OrdinalIgnoreCase))
            {
                placeholder = "{{prompt}}";
                return true;
            }
            if (string.Equals(toolType, HandlersTypes.N8N, StringComparison.OrdinalIgnoreCase))
            {
                placeholder = "{{n8n}}";
                isJsonNode = true;
                return true;
            }
            if (string.Equals(toolType, HandlersTypes.API, StringComparison.OrdinalIgnoreCase))
            {
                placeholder = "{{api}}";
                isJsonNode = true;
                return true;
            }
            if (string.Equals(toolType, HandlersTypes.Quiz, StringComparison.OrdinalIgnoreCase))
            {
                placeholder = "{{quiz}}";
                isJsonNode = true;
                return true;
            }
            placeholder = string.Empty;
            return false;
        }

        /// <summary>
        /// Processes the output value of a given tool type.
        /// </summary>
        /// <param name="toolType">The tool type to process the output value for.</param>
        /// <param name="rawValue">The raw output value to process.</param>
        /// <param name="isJsonNode">A boolean indicating whether the given tool type is a JSON node.</param>
        /// <returns>The processed output value.</returns>
        private string ProcessOutputValue(string toolType, string? rawValue, bool isJsonNode)
        {
            var raw = rawValue ?? string.Empty;
            string processed;

            if (string.Equals(toolType, HandlersTypes.Ocr, StringComparison.OrdinalIgnoreCase))
            {
                processed = ExtractOcrText(raw);
            }
            else if (string.Equals(toolType, HandlersTypes.Prompt, StringComparison.OrdinalIgnoreCase))
            {
                processed = raw;
            }
            else
            {
                processed = string.IsNullOrWhiteSpace(raw) ? "null" : raw;
            }

            if (!isJsonNode && !processed.StartsWith('"') && !processed.EndsWith('"'))
            {
                processed = JsonSerializer.Serialize(processed, _jsonOptions);
            }

            return processed;
        }

        /// <summary>
        /// Builds the replacement string for the given tool type.
        /// </summary>
        /// <param name="processedValues">The processed output values for the given tool type.</param>
        /// <param name="isJsonNode">A boolean indicating whether the given tool type is a JSON node.</param>
        /// <returns>The replacement string for the given tool type.</returns>
        private string BuildReplacementString(List<string> processedValues, bool isJsonNode)
        {
            if (processedValues.Count == 0)
                return isJsonNode ? "null" : JsonSerializer.Serialize(string.Empty, _jsonOptions);
                
            if (processedValues.Count == 1)
                return processedValues[0];
                
            return "[" + string.Join(", ", processedValues) + "]";
        }

        /// <summary>
        /// Extracts OCR text from a JSON-formatted string containing document embeddings.
        /// </summary>
        /// <remarks>The returned text segments are separated by double newlines. If the input does not
        /// contain valid embeddings or is not properly formatted, the method returns an empty string.</remarks>
        /// <param name="outputValue">A JSON string representing the output with document embeddings. Must contain a 'DocumentEmbeddings' array
        /// with 'Text' properties to extract OCR text.</param>
        /// <returns>A string containing the concatenated OCR text extracted from the 'Text' properties of each embedding.
        /// Returns an empty string if no text is found or if the input is invalid.</returns>
        private static string ExtractOcrText(string outputValue)
        {
            if (string.IsNullOrEmpty(outputValue))
                return string.Empty;

            using var document = JsonDocument.Parse(outputValue);
            var root = document.RootElement;

            if (root.TryGetProperty("DocumentEmbeddings", out var embeddingsArray) &&
                embeddingsArray.ValueKind == JsonValueKind.Array)
            {
                var texts = new List<string>();

                foreach (var embedding in embeddingsArray.EnumerateArray())
                {
                    if (embedding.TryGetProperty("Text", out var embedTextProperty))
                    {
                        var text = embedTextProperty.GetString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            texts.Add(text);
                        }
                    }
                }

                return texts.Count > 0 ? string.Join("\n\n", texts) : string.Empty;
            }

            return string.Empty;
        }
    }
}
