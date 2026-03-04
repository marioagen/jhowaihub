using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(CardCollection))]
    public class DocumentAnalysisRejectionServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IDocumentAnalysisRejectionRepository> _rejectionRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IStepRepository> _stepRepositoryMock;
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<IStatusRepository> _statusRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPermissionServices> _permissionServicesMock;
        private readonly DocumentAnalysisRejectionServices _rejectionServices;

        public DocumentAnalysisRejectionServicesTests()
        {
            _mocker = new AutoMocker();
            _rejectionRepositoryMock = _mocker.GetMock<IDocumentAnalysisRejectionRepository>();
            _userRepositoryMock = _mocker.GetMock<IUserRepository>();
            _stepRepositoryMock = _mocker.GetMock<IStepRepository>();
            _cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            _statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            _unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();
            _permissionServicesMock = _mocker.GetMock<IPermissionServices>();

            _rejectionServices = _mocker.CreateInstance<DocumentAnalysisRejectionServices>();
        }

        [Fact(DisplayName = "CreateRejectionAsync should throw exception when user does not have permission")]
        [Trait("CreateRejectionAsync", "Fail")]
        public async Task CreateRejectionAsync_UserWithoutPermission_ThrowsAppException()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _rejectionServices.CreateRejectionAsync(dto, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("User does not have permission to reject documents", exception.Message);
        }

        [Fact(DisplayName = "CreateRejectionAsync should throw exception when card is not found")]
        [Trait("CreateRejectionAsync", "Fail")]
        public async Task CreateRejectionAsync_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindById(dto.CardId))
                .ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _rejectionServices.CreateRejectionAsync(dto, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(CardLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "CreateRejectionAsync should throw exception when step is not found")]
        [Trait("CreateRejectionAsync", "Fail")]
        public async Task CreateRejectionAsync_StepNotFound_ThrowsAppException()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindById(dto.CardId))
                .ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindById(dto.StepId))
                .ReturnsAsync((Step?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _rejectionServices.CreateRejectionAsync(dto, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(StepLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "CreateRejectionAsync should throw exception when status is not found")]
        [Trait("CreateRejectionAsync", "Fail")]
        public async Task CreateRejectionAsync_StatusNotFound_ThrowsAppException()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindById(dto.CardId))
                .ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindById(dto.StepId))
                .ReturnsAsync(step);
            _statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync((Status?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _rejectionServices.CreateRejectionAsync(dto, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(StatusLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "CreateRejectionAsync should throw exception when user is not found")]
        [Trait("CreateRejectionAsync", "Fail")]
        public async Task CreateRejectionAsync_UserNotFound_ThrowsAppException()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";
            var profiles = new List<string>();

            _userRepositoryMock.Setup(repo => repo.FindUserProfilesByEmailAsync(email))
                .ReturnsAsync(profiles);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _rejectionServices.CreateRejectionAsync(dto, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(UserLabel.UnauthorizedOperation, exception.LabelError);
        }

        [Fact(DisplayName = "CreateRejectionAsync should successfully create rejection")]
        [Trait("CreateRejectionAsync", "Success")]
        public async Task CreateRejectionAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();
            var status = CardFixture.FindValidStatus();
            var user = new User(Guid.NewGuid(), "Test User", email, true, DateTime.Now);

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindById(dto.CardId))
                .ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindById(dto.StepId))
                .ReturnsAsync(step);
            _statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync(status);
            _userRepositoryMock.Setup(repo => repo.FindIdByEmail(email))
                .Returns(user.Id);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>()))
                .Returns(true);
            _rejectionRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<DocumentAnalysisRejection>()))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.BeginTransaction()).Verifiable();
            _unitOfWorkMock.Setup(u => u.Commit()).Verifiable();

            // Act
            var result = await _rejectionServices.CreateRejectionAsync(dto, email);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.IsAny<List<Card>>()), Times.Once);
            _rejectionRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<DocumentAnalysisRejection>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "CreateRejectionAsync should rollback transaction on exception")]
        [Trait("CreateRejectionAsync", "Fail")]
        public async Task CreateRejectionAsync_TransactionFails_RollsBack()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();
            var status = CardFixture.FindValidStatus();
            var user = new User(Guid.NewGuid(), "Test User", email, true, DateTime.Now);
            var permissions = new Dictionary<string, List<string>>
            {
                { "Actions", new List<string> { "DocumentRejection" } }
            };

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindById(dto.CardId))
                .ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindById(dto.StepId))
                .ReturnsAsync(step);
            _statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync(status);
            _userRepositoryMock.Setup(repo => repo.FindIdByEmail(email))
                .Returns(user.Id);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>()))
                .Returns(true);
            _rejectionRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<DocumentAnalysisRejection>()))
                .ThrowsAsync(new Exception("Database error"));
            _unitOfWorkMock.Setup(u => u.BeginTransaction()).Verifiable();
            _unitOfWorkMock.Setup(u => u.Rollback()).Verifiable();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _rejectionServices.CreateRejectionAsync(dto, email));
            Assert.Equal(ErrorCode.DefaultError, exception.ErrorCode);
            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
        }

        [Fact(DisplayName = "FindRejectionsByCardIdAsync should return rejections for valid card ID")]
        [Trait("FindRejectionsByCardIdAsync", "Success")]
        public async Task FindRejectionsByCardIdAsync_ValidCardId_ReturnsRejections()
        {
            // Arrange
            var cardId = 1;
            var rejections = CardFixture.FindValidDocumentAnalysisRejectionDtoList();

            _rejectionRepositoryMock.Setup(repo => repo.FindByCardIdAsync(cardId))
                .ReturnsAsync(rejections);

            // Act
            var result = await _rejectionServices.FindRejectionsByCardIdAsync(cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(rejections.Count, result.Count);
            Assert.Equal(rejections, result);
            _rejectionRepositoryMock.Verify(repo => repo.FindByCardIdAsync(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindRejectionsByCardIdAsync should return empty list when no rejections found")]
        [Trait("FindRejectionsByCardIdAsync", "Success")]
        public async Task FindRejectionsByCardIdAsync_NoRejections_ReturnsEmptyList()
        {
            // Arrange
            var cardId = 1;
            var emptyList = new List<DocumentAnalysisRejectionDto>();

            _rejectionRepositoryMock.Setup(repo => repo.FindByCardIdAsync(cardId))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _rejectionServices.FindRejectionsByCardIdAsync(cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _rejectionRepositoryMock.Verify(repo => repo.FindByCardIdAsync(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindWorkflowPreviousStepsAsync should throw exception when step not found")]
        [Trait("FindWorkflowPreviousStepsAsync", "Fail")]
        public async Task FindWorkflowPreviousStepsAsync_StepNotFound_ThrowsAppException()
        {
            // Arrange
            var workflowId = 1;
            var cardId = 1;

            _stepRepositoryMock.Setup(repo => repo.FindStepByCardId(cardId))
                .ReturnsAsync((Step?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _rejectionServices.FindWorkflowPreviousStepsAsync(workflowId, cardId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Step Card not found", exception.Message);
        }

        [Fact(DisplayName = "FindWorkflowPreviousStepsAsync should return previous steps ordered by step order")]
        [Trait("FindWorkflowPreviousStepsAsync", "Success")]
        public async Task FindWorkflowPreviousStepsAsync_ValidData_ReturnsPreviousStepsOrdered()
        {
            // Arrange
            var workflowId = 1;
            var cardId = 1;
            var currentStep = new Step(2, DateTime.Now, 2, "Step 2", 2, workflowId, 1);
            var previousSteps = new List<StepDto>
            {
                new StepDto { Id = 1, Name = "Step 1", Order = 1, WorkflowId = workflowId },
            };

            _stepRepositoryMock.Setup(repo => repo.FindStepByCardId(cardId))
                .ReturnsAsync(currentStep);
            _stepRepositoryMock.Setup(repo => repo.FindPreviousStepsByWorkflowIdAndOrder(workflowId, currentStep.Order))
                .ReturnsAsync(previousSteps);

            // Act
            var result = await _rejectionServices.FindWorkflowPreviousStepsAsync(workflowId, cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(previousSteps.Count, result.Count);
            Assert.All(result, step => Assert.Equal(workflowId, step.WorkflowId));
            _stepRepositoryMock.Verify(repo => repo.FindStepByCardId(cardId), Times.Once);
            _stepRepositoryMock.Verify(repo => repo.FindPreviousStepsByWorkflowIdAndOrder(workflowId, currentStep.Order), Times.Once);
        }

        [Fact(DisplayName = "FindWorkflowPreviousStepsAsync should return empty list when no previous steps")]
        [Trait("FindWorkflowPreviousStepsAsync", "Success")]
        public async Task FindWorkflowPreviousStepsAsync_NoPreviousSteps_ReturnsEmptyList()
        {
            // Arrange
            var workflowId = 1;
            var cardId = 1;
            var currentStep = new Step(1, DateTime.Now, 1, "Step 1", 1, workflowId, 1);
            var emptyList = new List<StepDto>();

            _stepRepositoryMock.Setup(repo => repo.FindStepByCardId(cardId))
                .ReturnsAsync(currentStep);
            _stepRepositoryMock.Setup(repo => repo.FindPreviousStepsByWorkflowIdAndOrder(workflowId, currentStep.Order))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _rejectionServices.FindWorkflowPreviousStepsAsync(workflowId, cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _stepRepositoryMock.Verify(repo => repo.FindStepByCardId(cardId), Times.Once);
            _stepRepositoryMock.Verify(repo => repo.FindPreviousStepsByWorkflowIdAndOrder(workflowId, currentStep.Order), Times.Once);
        }

        [Fact(DisplayName = "FindWorkflowPreviousStepsAsync should return steps ordered by step order")]
        [Trait("FindWorkflowPreviousStepsAsync", "Success")]
        public async Task FindWorkflowPreviousStepsAsync_MultipleSteps_ReturnsOrderedByOrder()
        {
            // Arrange
            var workflowId = 1;
            var cardId = 1;
            var currentStep = new Step(3, DateTime.Now, 3, "Step 3", 3, workflowId, 1);
            var previousSteps = new List<StepDto>
            {
                new StepDto { Id = 2, Name = "Step 2", Order = 2, WorkflowId = workflowId },
                new StepDto { Id = 1, Name = "Step 1", Order = 1, WorkflowId = workflowId },
            };

            _stepRepositoryMock.Setup(repo => repo.FindStepByCardId(cardId))
                .ReturnsAsync(currentStep);
            _stepRepositoryMock.Setup(repo => repo.FindPreviousStepsByWorkflowIdAndOrder(workflowId, currentStep.Order))
                .ReturnsAsync(previousSteps);

            // Act
            var result = await _rejectionServices.FindWorkflowPreviousStepsAsync(workflowId, cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(previousSteps.Count, result.Count);
            Assert.Equal(1, result[0].Order);
            Assert.Equal(2, result[1].Order);
            _stepRepositoryMock.Verify(repo => repo.FindStepByCardId(cardId), Times.Once);
            _stepRepositoryMock.Verify(repo => repo.FindPreviousStepsByWorkflowIdAndOrder(workflowId, currentStep.Order), Times.Once);
        }

        [Fact(DisplayName = "CreateRejectionAsync should reject all cards in batch when DocumentBatchId is present")]
        [Trait("CreateRejectionAsync", "DocumentBatch")]
        public async Task CreateRejectionAsync_WithDocumentBatch_RejectsAllBatchCards()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";
            var documentBatchId = 100;

            var card1 = new Card(1, DateTime.UtcNow, 1, 1, "Card 1", 1, null, documentBatchId);
            var card2 = new Card(2, DateTime.UtcNow, 1, 2, "Card 2", 1, null, documentBatchId);
            var card3 = new Card(3, DateTime.UtcNow, 1, 3, "Card 3", 1, null, documentBatchId);

            var batchCards = new List<Card> { card1, card2, card3 };
            var step = CardFixture.FindValidStep();
            var status = CardFixture.FindValidStatus();
            var userId = Guid.NewGuid();

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindById(dto.CardId))
                .ReturnsAsync(card1);
            _cardRepositoryMock.Setup(repo => repo.FindByDocumentBatchId(documentBatchId))
                .ReturnsAsync(batchCards);
            _stepRepositoryMock.Setup(repo => repo.FindById(dto.StepId))
                .ReturnsAsync(step);
            _statusRepositoryMock.Setup(repo => repo.FindByName(StatusNames.Rejected))
                .ReturnsAsync(status);
            _userRepositoryMock.Setup(repo => repo.FindIdByEmail(email))
                .Returns(userId);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>()))
                .Returns(true);
            _rejectionRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<DocumentAnalysisRejection>()))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.BeginTransaction()).Verifiable();
            _unitOfWorkMock.Setup(u => u.Commit()).Verifiable();

            // Act
            var result = await _rejectionServices.CreateRejectionAsync(dto, email);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.FindByDocumentBatchId(documentBatchId), Times.Once);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.Is<List<Card>>(cards => cards.Count == 3)), Times.Once);
            _rejectionRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<DocumentAnalysisRejection>()), Times.Exactly(3));
            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "CreateRejectionAsync should reject single card when no DocumentBatchId")]
        [Trait("CreateRejectionAsync", "SingleCard")]
        public async Task CreateRejectionAsync_WithoutDocumentBatch_RejectsSingleCard()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();
            var status = CardFixture.FindValidStatus();
            var userId = Guid.NewGuid();

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindById(dto.CardId))
                .ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindById(dto.StepId))
                .ReturnsAsync(step);
            _statusRepositoryMock.Setup(repo => repo.FindByName(StatusNames.Rejected))
                .ReturnsAsync(status);
            _userRepositoryMock.Setup(repo => repo.FindIdByEmail(email))
                .Returns(userId);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>()))
                .Returns(true);
            _rejectionRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<DocumentAnalysisRejection>()))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.BeginTransaction()).Verifiable();
            _unitOfWorkMock.Setup(u => u.Commit()).Verifiable();

            // Act
            var result = await _rejectionServices.CreateRejectionAsync(dto, email);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.FindByDocumentBatchId(It.IsAny<int>()), Times.Never);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.Is<List<Card>>(cards => cards.Count == 1)), Times.Once);
            _rejectionRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<DocumentAnalysisRejection>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "CreateRejectionAsync should update all batch cards with correct step and status")]
        [Trait("CreateRejectionAsync", "DocumentBatch")]
        public async Task CreateRejectionAsync_WithDocumentBatch_UpdatesAllCardsWithCorrectStepAndStatus()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";
            var documentBatchId = 100;

            var card1 = new Card(1, DateTime.UtcNow, 1, 1, "Card 1", 1, null, documentBatchId);
            var card2 = new Card(2, DateTime.UtcNow, 1, 2, "Card 2", 1, null, documentBatchId);

            var batchCards = new List<Card> { card1, card2 };
            var step = new Step(dto.StepId, DateTime.Now, dto.StepId, "Rejection Step", 1, 1, 1);
            var status = new Status("Rejected", "Rejected status", 99, DateTime.Now);
            var userId = Guid.NewGuid();

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindById(dto.CardId))
                .ReturnsAsync(card1);
            _cardRepositoryMock.Setup(repo => repo.FindByDocumentBatchId(documentBatchId))
                .ReturnsAsync(batchCards);
            _stepRepositoryMock.Setup(repo => repo.FindById(dto.StepId))
                .ReturnsAsync(step);
            _statusRepositoryMock.Setup(repo => repo.FindByName(StatusNames.Rejected))
                .ReturnsAsync(status);
            _userRepositoryMock.Setup(repo => repo.FindIdByEmail(email))
                .Returns(userId);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>()))
                .Returns(true);
            _rejectionRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<DocumentAnalysisRejection>()))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.BeginTransaction()).Verifiable();
            _unitOfWorkMock.Setup(u => u.Commit()).Verifiable();

            // Act
            var result = await _rejectionServices.CreateRejectionAsync(dto, email);

            // Assert
            Assert.True(result);
            Assert.All(batchCards, card => 
            {
                Assert.Equal(dto.StepId, card.StepId);
                Assert.Equal(status.Id, card.StatusId);
            });
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.Is<List<Card>>(cards => 
                cards.All(c => c.StepId == dto.StepId && c.StatusId == status.Id)
            )), Times.Once);
        }

        [Fact(DisplayName = "CreateRejectionAsync should rollback all changes when exception occurs with batch")]
        [Trait("CreateRejectionAsync", "DocumentBatch")]
        public async Task CreateRejectionAsync_WithDocumentBatch_RollsBackOnException()
        {
            // Arrange
            var dto = CardFixture.FindValidCreateDocumentAnalysisRejectionDto();
            var email = "test@example.com";
            var documentBatchId = 100;

            var card1 = new Card(1, DateTime.UtcNow, 1, 1, "Card 1", 1, null, documentBatchId);
            var card2 = new Card(2, DateTime.UtcNow, 1, 2, "Card 2", 1, null, documentBatchId);

            var batchCards = new List<Card> { card1, card2 };
            var step = CardFixture.FindValidStep();
            var status = CardFixture.FindValidStatus();
            var userId = Guid.NewGuid();

            _permissionServicesMock.Setup(repo => repo.UserHasPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindById(dto.CardId))
                .ReturnsAsync(card1);
            _cardRepositoryMock.Setup(repo => repo.FindByDocumentBatchId(documentBatchId))
                .ReturnsAsync(batchCards);
            _stepRepositoryMock.Setup(repo => repo.FindById(dto.StepId))
                .ReturnsAsync(step);
            _statusRepositoryMock.Setup(repo => repo.FindByName(StatusNames.Rejected))
                .ReturnsAsync(status);
            _userRepositoryMock.Setup(repo => repo.FindIdByEmail(email))
                .Returns(userId);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>()))
                .Throws(new Exception("Database error during batch update"));
            _unitOfWorkMock.Setup(u => u.BeginTransaction()).Verifiable();
            _unitOfWorkMock.Setup(u => u.Rollback()).Verifiable();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _rejectionServices.CreateRejectionAsync(dto, email));

            Assert.Equal(ErrorCode.DefaultError, exception.ErrorCode);
            Assert.Contains("Database error during batch update", exception.Message);
            _cardRepositoryMock.Verify(repo => repo.FindByDocumentBatchId(documentBatchId), Times.Once);
            _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }
}
