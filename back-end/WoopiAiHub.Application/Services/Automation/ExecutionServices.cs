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
            await UpdateExecutionStatus(execution);

            var card = await _cardRepository.FindById(execution.CardId);
            if (card == null)
                return;

            var relatedCardIds = await GetRelatedCardIds(card);
            var step = await _stepRepository.FindByIdWithTools(execution.StepTool!.StepId);

            if (step == null)
                return;

            if (step.StepTools.Count == 0)
            {
                await NotifyCompleteProgress(email, execution.CardId, execution.StepTool.StepId);
                return;
            }

            var (completedCount, currentToolName) = await CalculateProgress(step.StepTools, relatedCardIds);
            var progressPercent = CalculateProgressPercentage(completedCount, step.StepTools.Count);

            await NotifyAllCards(email, relatedCardIds, progressPercent, execution.StepTool.StepId, currentToolName);
        }

        private async Task UpdateExecutionStatus(StepToolExecution execution)
        {
            execution.UpdateStatusExecution(StatusExecution.Ready);
            await _stepToolExecutionRepository.UpdateAsync(execution);
        }

        private async Task<List<int>> GetRelatedCardIds(Card card)
        {
            if (!card.DocumentBatchId.HasValue)
                return [card.Id];

            var batchCards = await _cardRepository.FindByDocumentBatchId(card.DocumentBatchId.Value);
            return [.. batchCards.Select(c => c.Id)];
        }

        private async Task<(int completedCount, string currentToolName)> CalculateProgress(
            ICollection<StepTool> stepTools, 
            List<int> relatedCardIds)
        {
            int completedStepTools = 0;
            string currentToolName = string.Empty;

            foreach (var stepTool in stepTools)
            {
                var (isCompleted, isRunning) = await EvaluateStepToolStatus(stepTool.Id, relatedCardIds);

                if (isCompleted)
                    completedStepTools++;

                if (isRunning && string.IsNullOrEmpty(currentToolName))
                    currentToolName = await GetToolName(stepTool.Id);
            }

            return (completedStepTools, currentToolName);
        }

        private async Task<(bool isCompleted, bool isRunning)> EvaluateStepToolStatus(
            int stepToolId, 
            List<int> relatedCardIds)
        {
            bool allCardsReady = true;
            bool anyCardRunning = false;

            foreach (var cardId in relatedCardIds)
            {
                var execution = await _stepToolExecutionRepository.FindByStepToolIdAndCardIdAsync(stepToolId, cardId);

                if (execution == null || execution.Status != StatusExecution.Ready)
                    allCardsReady = false;

                if (execution?.Status == StatusExecution.Running)
                    anyCardRunning = true;
            }

            return (allCardsReady, anyCardRunning);
        }

        private async Task<string> GetToolName(int stepToolId)
        {
            var tool = await _workflowRepository.FindToolByStepToolId(stepToolId);
            return tool?.Name ?? string.Empty;
        }

        private static double CalculateProgressPercentage(int completedCount, int totalCount)
        {
            return ((double)completedCount / totalCount) * 100;
        }

        private async Task NotifyCompleteProgress(string email, int cardId, int stepId)
        {
            await _hubNotifier.CardProgessAsync(email, cardId, 100.0, stepId, string.Empty);
        }

        private async Task NotifyAllCards(
            string email, 
            List<int> relatedCardIds, 
            double progressPercent, 
            int stepId, 
            string currentToolName)
        {
            foreach (var cardId in relatedCardIds)
            {
                await _hubNotifier.CardProgessAsync(email, cardId, progressPercent, stepId, currentToolName);
            }
        }
    }
}
