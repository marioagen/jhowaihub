using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services.Automation
{
    public class ExecutionServices(IStepToolExecutionRepository stepToolExecutionRepository,
        ICardRepository cardRepository,
        IStepRepository stepRepository,
        IWorkflowRepository workflowRepository,
        IHubNotifier hubNotifier) : IExecutionServices
    {
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository = stepToolExecutionRepository;
        private readonly ICardRepository _cardRepository = cardRepository;
        private readonly IStepRepository _stepRepository = stepRepository;
        private readonly IWorkflowRepository _workflowRepository = workflowRepository;
        private readonly IHubNotifier _hubNotifier = hubNotifier;

        /// <summary>
        /// Handles the progress update for a step tool execution and notifies the user of the current status.
        /// </summary>
        /// <remarks>This method updates the execution status, calculates the completion percentage of
        /// step tools associated with the relevant card(s), and sends a progress notification to the specified user. If
        /// no step tools are present, the progress is considered complete.</remarks>
        /// <param name="execution">The execution context for the step tool, containing status and identification information required to track
        /// progress.</param>
        /// <param name="email">The email address of the user to notify about the execution progress.</param>
        /// <returns>A task that represents the asynchronous operation of handling execution progress and sending notifications.</returns>
        public async Task HandleExecutionProgress(StepToolExecution execution, string email)
        {
            execution.UpdateStatusExecution(StatusExecution.Ready);
            await _stepToolExecutionRepository.UpdateAsync(execution);

            var card = await _cardRepository.FindById(execution.CardId);
            if (card == null)
                return;

            var relatedCardIds = new List<int> { execution.CardId };

            if (card.DocumentBatchId.HasValue)
            {
                var batchCards = await _cardRepository.FindByDocumentBatchId((int)card.DocumentBatchId);
                relatedCardIds = [.. batchCards.Select(c => c.Id)];
            }

            var step = await _stepRepository.FindByIdWithTools(execution.StepTool!.StepId);
            if (step == null)
                return;

            var totalStepTools = step.StepTools.Count;
            if (totalStepTools == 0)
            {
                await _hubNotifier.CardProgessAsync(email, execution.CardId, 100.0, execution.StepTool.StepId, string.Empty);
                return;
            }

            int completedStepTools = 0;
            string currentToolName = string.Empty;

            foreach (var stepTool in step.StepTools)
            {
                bool allCardsReady = true;
                bool anyCardRunning = false;

                foreach (var cardId in relatedCardIds)
                {
                    var exec = await _stepToolExecutionRepository.FindByStepToolIdAndCardIdAsync(stepTool.Id, cardId);

                    if (exec == null || exec.Status != StatusExecution.Ready)
                    {
                        allCardsReady = false;
                    }

                    if (exec?.Status == StatusExecution.Running)
                    {
                        anyCardRunning = true;
                    }
                }

                if (allCardsReady)
                {
                    completedStepTools++;
                }

                if (anyCardRunning && string.IsNullOrEmpty(currentToolName))
                {
                    var tool = await _workflowRepository.FindToolByStepToolId(stepTool.Id);
                    currentToolName = tool?.Name ?? string.Empty;
                }
            }

            var percent = ((double)completedStepTools / totalStepTools) * 100;

            foreach (var cardId in relatedCardIds)
            {
                await _hubNotifier.CardProgessAsync(email, cardId, percent, execution.StepTool.StepId, currentToolName);
            }
        }
    }
}
