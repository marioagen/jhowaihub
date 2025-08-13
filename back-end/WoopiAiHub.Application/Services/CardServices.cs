using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class CardServices : ICardServices
    {
        private readonly ICardRepository _cardRepository;
        private readonly IStepRepository _stepRepository;
        private readonly IStatusRepository _statusRepository;

        public CardServices(ICardRepository cardRepository,
                            IStepRepository stepRepository,
                            IStatusRepository statusRepository)
        {
            _cardRepository = cardRepository;
            _stepRepository = stepRepository;
            _statusRepository = statusRepository;
        }

        /// <summary>
        /// Updates the step and status of a card.
        /// </summary>
        /// <param name="updateCardStepStatusDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> UpdateStepAndStatus(UpdateCardStepStatusDto updateCardStepStatusDto)
        {
            var card = await _cardRepository.FindById(updateCardStepStatusDto.CardId);
            if (card == null)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, "Card not found", CardLabel.NotFound);
            }
            var step = await _stepRepository.FindById(updateCardStepStatusDto.StepId);
            if (step == null)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, "Step not found", StepLabel.NotFound);
            }
            var status = await _statusRepository.FindById(updateCardStepStatusDto.StatusId);
            if (status == null)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, "Status not found", StatusLabel.NotFound);
            }

            card.UpdateStepAndSatus(step.Id, status.Id);

            return _cardRepository.Update(card);
        }
    }
}
