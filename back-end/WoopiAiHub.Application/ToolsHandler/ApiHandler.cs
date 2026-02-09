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
        IEncryptionService encryptationService) : IToolHandler
    {
        public string Type => HandlersTypes.API;
        private readonly MessageQueues _messageQueues = messageQueues.Value;
        private readonly IStepToolRepository _stepToolRepository = stepToolRepository;
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

            var request = BuildApiRequest(stepTool);

            var automationInputDto = new AutomationInputDto
            {
                Url = request.Url,
                RequiredFile = input?.RequiredFile ?? false,
                Tenant = automationServicesDto.Tenant,
                Email = automationServicesDto.Email,
                ResponseQueue = _messageQueues.AutomationQueueResponse,
                Type = ConnectorNames.API,
                Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
                Content = JsonSerializer.Serialize(request),
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
        /// Builds an API request object from the specified step tool parameters.
        /// The method validates that the step tool is configured with the correct tool type and contains the necessary parameters for constructing the API request.
        /// If the validation fails, an <see cref="AppException"/> is thrown with an appropriate error message.
        /// </summary>
        /// <param name="stepTool"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private ApiRequestDto BuildApiRequest(StepToolDto stepTool)
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

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var request = JsonSerializer.Deserialize<ApiRequestDto>(requestStr, options)
                ?? throw new AppException(ErrorCode.InvalidValue, "Invalid API request configuration", null);

            if (!string.IsNullOrEmpty(request.Body))
            {
                // TODO: Preencher com dados do output de passos anteriores, se necessário
            }

            return request;
        }

        private string ConvertOutputsToJson(ICollection<StepToolOutput> outputs, string inputValue)
        {
            
        }
    }
}
