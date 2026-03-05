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

        /// <summary>
        /// Updates the status of the specified step tool execution to indicate that it is ready for processing.
        /// </summary>
        /// <remarks>This method sets the execution status to ready and saves the change to the
        /// repository. Ensure that the provided execution instance is valid and not null before calling this
        /// method.</remarks>
        /// <param name="execution">The step tool execution instance whose status will be updated to <see cref="StatusExecution.Ready"/> and
        /// persisted.</param>
        /// <returns>A task that represents the asynchronous operation of updating the execution status.</returns>
        private async Task UpdateExecutionStatus(StepToolExecution execution)
        {
            execution.UpdateStatusExecution(StatusExecution.Ready);
            await _stepToolExecutionRepository.UpdateAsync(execution);
        }

        /// <summary>
        /// Retrieves a list of card IDs that are related to the specified card based on its document batch association.
        /// </summary>
        /// <remarks>If the card's DocumentBatchId is not set, the method returns a list containing only
        /// the ID of the specified card. Otherwise, it retrieves all card IDs associated with the same DocumentBatchId
        /// from the repository.</remarks>
        /// <param name="card">The card for which to find related card IDs. The card must have a valid DocumentBatchId to retrieve related
        /// cards; otherwise, only its own ID will be returned.</param>
        /// <returns>A list of integers representing the IDs of related cards. If the specified card does not have a
        /// DocumentBatchId, the list contains only the ID of the provided card.</returns>
        private async Task<List<int>> GetRelatedCardIds(Card card)
        {
            if (!card.DocumentBatchId.HasValue)
                return [card.Id];

            var batchCards = await _cardRepository.FindByDocumentBatchId(card.DocumentBatchId.Value);
            return [.. batchCards.Select(c => c.Id)];
        }

        /// <summary>
        /// Asynchronously calculates the number of completed step tools and determines the name of the currently
        /// running tool.
        /// </summary>
        /// <remarks>This method evaluates each step tool's status based on the provided related card IDs.
        /// The evaluation is performed asynchronously, and only the first running tool's name is returned if multiple
        /// tools are running.</remarks>
        /// <param name="stepTools">A collection of step tools to evaluate for completion and running status.</param>
        /// <param name="relatedCardIds">A list of related card IDs that influence the evaluation of each step tool's status.</param>
        /// <returns>A tuple containing the count of completed step tools and the name of the currently running tool. The tool
        /// name is an empty string if no tool is currently running.</returns>
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

        /// <summary>
        /// Asynchronously evaluates the status of a step tool and its associated cards, indicating whether all cards
        /// are ready and if any card is currently running.
        /// </summary>
        /// <remarks>This method checks the execution status of each related card to determine the overall
        /// readiness and running state of the step tool. The method performs asynchronous lookups for each card and
        /// aggregates their statuses.</remarks>
        /// <param name="stepToolId">The unique identifier of the step tool whose status is being evaluated.</param>
        /// <param name="relatedCardIds">A list of unique identifiers for the cards related to the specified step tool.</param>
        /// <returns>A tuple containing two boolean values: the first is <see langword="true"/> if all related cards are ready;
        /// the second is <see langword="true"/> if any related card is currently running.</returns>
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

        /// <summary>
        /// Asynchronously retrieves the name of the tool associated with the specified step tool identifier.
        /// </summary>
        /// <remarks>This method performs an asynchronous lookup in the workflow repository to find the
        /// tool by its step tool ID. If the tool is not found, an empty string is returned.</remarks>
        /// <param name="stepToolId">The unique identifier of the step tool whose name is to be retrieved. Must be a valid step tool ID.</param>
        /// <returns>A string containing the name of the tool associated with the specified step tool ID, or an empty string if
        /// no tool is found.</returns>
        private async Task<string> GetToolName(int stepToolId)
        {
            var tool = await _workflowRepository.FindToolByStepToolId(stepToolId);
            return tool?.Name ?? string.Empty;
        }

        /// <summary>
        /// Calculates the progress percentage based on the number of completed items relative to the total number of
        /// items.
        /// </summary>
        /// <remarks>If <paramref name="totalCount"/> is zero, a division by zero exception will occur.
        /// Ensure that <paramref name="totalCount"/> is greater than zero before calling this method.</remarks>
        /// <param name="completedCount">The number of items that have been completed. Must be greater than or equal to zero and less than or equal
        /// to <paramref name="totalCount"/>.</param>
        /// <param name="totalCount">The total number of items to be completed. Must be greater than zero.</param>
        /// <returns>A double representing the percentage of completed items, calculated as (<paramref name="completedCount"/>
        /// divided by <paramref name="totalCount"/>) multiplied by 100.</returns>
        private static double CalculateProgressPercentage(int completedCount, int totalCount)
        {
            return ((double)completedCount / totalCount) * 100;
        }

        /// <summary>
        /// Notifies the user by email when a progress step for the specified card has been completed.
        /// </summary>
        /// <remarks>This method sends a notification indicating that the progress for the specified card
        /// has reached 100%.</remarks>
        /// <param name="email">The email address of the user to notify about the completion of the progress step.</param>
        /// <param name="cardId">The unique identifier of the card for which the progress completion is being reported.</param>
        /// <param name="stepId">The unique identifier of the step that has reached completion.</param>
        /// <returns>A task that represents the asynchronous operation of sending the notification.</returns>
        private async Task NotifyCompleteProgress(string email, int cardId, int stepId)
        {
            await _hubNotifier.CardProgessAsync(email, cardId, 100.0, stepId, string.Empty);
        }

        /// <summary>
        /// Sends progress notifications for all specified cards to the given user via email.
        /// </summary>
        /// <remarks>Notifications are sent individually for each card in the provided list, updating them
        /// with the specified progress information.</remarks>
        /// <param name="email">The email address of the user who will receive the progress notifications.</param>
        /// <param name="relatedCardIds">A list of card identifiers for which progress notifications will be sent.</param>
        /// <param name="progressPercent">The percentage of progress completed for the current operation. Must be between 0 and 100.</param>
        /// <param name="stepId">The identifier of the current step in the process being reported.</param>
        /// <param name="currentToolName">The name of the tool currently used to perform the operation.</param>
        /// <returns>A task that represents the asynchronous operation of sending notifications for each related card.</returns>
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
