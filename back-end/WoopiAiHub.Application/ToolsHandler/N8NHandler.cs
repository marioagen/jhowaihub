using Microsoft.Extensions.Options;
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
        /// Builds an execution payload for processing OCR tasks based on the provided automation service details.
        /// </summary>
        /// <param name="automationServicesDto"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                            StepToolParameter? input,
                                                            string output,
                                                            StepToolExecution? execution = null)
        {        
            var tool = await _toolRepository.FindModelByStepToolIdAsync(automationServicesDto.StepToolId)
                ?? throw new AppException(ErrorCode.NotFound, "Tool not found", null);

            return new ExecutionMessageDto
            {
                Queue = _messageQueues.AutomationQueueConsumer,
                Message = new AutomationInputDto
                {
                    Url = tool.ConnectorUrl!,
                    WebhookId = input!.WebhookId!.Value.ToString(),
                    RequiredFile = input.RequiredFile,
                    Tenant = automationServicesDto.Tenant,
                    Email = automationServicesDto.Email,
                    ResponseQueue = _messageQueues.AutomationQueueResponse,
                    Type = ConnectorNames.N8N,
                    Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
                    Content = input.Value.ToString(),
                    ExecutionId = execution!.Id
                }
            };
        }
    }
}
