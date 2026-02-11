using System.Text.Encodings.Web;
using System.Text.Json;
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

            if (!string.IsNullOrEmpty(request.Body))
            {
                request.Body = ConvertOutputsToJson(outputs, request.Body);
            }

            request.Email = automation.Email;
            request.Tenant = automation.Tenant;
            request.Data = new MetaDataAutomationDto(automation.CardId, automation.StepToolId);
            request.ResponseQueue = _messageQueues.ApiRequestQueueResponse;
            request.ExecutionId = executionId ?? throw new AppException(ErrorCode.InvalidValue, "ExecutionId not defined", null);
            request.TemplateName = template.Name;

            return request;
        }

        /// <summary>
        /// Replaces specific placeholders in the input string with corresponding values from the provided tool outputs
        /// and returns the resulting JSON string.
        /// </summary>
        /// <remarks>Placeholders are replaced in a case-insensitive manner. Only outputs with tool types
        /// of OCR, Embeddings, or Prompt are processed; other tool types are ignored.</remarks>
        /// <param name="outputs">A collection of tool output objects whose values are used to replace placeholders in the input string. Only
        /// outputs with recognized tool types are considered.</param>
        /// <param name="inputValue">The input string containing placeholders to be replaced with output values. Placeholders must match the
        /// expected format (e.g., "{{ocr}}", "{{embeddings}}", or "{{prompt}}").</param>
        /// <returns>A string representing the input value with recognized placeholders replaced by the corresponding output
        /// values. If no placeholders are matched, the original input string is returned.</returns>
        private string ConvertOutputsToJson(ICollection<StepToolOutput> outputs, string inputValue)
        {
            var result = inputValue;

            foreach (var output in outputs)
            {
                if (output.StepTool?.Tool?.ToolType == null)
                {
                    continue;
                }

                var toolType = output.StepTool.Tool.ToolType.Name;
                var placeholder = string.Empty;
                var replaceValue = string.Empty;

                switch (toolType)
                {
                    case HandlersTypes.Ocr:
                        placeholder = "{{ocr}}";
                        replaceValue = ExtractOcrText(output.Value);
                        break;
                    case HandlersTypes.Embeddings:
                        placeholder = "{{embeddings}}";
                        replaceValue = ExtractEmbeddingsText(output.Value);
                        break;
                    case HandlersTypes.Prompt:
                        placeholder = "{{prompt}}";
                        replaceValue = output.Value;
                        break;
                    default:
                        continue;
                }

                if (result.Contains(placeholder, StringComparison.OrdinalIgnoreCase))
                {
                    if (!replaceValue.StartsWith("\"") && !replaceValue.EndsWith("\""))
                    {
                        replaceValue = JsonSerializer.Serialize(replaceValue, _jsonOptions);
                    }

                    result = result.Replace(placeholder, replaceValue, StringComparison.OrdinalIgnoreCase);
                }
            }

            return result;
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
            try
            {
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
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts and concatenates the text content from the "DocumentEmbeddings" array in a JSON-formatted string.
        /// </summary>
        /// <param name="outputValue">A JSON-formatted string containing a "DocumentEmbeddings" array, where each element may include a "Text"
        /// property.</param>
        /// <returns>A single string containing the concatenated text values from all "Text" properties in the
        /// "DocumentEmbeddings" array, separated by double newlines. Returns an empty string if no such text values are
        /// found or if the input is not in the expected format.</returns>
        private static string ExtractEmbeddingsText(string outputValue)
        {
            try
            {
                using var document = JsonDocument.Parse(outputValue);
                var root = document.RootElement;
                
                if (root.TryGetProperty("DocumentEmbeddings", out var embeddingsArray) && 
                    embeddingsArray.ValueKind == JsonValueKind.Array)
                {
                    var texts = new List<string>();
                    
                    foreach (var embedding in embeddingsArray.EnumerateArray())
                    {
                        if (embedding.TryGetProperty("Text", out var textProperty))
                        {
                            var text = textProperty.GetString();
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
            catch
            {
                return string.Empty;
            }
        }
    }
}
