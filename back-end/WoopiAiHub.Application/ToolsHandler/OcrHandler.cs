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
    /// Builds an execution payload for processing OCR tasks based on the provided automation service details.
    /// </summary>
    /// <remarks>This method retrieves tenant-specific OCR model information and constructs a payload for
    /// processing OCR tasks. The payload includes metadata about the automation service, tenant details, and the
    /// appropriate message queues.</remarks>
    /// <param name="automationServicesDto">The automation service details, including tenant, card ID, step tool ID, and other metadata.</param>
    /// <param name="input">The input data required for the payload. This parameter is currently unused but reserved for future use.</param>
    /// <param name="output">The output data required for the payload. This parameter is currently unused but reserved for future use.</param>
    /// <returns>An <see cref="ExecutionMessageDto"/> containing the payload for the OCR processing task, including metadata,
    /// tenant information, and queue details.</returns>
    /// <exception cref="ArgumentException">Thrown if the OCR model for the specified tenant cannot be found.</exception>
    public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                        StepToolParameter? input,
                                                        string output)
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
                ReferenceFile = automationServicesDto.ReferenceFile,
                Model = tenantInfo.OcrModel,
                Email = automationServicesDto.Email,
                ResponseQueue = _messageQueues.OcrQueueAiHubResponse
            }
        };
    }
}