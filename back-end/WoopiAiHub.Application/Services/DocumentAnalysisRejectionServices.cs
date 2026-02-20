using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class DocumentAnalysisRejectionServices : IDocumentAnalysisRejectionServices
    {
        private readonly IDocumentAnalysisRejectionRepository _repository;
        private readonly IStepRepository _stepRepository;
        private readonly IPermissionServices _permissionServices;
        private readonly ICardRepository _cardRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DocumentAnalysisRejectionServices(
            IDocumentAnalysisRejectionRepository repository,
            IStepRepository stepRepository,
            IPermissionServices permissionServices,
            ICardRepository cardRepository,
            IStatusRepository statusRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _stepRepository = stepRepository;
            _cardRepository = cardRepository;
            _statusRepository = statusRepository;
            _permissionServices = permissionServices;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Creates a new document analysis rejection record for the specified user.
        /// </summary>
        /// <remarks>The user must have the "DocumentRejection" permission in the "Actions" group to
        /// create a rejection.</remarks>
        /// <param name="dto">The data transfer object containing the justification, card ID, and step ID for the rejection.</param>
        /// <param name="emailCreator">The email of the user performing the rejection.</param>
        /// <returns><see langword="true"/> if the rejection was created successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="AppException">Thrown if the user is not found or does not have permission to reject documents.</exception>
        public async Task<bool> CreateRejectionAsync(CreateDocumentAnalysisRejectionDto dto, string emailCreator)
        {
            (Card card, Status status) = await Validate(dto, emailCreator);
            var userId = _userRepository.FindIdByEmail(emailCreator);
            var rejection = new DocumentAnalysisRejection(
                0,
                DateTime.Now,
                dto.Justification,
                dto.CardId,
                dto.StepId,
                userId
            );
            _unitOfWork.BeginTransaction();
            try
            {
                card.UpdateStepAndStatus(dto.StepId, status.Id);
                _cardRepository.Update(card);
                await _repository.CreateAsync(rejection);
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Validate and return card, status and user
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task<(Card card, Status status)> Validate(CreateDocumentAnalysisRejectionDto dto, string emailCreator)
        {
            var hasPermission = await _permissionServices.UserHasPermissionAsync(emailCreator, "Actions", "DocumentRejection");
            if (!hasPermission)
            {
                throw new AppException(ErrorCode.NotFound, "User does not have permission to reject documents", UserLabel.UnauthorizedOperation);
            }
            var card = await _cardRepository.FindById(dto.CardId);
            if (card == null)
            {
                throw new AppException(ErrorCode.NotFound, "Card not found", CardLabel.NotFound);
            }
            var step = await _stepRepository.FindById(dto.StepId);
            if (step == null)
            {
                throw new AppException(ErrorCode.NotFound, "Step not found", StepLabel.NotFound);
            }
            var status = await _statusRepository.FindById((int)CardStatus.Rejected);
            if (status == null)
            {
                throw new AppException(ErrorCode.NotFound, "Status not found", StatusLabel.NotFound);
            }

            return (card, status);
        }

        /// <summary>
        /// Asynchronously retrieves all document analysis rejections associated with the specified card identifier.
        /// </summary>
        /// <param name="cardId">The unique identifier of the card for which to retrieve rejection records. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="DocumentAnalysisRejectionDto"/> objects associated with the specified card. The list is empty if no
        /// rejections are found.</returns>
        public async Task<List<DocumentAnalysisRejectionDto>> FindRejectionsByCardIdAsync(int cardId)
        {
            return await _repository.FindByCardIdAsync(cardId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public async Task<List<StepDto>> FindWorkflowPreviousStepsAsync(int workflowId, int cardId)
        {
            var step = await _stepRepository.FindStepByCardId(cardId);
            if (step == null)
            {
                throw new AppException(ErrorCode.NotFound, "Step Card not found", null);
            }
            var steps = await _stepRepository.FindPreviousStepsByWorkflowIdAndOrder(workflowId, step.Order);
            return steps.OrderBy(s => s.Order).ToList();
        }
    }
}
