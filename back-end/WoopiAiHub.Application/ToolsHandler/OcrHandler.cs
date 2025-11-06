using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
namespace WoopiAiHub.Application.ToolsHandler;

public class OcrHandler : IToolHandler
{
    public string Type => HandlersTypes.Ocr;
    private readonly MessageQueues _messageQueues;
    private readonly ITenantCacheServices _tenantCacheServices;

    public OcrHandler(ITenantCacheServices tenantCacheServices, IOptions<MessageQueues> messageQueues)
    {
        _tenantCacheServices = tenantCacheServices;
        _messageQueues = messageQueues.Value;
    }

    /// <summary>
    /// Builds an execution payload for processing OCR tasks with multiple outputs from dependent StepTools.
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
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(automationServicesDto.Tenant, ColTypeModule.WoopiAiHub);
        if (string.IsNullOrEmpty(tenantInfo!.OcrModel))
        {
            throw new ArgumentException("Ocr not found");
        }

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.OcrQueue,
            Message = new ProcessOcrDto
            {
                Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
                Tenant = automationServicesDto.Tenant,
                ReferenceFile = automationServicesDto.ReferenceFile!,
                Model = tenantInfo.OcrModel,
                Email = automationServicesDto.Email,
                ResponseQueue = _messageQueues.OcrQueueAiHubResponse
            }
        };
    }
}