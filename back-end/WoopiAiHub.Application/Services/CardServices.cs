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

        public CardServices(ICardRepository cardRepository,
                            IStepRepository stepRepository)
        {
            _cardRepository = cardRepository;
            _stepRepository = stepRepository;
        }

        /// <summary>
        /// Updates assigned user
        /// </summary>
        /// <param name="updateAssingnedUserDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> UpdateAssignedUser(UpdateAssignedUserDto updateAssingnedUserDto)
        {
            var card = await _cardRepository.FindById(updateAssingnedUserDto.CardId);
            if (card == null)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, "Card not found", CardLabel.NotFound);
            }

            if (updateAssingnedUserDto.UserId == null || updateAssingnedUserDto.UserId == Guid.Empty)
            {
                card.UpdateAssignedUser(null);
            }
            else
            {
                var isValidTeamUser = card.Step?.Workflow?.Team?.Users.Any(a => a.Id.Equals(updateAssingnedUserDto.UserId));
                if (!isValidTeamUser.HasValue || !isValidTeamUser.Value)
                {
                    throw new AppException(Domain.Enum.ErrorCode.NotFound, "User not found", CardLabel.UserCannotBeAssigned);
                }

                card.UpdateAssignedUser(updateAssingnedUserDto.UserId);
            }

            return _cardRepository.Update(card);
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
            var step = await _stepRepository.FindByOrderAndWorkflowId(updateCardStepStatusDto.NextStepOrder,
                                                                      updateCardStepStatusDto.WorkflowId);
            if (step == null)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, "Step not found", StepLabel.NotFound);
            }

            card.UpdateStepAndSatus(step.Id, step.StatusId);

            return _cardRepository.Update(card);
        }
    }
}
