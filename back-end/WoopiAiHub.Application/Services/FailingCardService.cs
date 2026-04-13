using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class FailingCardService : IFailingCardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubNotifier _hubNotifier;

        private const string CardNotFoundMessage = "Card not found";

        public FailingCardService(ICardRepository cardRepository,
                                  IStatusRepository statusRepository,
                                  IStepToolExecutionRepository stepToolExecutionRepository,
                                  IUnitOfWork unitOfWork,
                                  IHubNotifier hubNotifier)
        {
            _cardRepository = cardRepository;
            _statusRepository = statusRepository;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _unitOfWork = unitOfWork;
            _hubNotifier = hubNotifier;
        }

        /// <summary>
        /// Sets the specified card to a failing status and updates its execution state if applicable.
        /// </summary>
        /// <remarks>If the card already has a failing status, the method returns immediately without further processing.
        /// If the card has an execution in a running state, its execution status is set to pending. The operation is 
        /// performed within a transaction and will be rolled back if an error occurs.</remarks>
        /// <param name="cardId">The unique identifier of the card to update. Must correspond to an existing card.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="AppException">Thrown if the card with the specified identifier is not found, if the fail status is not found, or if an
        /// error occurs during the update process.</exception>
        public async Task SetFailingCard(int cardId, string? email)
        {
            var card = await _cardRepository.FindByIdWithExecutions(cardId) ?? throw new AppException(ErrorCode.NotFound, CardNotFoundMessage, CardLabel.NotFound);
            var failStatus = await _statusRepository.FindByName(StatusNames.Fail) ?? throw new AppException(ErrorCode.NotFound, $"Status '{StatusNames.Fail}' not found", null);

            if (card.StatusId.Equals(failStatus.Id))
                return;

            _unitOfWork.BeginTransaction();
            try
            {
                card.UpdateStatus(failStatus.Id);
                _cardRepository.Update(card);

                var execution = card.Executions.FirstOrDefault(w => w.Status == StatusExecution.Running);
                if (execution is not null)
                {
                    execution.UpdateStatusExecution(StatusExecution.Pending);
                    await _stepToolExecutionRepository.UpdateAsync(execution);
                }

                if (!string.IsNullOrEmpty(email))
                {
                    await _hubNotifier.CardProgessAsync(email, card.Id, 0.0, card.StepId, string.Empty, true);
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }
    }
}
