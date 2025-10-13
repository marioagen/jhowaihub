using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services.Automation
{
    public class N8NServices : IN8NServices
    {
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IHubNotifier _hubNotifier;

        public N8NServices(IStepToolOutputRepository stepToolOutputRepository,
                           IStepToolExecutionRepository stepToolExecutionRepository,
                           IHubNotifier hubNotifier)
        {
            _stepToolOutputRepository = stepToolOutputRepository;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _hubNotifier = hubNotifier;
        }

        /// <summary>
        /// Process message from N8N workflow 
        /// </summary>
        /// <param name="automationInputDto"></param>
        /// <returns></returns>
        public async Task ProcessMessage(AutomationOutputDto automationOutputDto)
        {
            var content = automationOutputDto.Content?.ToString() ?? "";
            var stepToolOutput = new StepToolOutput(
                0, 
                DateTime.Now,
                automationOutputDto.Data.StepToolId,
                automationOutputDto.Data.CardId,
                content);

            await _stepToolOutputRepository.CreateAsync(stepToolOutput);

            var execution = await _stepToolExecutionRepository
                .FindByStepToolIdAndCardIdAsync(automationOutputDto.Data.StepToolId, automationOutputDto.Data.CardId);

            await UpdateExecutionAsync(execution!, automationOutputDto.Email!);
        }

        /// <summary>
        /// Updates StepToolExecution status and send notification 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task UpdateExecutionAsync(StepToolExecution execution, string email)
        {
            var count = await _stepToolExecutionRepository.ExecutionsByStepIdCountAsync(execution.StepTool!.StepId,
                                                                                        execution.CardId);
            var percent = (count / execution.StepTool.Order) * 100;

            execution.UpdateStatusExecution(StatusExecution.Ready);
            await _stepToolExecutionRepository.UpdateAsync(execution);

            await _hubNotifier.CardProgessAsync(email, execution.CardId, percent, execution.StepTool.StepId);
        }
    }
}
