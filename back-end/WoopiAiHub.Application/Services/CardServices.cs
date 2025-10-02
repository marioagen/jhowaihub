using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class CardServices : ICardServices
    {
        private readonly ICardRepository _cardRepository;
        private readonly IStepRepository _stepRepository;
        private readonly IAutomationServices _automationServices;

        public CardServices(ICardRepository cardRepository,
                            IStepRepository stepRepository,
                            IAutomationServices automationServices)
        {
            _cardRepository = cardRepository;
            _stepRepository = stepRepository;
            _automationServices = automationServices;
        }

        /// <summary>
        /// Updates assigned user
        /// </summary>
        /// <param name="updateAssingnedUserDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> AssignUser(UpdateAssignedUserDto updateAssingnedUserDto)
        {
            var card = await _cardRepository.FindById(updateAssingnedUserDto.CardId);
            if (card == null)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, "Card not found", CardLabel.NotFound);
            }

            if (updateAssingnedUserDto.UserId == Guid.Empty)
            {
                throw new ArgumentNullException(updateAssingnedUserDto.UserId.ToString(), "Invalid UserId");
            }

            var isValidTeamUser = card.Step?.Workflow?.Team?.Users.Any(a => a.Id.Equals(updateAssingnedUserDto.UserId));
            if (!isValidTeamUser.HasValue || !isValidTeamUser.Value)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, "User not found", CardLabel.UserCannotBeAssigned);
            }

            card.UpdateAssignedUser(updateAssingnedUserDto.UserId);

            return _cardRepository.Update(card);
        }

        /// <summary>
        /// Updates assigned user to null
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> UnassignUser(int cardId)
        {
            var card = await _cardRepository.FindById(cardId);
            if (card == null)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, "Card not found", CardLabel.NotFound);
            }

            card.UpdateAssignedUser(null);

            return _cardRepository.Update(card);
        }

        /// <summary>
        /// Updates the step and status of a card.
        /// </summary>
        /// <param name="updateCardStepStatusDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> UpdateStepAndStatus(UpdateCardStepStatusDto updateCardStepStatusDto,
                                                    string tenant,
                                                    string email)
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
            var result = _cardRepository.Update(card);

            var automationServicesDto = new AutomationServicesDto
            (
                0,
                card.Id,
                tenant,
                email,
                card.Document.ReferenceFile,
                step.Id
            );
            if (result)
                await _automationServices.StartExecutionByCardAsync(automationServicesDto);

            return true;
        }
    }
}
