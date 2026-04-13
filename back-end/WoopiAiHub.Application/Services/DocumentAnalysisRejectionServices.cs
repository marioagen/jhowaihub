using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class DocumentAnalysisRejectionServices : IDocumentAnalysisRejectionServices
    {
        private readonly IDocumentAnalysisRejectionRepository _repository;
        private readonly IStepRepository _stepRepository;
        private readonly IPermissionServices _permissionServices;
        private readonly ICardRepository _cardRepository;
        private readonly IAuditCardService _auditCardService;
        private readonly IStatusRepository _statusRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICardServices _cardServices;

        public DocumentAnalysisRejectionServices(
            IDocumentAnalysisRejectionRepository repository,
            IStepRepository stepRepository,
            IPermissionServices permissionServices,
            ICardRepository cardRepository,
            IAuditCardService auditCardService,
            IStatusRepository statusRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICardServices cardServices)
        {
            _repository = repository;
            _stepRepository = stepRepository;
            _cardRepository = cardRepository;
            _auditCardService = auditCardService;
            _statusRepository = statusRepository;
            _permissionServices = permissionServices;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _cardServices = cardServices;
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
            (List<Card> cards, Status status) = await Validate(dto, emailCreator);
            var userId = _userRepository.FindIdByEmail(emailCreator);
            return await CommitRejectionsAsync(cards, dto.StepId, dto.Justification, userId, status);
        }

        /// <summary>
        /// Creates document analysis rejection records for multiple cards in one operation.
        /// </summary>
        /// <remarks>When a user id is supplied for assignment, assigns that user to the cards first (persisted), then reloads
        /// cards as tracked entities so the rejection update does not conflict with Entity Framework tracking from assign.</remarks>
        /// <param name="dto">Justification, step id, card ids, and optional user to assign before rejecting.</param>
        /// <param name="emailCreator">Email of the user performing the operation; used for permission checks and for the rejection user id when <paramref name="dto"/> has no user id.</param>
        /// <returns><see langword="true"/> if rejections were committed successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="AppException">Thrown when validation fails (permission, missing card, step, or status).</exception>
        public async Task<bool> CreateRejectionRangeAsync(CreateDocumentAnalysisRejectionRangeDto dto, string emailCreator)
        {
            (List<Card> cards, Status status) = await ValidateRangeAsync(dto, emailCreator);

            if (dto.UserId.HasValue)
            {
                await AssignRangeAsync(dto.UserId, cards);
                var cardIds = cards.Select(c => c.Id).Distinct().ToList();
                cards = await _cardRepository.FindRangeByIdsWithStepWorkflowTracked(cardIds);
            }

            var userId = dto.UserId ?? _userRepository.FindIdByEmail(emailCreator);
            return await CommitRejectionsAsync(cards, dto.StepId, dto.Justification, userId, status);
        }

        private async Task AssignRangeAsync(Guid? userIdToAssign, List<Card> cards)
        {
            if (!userIdToAssign.HasValue)
            {
                return;
            }

            var cardIds = cards.Select(c => c.Id).Distinct().ToList();
            if (cardIds.Count == 0)
            {
                return;
            }

            await _cardServices.AssignRangeAsync(new AssignRangeDto(userIdToAssign.Value, cardIds));
        }

        private async Task<bool> CommitRejectionsAsync(
            List<Card> cards,
            int stepId,
            string justification,
            Guid userId,
            Status status)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                List<DocumentAnalysisRejection> rejections = [];
                foreach (var card in cards)
                {
                    var rejection = new DocumentAnalysisRejection(
                        0,
                        DateTime.Now,
                        justification,
                        card.Id,
                        stepId,
                        userId
                    );

                    rejections.Add(rejection);
                }

                Card.UpdateStepAndStatus(cards, stepId, status.Id);

                var cardWorkflows = cards.Where(c => c.Step != null).Select(c => (c.Id, c.Step!.WorkflowId, c.DocumentId)).ToList();
                foreach (var card in cards)
                {
                    card.Step = null;
                    card.Status = null;
                }

                if (cardWorkflows.Count > 0)
                {
                    await _auditCardService.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Rejection);
                }

                await _repository.CreateRangeAsync(rejections);
                _cardRepository.UpdateList(cards);
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        private async Task ValidateRejectionPermissionsAsync(string emailCreator)
        {
            var hasPermission = await _permissionServices.UserHasPermissionAsync(
                emailCreator,
                PermissionGroups.Documents,
                PermissionNames.Rejection);
            if (!hasPermission)
            {
                throw new AppException(ErrorCode.NotFound, "User does not have permission to reject documents", UserLabel.UnauthorizedOperation);
            }
        }

        /// <summary>
        /// Validate and return a list of cards, status and user
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task<(List<Card> card, Status status)> Validate(CreateDocumentAnalysisRejectionDto dto, string emailCreator)
        {
            await ValidateRejectionPermissionsAsync(emailCreator);

            var card = await _cardRepository.FindByIdWithStepWorkflow(dto.CardId) ?? throw new AppException(ErrorCode.NotFound, "Card not found", CardLabel.NotFound);

            List<Card> cards = [card];
            if (card.DocumentBatchId.HasValue)
            {
                cards = await _cardRepository.FindByDocumentBatchId(card.DocumentBatchId.Value);
            }

            _ = await _stepRepository.FindById(dto.StepId) ?? throw new AppException(ErrorCode.NotFound, "Step not found", StepLabel.NotFound);
            var status = await _statusRepository.FindByName(StatusNames.Rejected) ?? throw new AppException(ErrorCode.NotFound, "Status not found", StatusLabel.NotFound);

            return (cards, status);
        }

        private async Task<(List<Card> card, Status status)> ValidateRangeAsync(CreateDocumentAnalysisRejectionRangeDto dto, string emailCreator)
        {
            await ValidateRejectionPermissionsAsync(emailCreator);

            if (dto.CardIds == null || dto.CardIds.Count == 0)
            {
                throw new AppException(ErrorCode.NotFound, "CardIds cannot be empty", CardLabel.NotFound);
            }

            var seen = new HashSet<int>();
            var cards = new List<Card>();
            foreach (var id in dto.CardIds)
            {
                if (!seen.Add(id))
                {
                    continue;
                }

                var card = await _cardRepository.FindByIdWithStepWorkflow(id)
                    ?? throw new AppException(ErrorCode.NotFound, $"Card not found: {id}", CardLabel.NotFound);
                cards.Add(card);
            }

            _ = await _stepRepository.FindById(dto.StepId) ?? throw new AppException(ErrorCode.NotFound, "Step not found", StepLabel.NotFound);
            var status = await _statusRepository.FindByName(StatusNames.Rejected) ?? throw new AppException(ErrorCode.NotFound, "Status not found", StatusLabel.NotFound);

            return (cards, status);
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
