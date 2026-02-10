using System.Text.Encodings.Web;
using System.Text.Json;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services.Automation
{
    public class ApiOutputServices(IStepToolOutputRepository stepToolOutputRepository,
        IStepToolExecutionRepository stepToolExecutionRepository,
        IDocumentHistoryRepository documentHistoryRepository,
        IWorkflowRepository workflowRepository,
        IHubNotifier hubNotifier) : IApiOutputServices
    {
        private readonly IStepToolOutputRepository _stepToolOutputRepository = stepToolOutputRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository = stepToolExecutionRepository;
        private readonly IDocumentHistoryRepository _documentHistoryRepository = documentHistoryRepository;
        private readonly IWorkflowRepository _workflowRepository = workflowRepository;
        private readonly IHubNotifier _hubNotifier = hubNotifier;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        /// <summary>
        /// Processes the specified API output message, updates related execution and document history records, and
        /// returns an automation services data transfer object.
        /// </summary>
        /// <param name="outputDto">The API output data to process. Must contain a valid execution identifier and associated output information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see
        /// cref="AutomationServicesDto"/> with details about the processed automation service.</returns>
        /// <exception cref="AppException">Thrown if the execution specified by <paramref name="outputDto"/> does not exist.</exception>
        public async Task<AutomationServicesDto> ProcessMessage(ApiOutputDto outputDto)
        {
            var execution = await _stepToolExecutionRepository.FindByIdAsync(outputDto.ExecutionId) ?? throw new AppException(ErrorCode.NotFound, "StepToolExecution not found", null);

            var content = JsonSerializer.Serialize(new
            {
                outputDto.StatusCode,
                outputDto.Content
            }, _jsonOptions);

            var stepToolOutput = new StepToolOutput(
                0,
                DateTime.Now,
                execution.StepToolId,
                execution.CardId,
                content
            );

            await _stepToolOutputRepository.CreateAsync(stepToolOutput);

            var documentHistory = new DocumentHistory(execution.Card!.DocumentId, "API", content, 0, DateTime.Now);
            _documentHistoryRepository.Create(documentHistory);

            await UpdateExecutionAsync(execution, outputDto.Email);

            return new AutomationServicesDto
            (
                execution.StepToolId,
                execution.CardId,
                outputDto.Tenant,
                outputDto.Email,
                execution.Card!.Document!.ReferenceFile,
                0
            );
        }

        /// <summary>
        /// Updates StepToolExecution status and send notification 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task UpdateExecutionAsync(StepToolExecution execution, string email)
        {
            var count = await _stepToolExecutionRepository.ExecutionsByStepIdCountAsync(execution.StepTool!.StepId, execution.CardId);
            var percent = (count / execution.StepTool.Order) * 100;

            execution.UpdateStatusExecution(StatusExecution.Ready);
            await _stepToolExecutionRepository.UpdateAsync(execution);

            var tool = await _workflowRepository.FindToolByStepToolId(execution.StepToolId);
            await _hubNotifier.CardProgessAsync(email, execution.CardId, percent, execution.StepTool.StepId, tool != null ? tool.Name : string.Empty);
        }
    }
}
