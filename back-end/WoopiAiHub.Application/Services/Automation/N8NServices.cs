using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services.Automation
{
    public class N8NServices : IN8NServices
    {
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IDocumentHistoryRepository _documentHistoryRepository;
        private readonly IExecutionServices _executionServices;

        public N8NServices(IStepToolOutputRepository stepToolOutputRepository,
                           IStepToolExecutionRepository stepToolExecutionRepository,
                           IDocumentHistoryRepository documentHistoryRepository,
                           IExecutionServices executionServices)
        {
            _stepToolOutputRepository = stepToolOutputRepository;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _documentHistoryRepository = documentHistoryRepository;
            _executionServices = executionServices;
        }

        /// <summary>
        /// Process message from N8N workflow 
        /// </summary>
        /// <param name="automationInputDto"></param>
        /// <returns></returns>
        public async Task<AutomationServicesDto> ProcessMessage(AutomationOutputDto automationOutputDto)
        {
            var execution = await _stepToolExecutionRepository.FindByIdAsync(automationOutputDto.ExecutionId);

            var automationServicesDto = new AutomationServicesDto(
                execution!.StepToolId,
                execution.CardId,
                automationOutputDto.Tenant!,
                automationOutputDto.Email!,
                execution.Card!.Document!.ReferenceFile,
                execution.StepTool?.StepId,
                execution.StepTool?.Step?.WorkflowId
            );

            var content = automationOutputDto.Content?.ToString() ?? "";
            var stepToolOutput = new StepToolOutput(
                0, 
                DateTime.Now,
                execution.StepToolId,
                execution.CardId,
                content);

            await _stepToolOutputRepository.CreateAsync(stepToolOutput);

            var documentHistory = new DocumentHistory(execution.Card!.DocumentId, "N8N", content.JsonToHumanReadable(), 0, DateTime.Now);
            _documentHistoryRepository.Create(documentHistory);

            await _executionServices.HandleExecutionProgress(execution, automationOutputDto.Email!);

            return automationServicesDto;
        }
    }
}
