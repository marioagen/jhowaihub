using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.Application.ToolsHandler
{
    public class N8NHandler : IToolHandler
    {
        public string Type => HandlersTypes.N8N;
        private readonly MessageQueues _messageQueues;
        private readonly IToolRepository _toolRepository;

        public N8NHandler(IOptions<MessageQueues> messageQueues,
                          IToolRepository toolRepository)
        {
            _messageQueues = messageQueues.Value;
            _toolRepository = toolRepository;
        }

        /// <summary>
        /// Builds an execution payload for processing N8N tasks with multiple outputs from dependent StepTools.
        /// This overload allows handling outputs from multiple dependencies.
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
            var tool = await _toolRepository.FindModelByStepToolIdAsync(automationServicesDto!.StepToolId)
                ?? throw new AppException(ErrorCode.NotFound, "Tool not found", null);

            string outputsJson = ConvertOutputsToJson(outputs, input!.Value);

            var automationInputDto = new AutomationInputDto
            {
                Url = tool.ConnectorUrl!,
                WebhookId = input!.WebhookId!.Value.ToString(),
                RequiredFile = input.RequiredFile,
                Tenant = automationServicesDto.Tenant,
                Email = automationServicesDto.Email,
                ResponseQueue = _messageQueues.AutomationQueueResponse,
                Type = ConnectorNames.N8N,
                Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
                Content = outputsJson,
                ExecutionId = execution!.Id,
                ReferenceFile = automationServicesDto.ReferenceFile
            };

            return new ExecutionMessageDto
            {
                Queue = _messageQueues.AutomationQueueConsumer,
                Message = automationInputDto
            };
        }

        /// <summary>
        /// converts the outputs collection to a JSON string
        /// </summary>
        /// <param name="outputs"></param>
        /// <returns></returns>
        private static string ConvertOutputsToJson(ICollection<StepToolOutput> outputs, string jsonInput)
        {
            var outputsDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (outputs != null)
            {
                foreach (var o in outputs)
                {
                    var key = o?.StepTool?.Tool?.ToolType?.Name;
                    if (string.IsNullOrWhiteSpace(key))
                        continue;
                    key = key!.ToLowerInvariant();

                    var value = string.Empty;
                    if (key.Equals(HandlersTypes.Ocr.ToLowerInvariant()))
                    {
                        value = ExtractOcrTextFromOutput(o!.Value!);
                    }
                    else
                    {
                        value = o!.Value ?? string.Empty;
                    }                       
                    outputsDict[key] = value;
                }
            }

            if (!string.IsNullOrWhiteSpace(jsonInput))
            {
                try
                {
                    var inputDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonInput);
                    
                    if (inputDict != null)
                    {
                        foreach (var kv in inputDict)
                        {
                            outputsDict[kv.Key.ToLowerInvariant()] = kv.Value?.ToString() ?? string.Empty;
                        }
                    }
                }
                catch (JsonException)
                {
                    outputsDict["input"] = jsonInput;
                }
            }

            var outputsJson = JsonConvert.SerializeObject(outputsDict);

            return outputsJson;
        }

        /// <summary>
        /// Extracts and concatenates OCR text from serialized output
        /// </summary>
        /// <param name="outputJson">Serialized StepToolOutput JSON</param>
        /// <param name="documentId">Document ID for logging</param>
        /// <returns>Concatenated OCR text or empty string if extraction fails</returns>
        private static string ExtractOcrTextFromOutput(string outputJson)
        {
            var embeddingsData = JsonConvert.DeserializeObject<DocumentEmbeddingsDataDto>(outputJson);

            if (embeddingsData?.DocumentEmbeddings == null || !embeddingsData.DocumentEmbeddings.Any())
                return string.Empty;

            return string.Join(Environment.NewLine + Environment.NewLine,
                embeddingsData.DocumentEmbeddings
                    .OrderBy(e => (e.Metadata as dynamic)?.PageNumber ?? 0)
                    .Select(e => e.Text));
        }
    }
}
