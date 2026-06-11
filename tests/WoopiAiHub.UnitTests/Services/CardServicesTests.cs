using System.Linq;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(CardCollection))]
    public class CardServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<IStepRepository> _stepRepositoryMock;
        private readonly Mock<IStepToolRepository> _stepToolRepositoryMock;
        private readonly Mock<IAutomationServices> _automationServices;
        private readonly CardServices _cardServices;

        public CardServicesTests()
        {
            _mocker = new AutoMocker();
            _cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            _stepRepositoryMock = _mocker.GetMock<IStepRepository>();
            _stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            _automationServices = _mocker.GetMock<IAutomationServices>();

            _mocker.GetMock<IAuditCardService>()
                .Setup(s => s.CreateBatchAndSaveAsync(
                    It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                    It.IsAny<AuditCardActionType>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _cardServices = _mocker.CreateInstance<CardServices>();
        }

        [Fact(DisplayName = "Tests update Step and Status and throws an AppException when Card not found")]
        [Trait("UpdateStepAndStatus", "Fail")]
        public async Task UpdateStepAndStatus_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithDocumentAsync(updateDto.CardId))
                .ReturnsAsync((List<Card>?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _cardServices.UpdateStepAndStatus(updateDto, It.IsAny<string>(), It.IsAny<string>()));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(CardLabel.NotFound, exception.LabelError);
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithDocumentAsync(updateDto.CardId), Times.Once);
        }

        [Fact(DisplayName = "Tests update Step and Status and throws an AppException when step not found")]
        [Trait("UpdateStepAndStatus", "Fail")]
        public async Task UpdateStepAndStatus_StepNotFound_ThrowsAppException()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            var card = CardFixture.FindValidCard();
            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithDocumentAsync(It.IsAny<int>())).ReturnsAsync(new List<Card> { card });
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                updateDto.WorkflowId)).ReturnsAsync((Step?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _cardServices.UpdateStepAndStatus(updateDto, It.IsAny<string>(), It.IsAny<string>()));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(StepLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Tests update Step and Status and returns true")]
        [Trait("UpdateStepAndStatus", "Success")]
        public async Task UpdateStepAndStatus_ValidInputs_UpdatesCardAndReturnsTrue()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();
            var status = CardFixture.FindValidStatus();
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithDocumentAsync(It.IsAny<int>())).ReturnsAsync(new List<Card> { card });
            _automationServices.Setup(s => s.StartExecutionByCardAsync(It.IsAny<AutomationServicesDto>())).Returns(Task.CompletedTask);
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                updateDto.WorkflowId)).ReturnsAsync(step);

            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);


            _stepToolRepositoryMock.Setup(repo => repo.FindByStepIdAndOrderAsync(1, 1))
                .ReturnsAsync(It.IsAny<StepTool>());

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.UpdateStepAndStatus(updateDto, "tenant", "email");

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.IsAny<List<Card>>()), Times.Once);
        }

        [Fact(DisplayName = "Tests update UnassignUser when card not found and throws AppException")]
        [Trait("UnassignUser", "Fail")]
        public async Task UnassignUser_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var cardId = 1;
            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync((List<Card>?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.UnassignUser(cardId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Card not found", exception.Message);
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId), Times.Once);
        }

        [Fact(DisplayName = "Tests update UnassignUser to null successfully")]
        [Trait("UnassignUser", "Success")]
        public async Task UnassignUser_Success()
        {
            // Arrange
            var cardId = 1;
            var card = CardFixture.FindValidCard();
            card.Step = CardFixture.FindValidStepWithWorkflow();

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.UnassignUser(cardId);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.IsAny<List<Card>>()), Times.Once);
            Assert.Null(card.AssignedUserId);
        }

        [Fact(DisplayName = "Tests update AssignedUser when card not found and throws AppException")]
        [Trait("AssignUser", "Fail")]
        public async Task AssignUser_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(updateAssignedUserDto.CardId))
                .ReturnsAsync((List<Card>?)null);

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignUser(updateAssignedUserDto));
            Assert.Equal(Domain.Enum.ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Card not found", exception.Message);
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithStepWorkflowAsync(updateAssignedUserDto.CardId),
                Times.Once);
        }

        [Fact(DisplayName = "Tests update AssignedUser throws ArgumentNullException when userId is empty")]
        [Trait("AssignUser", "Fail")]
        public async Task UpdateAssignedUser_UserIdIsEmpty_ThrowsArgumentNullException()
        {
            // Arrange
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            updateAssignedUserDto.UserId = Guid.Empty;

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentNullException>(() => _cardServices.AssignUser(updateAssignedUserDto));

            // Assert
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithStepWorkflowAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact(DisplayName = "Tests update AssignedUser when User not in Team and throws AppException")]
        [Trait("AssignUser", "Fail")]
        public async Task AssignUser_UserNotInTeam_ThrowsAppException()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            card.Step = CardFixture.FindValidStepWithWorkflow();
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(updateAssignedUserDto.CardId))
                .ReturnsAsync(new List<Card> { card });

            var workflowRepositoryMock = _mocker.GetMock<IWorkflowRepository>();
            workflowRepositoryMock
                .Setup(r => r.IsValidTeamUser(updateAssignedUserDto.CardId, updateAssignedUserDto.UserId))
                .ReturnsAsync(false);

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignUser(updateAssignedUserDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("User not found", exception.Message);
        }

        [Fact(DisplayName = "Tests update AssignedUser when UserId is valid")]
        [Trait("AssignUser", "Sucess")]
        public async Task AssignUser_ValidUser_UpdatesAssignedUser()
        {
            // Arrange
            var userId = Guid.Parse("20c41dd6-1518-468b-8b0c-b5d8c0d31dec");
            var card = CardFixture.FindValidCard();
            card.UpdateAssignedUser(userId);
            card.Step = CardFixture.FindValidStepWithWorkflow();
            card.Step.Workflow.Teams = [DocumentFixture.FindValidTeam()];
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            updateAssignedUserDto.UserId = userId;

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(updateAssignedUserDto.CardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var workflowRepositoryMock = _mocker.GetMock<IWorkflowRepository>();
            workflowRepositoryMock.Setup(r => r.IsValidTeamUser(updateAssignedUserDto.CardId, userId)).ReturnsAsync(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.AssignUser(updateAssignedUserDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.IsAny<List<Card>>()), Times.Once);
        }

        [Fact(DisplayName = "Tests update UnassignUser with DocumentBatch updates all batch cards")]
        [Trait("UnassignUser", "DocumentBatch")]
        public async Task UnassignUser_WithDocumentBatch_UpdatesAllBatchCards()
        {
            // Arrange
            var cardId = 1;
            var documentBatchId = 100;
            var batchCards = CardFixture.FindDocumentBatchCardsWithAssignedUsers(documentBatchId);

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(batchCards);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.UnassignUser(cardId);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId), Times.Once);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.Is<List<Card>>(cards => cards.Count == 3)), Times.Once);
            Assert.All(batchCards, c => Assert.Null(c.AssignedUserId));
        }

        [Fact(DisplayName = "Tests update AssignedUser with DocumentBatch updates all batch cards")]
        [Trait("AssignUser", "DocumentBatch")]
        public async Task AssignUser_WithDocumentBatch_UpdatesAllBatchCards()
        {
            // Arrange
            var userId = Guid.Parse("20c41dd6-1518-468b-8b0c-b5d8c0d31dec");
            var documentBatchId = 100;
            var batchCards = CardFixture.FindDocumentBatchCardsWithoutAssignedUser(documentBatchId);
            batchCards[0].Step!.Workflow!.Teams = [DocumentFixture.FindValidTeam()];

            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            updateAssignedUserDto.UserId = userId;

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(updateAssignedUserDto.CardId))
                .ReturnsAsync(batchCards);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var workflowRepositoryMock = _mocker.GetMock<IWorkflowRepository>();
            workflowRepositoryMock.Setup(r => r.IsValidTeamUser(updateAssignedUserDto.CardId, userId)).ReturnsAsync(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.AssignUser(updateAssignedUserDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithStepWorkflowAsync(updateAssignedUserDto.CardId), Times.Once);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.Is<List<Card>>(cards => cards.Count == 3)), Times.Once);
            Assert.All(batchCards, c => Assert.Equal(userId, c.AssignedUserId));
        }

        [Fact(DisplayName = "UpdateStatus with DocumentBatch should update all batch cards")]
        [Trait("UpdateStatus", "DocumentBatch")]
        public async Task UpdateStatus_WithDocumentBatch_UpdatesAllBatchCards()
        {
            // Arrange
            var documentBatchId = 100;
            var updateCardStatusDto = CardFixture.FindValidCardStatusDto();
            var cardId = updateCardStatusDto.CardId;
            var batchCards = CardFixture.FindDocumentBatchCardsWithoutAssignedUser(documentBatchId);

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId)).ReturnsAsync(batchCards);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.UpdateStatus(updateCardStatusDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId), Times.Once);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.Is<List<Card>>(cards => cards.Count == 3)), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsSuccess")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_Success()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var workflow = WorkflowFixture.FindWorkflowForAnalyzeWithPromptTool();

            var cardAnalysisDto = CardFixture.FindCardAnalysisDtoWithOutput();

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"doc-{cardAnalysisDto.DocumentId}", result.DocumentId);
            Assert.Equal(cardAnalysisDto.Document?.Name, result.Name);
            Assert.Equal(cardAnalysisDto.Document?.Description, result.Description);
            Assert.Equal(cardAnalysisDto.Document?.ReferenceFile, result.ReferenceFile);
            Assert.NotEmpty(result.Steps);
            _cardRepositoryMock.Verify(a => a.FindByIdWithDocumentAndWorkflow(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFail")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_Fail()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync((CardAnalysisDto)null!);

            // Act / Assert
            await Assert.ThrowsAsync<AppException>(() => _cardServices.FindByIdAnalyzeWithSteps(cardId, headers));
            _cardRepositoryMock.Verify(a => a.FindByIdWithDocumentAndWorkflow(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFailsWhenDocumentNotFound")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_FailsWhenDocumentNotFound()
        {
            // Arrange: card found but with no Document (relationship not loaded / null)
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            
            var cardAnalysisDto = CardFixture.FindValidCardAnalysisDto(cardId);
            cardAnalysisDto.Document = null;

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act / Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _cardServices.FindByIdAnalyzeWithSteps(cardId, headers));
            Assert.Equal("Document not found for the card", ex.Message);
            _cardRepositoryMock.Verify(a => a.FindByIdWithDocumentAndWorkflow(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFailsWhenWorkflowNotFound")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_FailsWhenWorkflowNotFound()
        {
            // Arrange: card has Document but no Step/Workflow
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            
            var cardAnalysisDto = CardFixture.FindValidCardAnalysisDto(cardId);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Actually call to workflowRepository will happen and return null for non-configured calls
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync((Workflow?)null);

            // Act / Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _cardServices.FindByIdAnalyzeWithSteps(cardId, headers));
            Assert.Contains("Workflow not found", ex.Message);
            _cardRepositoryMock.Verify(a => a.FindByIdWithDocumentAndWorkflow(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFiltersOCROutputs")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_FiltersOCROutputs()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var workflow = WorkflowFixture.FindWorkflowForAnalyzeWithOcrTool();

            var cardAnalysisDto = CardFixture.FindCardAnalysisDtoWithOCROutput(cardId);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Empty(result.Steps[0].Outputs);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFiltersEmbeddingsOutputs")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_FiltersEmbeddingsOutputs()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var workflow = WorkflowFixture.FindWorkflowForAnalyzeWithEmbeddingsTool();

            var cardAnalysisDto = CardFixture.FindCardAnalysisDtoWithEmbeddingsOutput(cardId);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Empty(result.Steps[0].Outputs);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsParsesJsonOutput")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_ParsesJsonOutput()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var workflow = WorkflowFixture.FindWorkflowForAnalyzeWithPromptTool();

            var cardAnalysisDto = CardFixture.FindCardAnalysisDtoWithJsonOutput(cardId);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Equal(2, result.Steps[0].Outputs.Count);
            Assert.Contains(result.Steps[0].Outputs, o => o.Label == "Nome" && o.Value == "João Silva");
            Assert.Contains(result.Steps[0].Outputs, o => o.Label == "Email" && o.Value == "joao@example.com");
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsHandlesPlainTextOutput")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_HandlesPlainTextOutput()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var workflow = WorkflowFixture.FindWorkflowForAnalyzeWithPromptTool();

            var cardAnalysisDto = CardFixture.FindCardAnalysisDtoWithPlainTextOutput(cardId);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Single(result.Steps[0].Outputs);
            Assert.Equal("Test Tool", result.Steps[0].Outputs[0].Label);
            Assert.Equal("This is a plain text response without JSON structure", result.Steps[0].Outputs[0].Value);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsHandlesMultipleSteps")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_HandlesMultipleSteps()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var workflow = WorkflowFixture.FindWorkflowForAnalyzeWithTwoPromptSteps();

            var cardAnalysisDto = CardFixture.FindCardAnalysisDtoWithMultipleOutputs(cardId);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Steps.Count);
            Assert.NotEmpty(result.Steps[0].Outputs);
            Assert.NotEmpty(result.Steps[1].Outputs);
            Assert.Equal("2", result.LastProcessedStepId);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsReturnsEmptyOutputsWhenNoOutputs")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_ReturnsEmptyOutputsWhenNoOutputs()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var workflow = WorkflowFixture.FindWorkflowForAnalyzeWithPromptTool();

            var cardAnalysisDto = CardFixture.FindValidCardAnalysisDto(cardId);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Empty(result.Steps[0].Outputs);
            Assert.Equal("1", result.LastProcessedStepId);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsHandlesInvalidJsonGracefully")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_HandlesInvalidJsonGracefully()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var workflow = WorkflowFixture.FindWorkflowForAnalyzeWithPromptTool();

            var cardAnalysisDto = CardFixture.FindCardAnalysisDtoWithInvalidJsonOutput(cardId);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardAnalysisDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Single(result.Steps[0].Outputs);
            Assert.Equal("Test Tool", result.Steps[0].Outputs[0].Label);
            Assert.Equal("{\"field\": \"value\", invalid json", result.Steps[0].Outputs[0].Value);
        }

        [Fact(DisplayName = "FindCardHeaderInfoAsync_Success")]
        [Trait("FindCardHeaderInfoAsync", "Success")]
        public async Task FindCardHeaderInfoAsync_Success()
        {
            // Arrange
            var cardId = 1;
            var expectedDto = CardFixture.FindValidCardHeaderDto();

            _cardRepositoryMock.Setup(repo => repo.FindHeaderInfoAsync(cardId))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _cardServices.FindHeaderInfoAsync(cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.CardName, result.CardName);
            Assert.Equal(expectedDto.WorkflowName, result.WorkflowName);
            Assert.Equal(expectedDto.WorkflowId, result.WorkflowId);
            _cardRepositoryMock.Verify(repo => repo.FindHeaderInfoAsync(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindCardHeaderInfoAsync returns DocumentBatchId when card belongs to a batch")]
        [Trait("FindCardHeaderInfoAsync", "Success")]
        public async Task FindCardHeaderInfoAsync_CardInBatch_ReturnsDocumentBatchId()
        {
            // Arrange
            var cardId = 1;
            const int documentBatchId = 50;
            var expectedDto = CardFixture.FindValidCardHeaderDtoWithBatchId(documentBatchId);

            _cardRepositoryMock.Setup(repo => repo.FindHeaderInfoAsync(cardId))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _cardServices.FindHeaderInfoAsync(cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(documentBatchId, result.DocumentBatchId);
            _cardRepositoryMock.Verify(repo => repo.FindHeaderInfoAsync(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindCardHeaderInfoAsync returns null DocumentBatchId when card is not in a batch")]
        [Trait("FindCardHeaderInfoAsync", "Success")]
        public async Task FindCardHeaderInfoAsync_CardNotInBatch_ReturnsNullDocumentBatchId()
        {
            // Arrange
            var cardId = 1;
            var expectedDto = CardFixture.FindValidCardHeaderDto();

            _cardRepositoryMock.Setup(repo => repo.FindHeaderInfoAsync(cardId))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _cardServices.FindHeaderInfoAsync(cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.DocumentBatchId);
            _cardRepositoryMock.Verify(repo => repo.FindHeaderInfoAsync(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindCardHeaderInfoAsync_CardNotFound_ThrowsAppException")]
        [Trait("FindCardHeaderInfoAsync", "Fail")]
        public async Task FindCardHeaderInfoAsync_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var cardId = 1;
            _cardRepositoryMock.Setup(repo => repo.FindHeaderInfoAsync(cardId))
                .ReturnsAsync((CardHeaderDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.FindHeaderInfoAsync(cardId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(CardLabel.NotFound, exception.LabelError);
            _cardRepositoryMock.Verify(repo => repo.FindHeaderInfoAsync(cardId), Times.Once);
        }

        [Fact(DisplayName = "UpdateStepAndStatus should rollback card changes when automation fails")]
        [Trait("UpdateStepAndStatus", "Fail")]
        public async Task UpdateStepAndStatus_AutomationFails_RollsBackCardChanges()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();

            var previousStepId = card.StepId;
            var previousStatusId = card.StatusId;

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithDocumentAsync(updateDto.CardId)).ReturnsAsync(new List<Card> { card });
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                updateDto.WorkflowId)).ReturnsAsync(step);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.Update(It.IsAny<Card>())).Returns(true);

            _automationServices.Setup(s => s.StartExecutionByCardAsync(It.IsAny<AutomationServicesDto>()))
                .ThrowsAsync(new Exception("Automation service failed"));

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _cardServices.UpdateStepAndStatus(updateDto, "tenant", "email"));

            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.IsAny<List<Card>>()), Times.Once);
            _cardRepositoryMock.Verify(repo => repo.Update(It.IsAny<Card>()), Times.Once);
            Assert.Equal(previousStepId, card.StepId);
            Assert.Equal(previousStatusId, card.StatusId);
        }

        [Fact(DisplayName = "UpdateStepAndStatus should not call automation when card update fails")]
        [Trait("UpdateStepAndStatus", "Fail")]
        public async Task UpdateStepAndStatus_CardUpdateFails_DoesNotCallAutomation()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithDocumentAsync(updateDto.CardId)).ReturnsAsync(new List<Card> { card });
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                updateDto.WorkflowId)).ReturnsAsync(step);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(false);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.UpdateStepAndStatus(updateDto, "tenant", "email");

            // Assert
            Assert.False(result);
            _automationServices.Verify(s => s.StartExecutionByCardAsync(It.IsAny<AutomationServicesDto>()), Times.Never);
        }

        [Fact(DisplayName = "UpdateStatus should return app exception")]
        [Trait("UpdateStatus", "Fail")]
        public async Task UpdateStatus_CardUpdateStatusFails_CardNotFound()
        {
            // Arrange
            var updateCardStatusDto = CardFixture.FindValidCardStatusDto();
            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(updateCardStatusDto.CardId)).ReturnsAsync((List<Card>?)null);

            // Act
            await Assert.ThrowsAsync<AppException>(() => _cardServices.UpdateStatus(updateCardStatusDto));

            // Assert
            _cardRepositoryMock.Verify(s => s.FindCardOrBatchWithStepWorkflowAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "UpdateStatus should return true")]
        [Trait("UpdateStatus", "True")]
        public async Task UpdateStatus_CardUpdateStatusSuccess_ReturnsTrue()
        {
            // Arrange
            var updateCardStatusDto = CardFixture.FindValidCardStatusDto();
            var cardId = updateCardStatusDto.CardId;
            var card = CardFixture.FindValidCard();
            card.Step = CardFixture.FindValidStepWithWorkflow();
            _cardRepositoryMock
                .Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.UpdateStatus(updateCardStatusDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId), Times.Once);
        }

        [Fact(DisplayName = "ReprocessCard should throw AppException when card not found")]
        [Trait("ReprocessCard", "Failure")]
        public async Task ReprocessCard_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var cardId = 1;
            var tenant = "test-tenant";
            var email = "test@example.com";

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithDocumentAndStep(cardId))
                .ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => 
                _cardServices.ReprocessCard(cardId, tenant, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(CardLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "ReprocessCard should update card status and trigger automation")]
        [Trait("ReprocessCard", "Success")]
        public async Task ReprocessCard_Success_UpdatesStatusAndTriggersAutomation()
        {
            // Arrange
            var cardId = 1;
            var tenant = "test-tenant";
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();
            card.Step = step;

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithDocumentAndStep(cardId))
                .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(It.IsAny<Card>()))
                .Returns(true);
            _automationServices.Setup(s => s.ReprocessStepTool(It.IsAny<AutomationServicesDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.ReprocessCard(cardId, tenant, email);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(It.IsAny<Card>()), Times.Once);
            _automationServices.Verify(s => s.ReprocessStepTool(It.IsAny<AutomationServicesDto>()), Times.Once);
        }

        [Fact(DisplayName = "AssignRangeAsync throws AppException when card batch is not found (null)")]
        [Trait("AssignRangeAsync", "Fail")]
        public async Task AssignRangeAsync_CardNotFound_Null_ThrowsAppException()
        {
            //Arrange
            var cardId = 1;
            var userId = Guid.NewGuid();
            _mocker.GetMock<IWorkflowRepository>()
                .Setup(r => r.IsValidTeamUser(It.IsAny<IReadOnlyList<int>>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync((List<Card>?)null);

            var request = new AssignRangeDto(userId, new List<int> { cardId });

            //Act//Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignRangeAsync(request));
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal(CardLabel.NotFound, ex.LabelError);
        }

        [Fact(DisplayName = "AssignRangeAsync throws AppException when card batch is empty")]
        [Trait("AssignRangeAsync", "Fail")]
        public async Task AssignRangeAsync_CardNotFound_EmptyList_ThrowsAppException()
        {
            //Arrange
            var cardId = 1;
            var userId = Guid.NewGuid();
            _mocker.GetMock<IWorkflowRepository>()
                .Setup(r => r.IsValidTeamUser(It.IsAny<IReadOnlyList<int>>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            _cardRepositoryMock.Setup(repo => repo.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(new List<Card>());

            var request = new AssignRangeDto(userId, new List<int> { cardId });

            // Act//Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignRangeAsync(request));
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal(CardLabel.NotFound, ex.LabelError);
        }

        [Fact(DisplayName = "AssignRangeAsync assigns user on single card and audits Assign")]
        [Trait("AssignRangeAsync", "Success")]
        public async Task AssignRangeAsync_ValidSingleCard_ReturnsTrue_AndAuditsAssign()
        {
            //Arrange
            var userId = Guid.Parse("20c41dd6-1518-468b-8b0c-b5d8c0d31dec");
            var cardId = 1;
            var card = CardFixture.FindValidCard();
            card.Step = CardFixture.FindValidStepWithWorkflow();

            _cardRepositoryMock.Setup(repo => repo.FindByCardIdsAsync(It.Is<IReadOnlyList<int>>(ids => ids.Count == 1 && ids[0] == cardId)))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            _mocker.GetMock<IWorkflowRepository>()
                .Setup(r => r.IsValidTeamUser(It.IsAny<IReadOnlyList<int>>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            //Act
            var result = await _cardServices.AssignRangeAsync(new AssignRangeDto(userId, new List<int> { cardId }));

            //Assert
            Assert.True(result);
            Assert.Equal(userId, card.AssignedUserId);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.IsAny<List<Card>>()), Times.Once);
            auditCardServiceMock.Verify(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                AuditCardActionType.Assign,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "AssignRangeAsync assigns user to all cards in document batch")]
        [Trait("AssignRangeAsync", "DocumentBatch")]
        public async Task AssignRangeAsync_WithDocumentBatch_UpdatesAllBatchCards()
        {
            //Arrange
            var userId = Guid.Parse("20c41dd6-1518-468b-8b0c-b5d8c0d31dec");
            var documentBatchId = 100;
            var batchCards = CardFixture.FindDocumentBatchCardsWithoutAssignedUser(documentBatchId);

            _cardRepositoryMock.Setup(repo => repo.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(batchCards);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            _mocker.GetMock<IWorkflowRepository>()
                .Setup(r => r.IsValidTeamUser(It.IsAny<IReadOnlyList<int>>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            //Act
            var result = await _cardServices.AssignRangeAsync(new AssignRangeDto(userId, new List<int> { 1, 2, 3 }));

            //Assert
            Assert.True(result);
            Assert.All(batchCards, c => Assert.Equal(userId, c.AssignedUserId));
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.Is<List<Card>>(l => l.Count == 3)), Times.Once);
        }

        [Fact(DisplayName = "AssignRangeAsync returns false when UpdateList returns false")]
        [Trait("AssignRangeAsync", "Success")]
        public async Task AssignRangeAsync_UpdateListReturnsFalse_ReturnsFalse()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var cardId = 1;
            var card = CardFixture.FindValidCard();
            card.Step = CardFixture.FindValidStepWithWorkflow();

            _cardRepositoryMock.Setup(repo => repo.FindByCardIdsAsync(It.Is<IReadOnlyList<int>>(ids => ids.Count == 1 && ids[0] == cardId)))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(false);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            _mocker.GetMock<IWorkflowRepository>()
                .Setup(r => r.IsValidTeamUser(It.IsAny<IReadOnlyList<int>>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            //Act
            var result = await _cardServices.AssignRangeAsync(new AssignRangeDto(userId, new List<int> { cardId }));

            //Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "AssignRangeAsync throws ArgumentException when CardIds is empty")]
        [Trait("AssignRangeAsync", "Fail")]
        public async Task AssignRangeAsync_EmptyCardIds_ThrowsArgumentException()
        {
            //Arrange
            var request = new AssignRangeDto(Guid.NewGuid(), new List<int>());

            //Act /Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _cardServices.AssignRangeAsync(request));
        }

        [Fact(DisplayName = "AssignRangeAsync throws ArgumentNullException when request is null")]
        [Trait("AssignRangeAsync", "Fail")]
        public async Task AssignRangeAsync_NullRequest_ThrowsArgumentNullException()
        {
            //Act /Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _cardServices.AssignRangeAsync(null!));
        }

        [Fact(DisplayName = "AssignRangeAsync expands distinct ids once and persists in a single update")]
        [Trait("AssignRangeAsync", "Success")]
        public async Task AssignRangeAsync_DistinctCardIds_SingleExpandAndSingleUpdate()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var request = new AssignRangeDto(userId, new List<int> { 1, 1, 2 });

            var card1 = CardFixture.FindCard(1, 1, "C1");
            card1.Step = CardFixture.FindValidStepWithWorkflow();
            var card2 = CardFixture.FindCard(2, 2, "C2");
            card2.Step = CardFixture.FindValidStepWithWorkflow();

            _cardRepositoryMock.Setup(repo => repo.FindByCardIdsAsync(It.Is<IReadOnlyList<int>>(ids => ids.Count == 2 && ids.Contains(1) && ids.Contains(2))))
                .ReturnsAsync(new List<Card> { card1, card2 });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            _mocker.GetMock<IWorkflowRepository>()
                .Setup(r => r.IsValidTeamUser(It.IsAny<IReadOnlyList<int>>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            //Act
            var result = await _cardServices.AssignRangeAsync(request);

            //Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.FindByCardIdsAsync(It.Is<IReadOnlyList<int>>(ids => ids.Count == 2 && ids.Contains(1) && ids.Contains(2))), Times.Once);
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.IsAny<List<Card>>()), Times.Once);
        }

        [Fact(DisplayName = "FindCardsByDocumentIdWithStepWorkflowAsync returns same list when repository returns cards")]
        [Trait("FindCardsByDocumentIdWithStepWorkflowAsync", "Success")]
        public async Task FindCardsByDocumentIdWithStepWorkflowAsync_RepositoryReturnsList_ReturnsSameInstances()
        {
            // Arrange
            var documentId = 42;
            var cards = new List<Card> { CardFixture.FindValidCard() };
            _cardRepositoryMock.Setup(r => r.FindByDocumentIdCardListWithStepWorkflowAsync(documentId))
                .ReturnsAsync(cards);

            // Act
            var result = await _cardServices.FindCardsByDocumentIdWithStepWorkflowAsync(documentId);

            // Assert
            Assert.Same(cards, result);
            _cardRepositoryMock.Verify(r => r.FindByDocumentIdCardListWithStepWorkflowAsync(documentId), Times.Once);
        }

        [Fact(DisplayName = "FindCardsByDocumentIdWithStepWorkflowAsync returns empty list when repository returns null")]
        [Trait("FindCardsByDocumentIdWithStepWorkflowAsync", "Success")]
        public async Task FindCardsByDocumentIdWithStepWorkflowAsync_RepositoryReturnsNull_ReturnsEmptyReadOnlyList()
        {
            // Arrange
            var documentId = 7;
            _cardRepositoryMock.Setup(r => r.FindByDocumentIdCardListWithStepWorkflowAsync(documentId))
                .ReturnsAsync((List<Card>)null!);

            // Act
            var result = await _cardServices.FindCardsByDocumentIdWithStepWorkflowAsync(documentId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact(DisplayName = "FindCardsByDocumentBatchId returns null when repository returns null")]
        [Trait("FindCardsByDocumentBatchId", "Fail")]
        public async Task FindCardsByDocumentBatchId_RepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            const int batchId = 1;
            const int workflowId = 38;
            _cardRepositoryMock
                .Setup(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId))
                .ReturnsAsync((List<Card>)null!);

            // Act
            var result = await _cardServices.FindCardsByDocumentBatchId(batchId, workflowId);

            // Assert
            Assert.Null(result);
            _cardRepositoryMock.Verify(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId), Times.Once);
        }

        [Fact(DisplayName = "FindCardsByDocumentBatchId returns null when repository returns empty list")]
        [Trait("FindCardsByDocumentBatchId", "Fail")]
        public async Task FindCardsByDocumentBatchId_RepositoryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            const int batchId = 2;
            const int workflowId = 74;
            _cardRepositoryMock
                .Setup(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId))
                .ReturnsAsync(new List<Card>());

            // Act
            var result = await _cardServices.FindCardsByDocumentBatchId(batchId, workflowId);

            // Assert
            Assert.Null(result);
            _cardRepositoryMock.Verify(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId), Times.Once);
        }

        [Fact(DisplayName = "FindCardsByDocumentBatchId maps cards to CardBatchDto with correct fields")]
        [Trait("FindCardsByDocumentBatchId", "Success")]
        public async Task FindCardsByDocumentBatchId_WithCards_ReturnsMappedDtos()
        {
            // Arrange
            const int batchId = 50;
            const int workflowId = 38;
            var cards = CardFixture.FindBatchCardsForWorkflow(batchId, workflowId, count: 2);
            _cardRepositoryMock
                .Setup(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId))
                .ReturnsAsync(cards);

            // Act
            var result = await _cardServices.FindCardsByDocumentBatchId(batchId, workflowId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.CardId == cards[0].Id && x.DocumentId == cards[0].DocumentId && x.DocumentName == cards[0].Name);
            Assert.Contains(result, x => x.CardId == cards[1].Id && x.DocumentId == cards[1].DocumentId && x.DocumentName == cards[1].Name);
            _cardRepositoryMock.Verify(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId), Times.Once);
        }

        [Fact(DisplayName = "FindCardsByDocumentBatchId maps WorkflowId from card Step")]
        [Trait("FindCardsByDocumentBatchId", "Success")]
        public async Task FindCardsByDocumentBatchId_MapsWorkflowIdFromStep()
        {
            // Arrange
            const int batchId = 50;
            const int workflowId = 74;
            var cards = CardFixture.FindBatchCardsForWorkflow(batchId, workflowId, count: 2);
            _cardRepositoryMock
                .Setup(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId))
                .ReturnsAsync(cards);

            // Act
            var result = await _cardServices.FindCardsByDocumentBatchId(batchId, workflowId);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, dto => Assert.Equal(workflowId, dto.WorkflowId));
        }

        [Fact(DisplayName = "FindCardsByDocumentBatchId maps WorkflowId as 0 when card has no Step")]
        [Trait("FindCardsByDocumentBatchId", "Success")]
        public async Task FindCardsByDocumentBatchId_CardWithNullStep_MapsWorkflowIdAsZero()
        {
            // Arrange
            const int batchId = 50;
            const int workflowId = 38;
            var card = CardFixture.FindCard(1, 10, "Doc 1", batchId);
            _cardRepositoryMock
                .Setup(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId))
                .ReturnsAsync(new List<Card> { card });

            // Act
            var result = await _cardServices.FindCardsByDocumentBatchId(batchId, workflowId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(0, result.First().WorkflowId);
        }

        [Fact(DisplayName = "FindCardsByDocumentBatchId delegates to FindByDocumentBatchIdAndWorkflow and not FindByDocumentBatchId")]
        [Trait("FindCardsByDocumentBatchId", "Success")]
        public async Task FindCardsByDocumentBatchId_DelegatesToWorkflowScopedMethod()
        {
            // Arrange
            const int batchId = 10;
            const int workflowId = 120;
            _cardRepositoryMock
                .Setup(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId))
                .ReturnsAsync(CardFixture.FindBatchCardsForWorkflow(batchId, workflowId));

            // Act
            await _cardServices.FindCardsByDocumentBatchId(batchId, workflowId);

            // Assert
            _cardRepositoryMock.Verify(r => r.FindByDocumentBatchIdAndWorkflow(batchId, workflowId), Times.Once);
            _cardRepositoryMock.Verify(r => r.FindByDocumentBatchId(It.IsAny<int>()), Times.Never);
        }

        [Fact(DisplayName = "AssignRangeAsync throws when user is not valid for the card ids")]
        [Trait("AssignRangeAsync", "Fail")]
        public async Task AssignRangeAsync_UserNotInTeam_ThrowsAppException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mocker.GetMock<IWorkflowRepository>()
                .Setup(r => r.IsValidTeamUser(It.IsAny<IReadOnlyList<int>>(), userId))
                .ReturnsAsync(false);

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                _cardServices.AssignRangeAsync(new AssignRangeDto(userId, new List<int> { 1 })));

            // Assert
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal(CardLabel.UserCannotBeAssigned, ex.LabelError);
            _cardRepositoryMock.Verify(r => r.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()), Times.Never);
        }

        [Fact(DisplayName = "AssignRangeAsync throws ArgumentException when CardIds is null")]
        [Trait("AssignRangeAsync", "Fail")]
        public async Task AssignRangeAsync_NullCardIds_ThrowsArgumentException()
        {
            // Arrange
            var assignRangeDto = new AssignRangeDto(Guid.NewGuid(), null!);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _cardServices.AssignRangeAsync(assignRangeDto));

            // Assert
            Assert.Contains("CardIds", ex.Message, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "AssignRangeAsync throws when fewer cards are returned than requested ids")]
        [Trait("AssignRangeAsync", "Fail")]
        public async Task AssignRangeAsync_PartialCardList_ThrowsAppException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mocker.GetMock<IWorkflowRepository>()
                .Setup(r => r.IsValidTeamUser(It.IsAny<IReadOnlyList<int>>(), userId))
                .ReturnsAsync(true);
            var oneCard = CardFixture.FindValidCard();
            oneCard.Step = CardFixture.FindValidStepWithWorkflow();
            _cardRepositoryMock.Setup(r => r.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(new List<Card> { oneCard });

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                _cardServices.AssignRangeAsync(new AssignRangeDto(userId, new List<int> { 1, 2 })));

            // Assert
            Assert.Equal(CardLabel.NotFound, ex.LabelError);
        }

        [Fact(DisplayName = "UpdateStepAndStatus keeps previous status when card is rejected")]
        [Trait("UpdateStepAndStatus", "Success")]
        public async Task UpdateStepAndStatus_RejectedCard_KeepsPreviousStatusId()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            var rejectedCard = CardFixture.FindRejectedCardWithDocument(9);
            rejectedCard.Step = CardFixture.FindValidStepWithWorkflow();
            var nextStep = CardFixture.FindWorkflowNextStep();

            _cardRepositoryMock.Setup(r => r.FindCardOrBatchWithDocumentAsync(updateDto.CardId))
                .ReturnsAsync(new List<Card> { rejectedCard });
            _stepRepositoryMock.Setup(r => r.FindByOrderAndWorkflowId(updateDto.NextStepOrder, updateDto.WorkflowId))
                .ReturnsAsync(nextStep);
            _cardRepositoryMock.Setup(r => r.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);
            _automationServices.Setup(s => s.StartExecutionByCardAsync(It.IsAny<AutomationServicesDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardServices.UpdateStepAndStatus(updateDto, "tenant", "email");

            // Assert
            Assert.True(result);
            Assert.Equal(nextStep.Id, rejectedCard.StepId);
            Assert.Equal(9, rejectedCard.StatusId);
        }

        [Fact(DisplayName = "UpdateStepAndStatus calls automation once per card in batch")]
        [Trait("UpdateStepAndStatus", "Success")]
        public async Task UpdateStepAndStatus_TwoCards_CallsAutomationTwice()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            var nextStep = CardFixture.FindWorkflowNextStep();
            var card1 = CardFixture.FindValidCard();
            card1.Step = CardFixture.FindValidStepWithWorkflow();
            var card2 = CardFixture.FindSecondaryCardSharingDocumentAndStep(card1, 2, "C2");

            _cardRepositoryMock.Setup(r => r.FindCardOrBatchWithDocumentAsync(updateDto.CardId))
                .ReturnsAsync(new List<Card> { card1, card2 });
            _stepRepositoryMock.Setup(r => r.FindByOrderAndWorkflowId(updateDto.NextStepOrder, updateDto.WorkflowId))
                .ReturnsAsync(nextStep);
            _cardRepositoryMock.Setup(r => r.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);
            _automationServices.Setup(s => s.StartExecutionByCardAsync(It.IsAny<AutomationServicesDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _cardServices.UpdateStepAndStatus(updateDto, "tenant", "email");

            // Assert
            _automationServices.Verify(s => s.StartExecutionByCardAsync(It.IsAny<AutomationServicesDto>()),
                Times.Exactly(2));
        }

        [Fact(DisplayName = "UpdateStatus throws when card list is empty")]
        [Trait("UpdateStatus", "Fail")]
        public async Task UpdateStatus_EmptyCardList_ThrowsAppException()
        {
            //Arrange
            var dto = CardFixture.FindValidCardStatusDto();
            _cardRepositoryMock.Setup(r => r.FindCardOrBatchWithStepWorkflowAsync(dto.CardId))
                .ReturnsAsync(new List<Card>());

            //Act /Assert
            await Assert.ThrowsAsync<AppException>(() => _cardServices.UpdateStatus(dto));
        }

        [Fact(DisplayName = "UpdateStatus skips audit when Step is null on all cards")]
        [Trait("UpdateStatus", "Success")]
        public async Task UpdateStatus_AllCardsMissingStep_SkipsAuditAndReturnsUpdateResult()
        {
            // Arrange
            var dto = CardFixture.FindValidCardStatusDto();
            var card = CardFixture.FindValidCard();
            card.Step = null;
            _cardRepositoryMock.Setup(r => r.FindCardOrBatchWithStepWorkflowAsync(dto.CardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(r => r.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();

            // Act
            var result = await _cardServices.UpdateStatus(dto);

            // Assert
            Assert.True(result);
            auditCardServiceMock.Verify(
                s => s.CreateBatchAndSaveAsync(
                    It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                    It.IsAny<AuditCardActionType>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps throws when Step is null on card DTO")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_StepNull_ThrowsArgumentException()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var dto = CardFixture.FindCardAnalysisDtoWithNullStep(cardId);
            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(dto);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _cardServices.FindByIdAnalyzeWithSteps(cardId, headers));

            // Assert
            Assert.Contains($"Step not found for card {cardId}", ex.Message, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps sets CanAnswer true when OCR and Embeddings executions are Ready")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_BothOcrAndEmbeddingsReady_CanAnswerTrue()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var cardDto = CardFixture.FindValidCardAnalysisDto(cardId);
            var workflow = WorkflowFixture.FindMinimalAnalyzeWorkflow();

            var execs = new List<StepToolExecution>
            {
                CardFixture.CreateStepToolExecutionWithToolTypeName(1, cardId, 1, HandlersTypes.Ocr),
                CardFixture.CreateStepToolExecutionWithToolTypeName(2, cardId, 2, HandlersTypes.Embeddings)
            };

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(execs);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.True(result.CanAnswer);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps sets CanAnswer false when only OCR is Ready")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_OnlyOcrReady_CanAnswerFalse()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var cardDto = CardFixture.FindValidCardAnalysisDto(cardId);
            var workflow = WorkflowFixture.FindMinimalAnalyzeWorkflow();

            var execs = new List<StepToolExecution>
            {
                CardFixture.CreateStepToolExecutionWithToolTypeName(1, cardId, 1, HandlersTypes.Ocr)
            };

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(execs);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.False(result.CanAnswer);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps skips outputs when StepTool.Tool is null")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_NullToolOnOutput_SkipsOutput()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var cardDto = CardFixture.FindCardAnalysisDtoWithNullToolOnOutput(cardId);
            var workflow = WorkflowFixture.FindMinimalAnalyzeWorkflow("W", "S", "T", 2, "Prompt");

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.Empty(result.Steps[0].Outputs);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps yields no extracted fields for whitespace-only output value")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_WhitespaceOutputValue_NoExtractedFields()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var cardDto = CardFixture.FindCardAnalysisDtoWithWhitespaceValueOutput(cardId);
            var workflow = WorkflowFixture.FindMinimalAnalyzeWorkflow("W", "S", "T", 2, "Prompt");

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.Empty(result.Steps[0].Outputs);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps treats empty JSON object as non-JSON plain value")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_EmptyJsonObject_FallsBackToSingleField()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var cardDto = CardFixture.FindCardAnalysisDtoWithEmptyJsonObjectOutput(cardId);
            var workflow = WorkflowFixture.FindMinimalAnalyzeWorkflow("W", "S", "ToolName", 2, "Prompt");

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.Single(result.Steps[0].Outputs);
            Assert.Equal("{}", result.Steps[0].Outputs[0].Value);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps throws AppException when JSON in braces fails to parse")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_MalformedJsonObject_ThrowsAppException()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var cardDto = CardFixture.FindCardAnalysisDtoWithJsonThatThrowsOnParse(cardId);
            var workflow = WorkflowFixture.FindMinimalAnalyzeWorkflow("W", "S", "T", 2, "Prompt");

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                _cardServices.FindByIdAnalyzeWithSteps(cardId, headers));

            // Assert
            Assert.Equal(ErrorCode.DefaultError, ex.ErrorCode);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps resolves prompt label via IPromptServices and caches by prompt id")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_PromptParameter_CallsFindByIdOnceForSamePromptId()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var promptId = 99;
            var cardDto = CardFixture.FindCardAnalysisDtoWithPromptOutputsUsingPromptParameter(cardId, promptId);
            var workflow = WorkflowFixture.FindMinimalAnalyzeWorkflow("W", "S", "T", 2, HandlersTypes.Prompt);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);
            const string promptName = "Prompt nome país";
            _mocker.GetMock<IPromptServices>().Setup(s => s.FindById(promptId))
                .Returns(new PromptDto { Id = promptId, Text = "Resolved Prompt", Name = promptName });

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.All(result.Steps[0].Outputs, o => Assert.Equal(promptName, o.ToolName));
            _mocker.GetMock<IPromptServices>().Verify(s => s.FindById(promptId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps resolves questionnaire title for Quiz tool type")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_QuizParameter_UsesQuestionnaireTitleAsToolName()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var questionnaireId = 7;
            var cardDto = CardFixture.FindCardAnalysisDtoWithQuizOutputUsingQuestionnaireParameter(cardId, questionnaireId);

            var workflow = WorkflowFixture.FindMinimalAnalyzeWorkflow("W", "S", "QuizTool", 5, HandlersTypes.Quiz);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);
            var questionnaire = DocumentFixture.FindValidQuestionnaireDto();
            questionnaire.Id = questionnaireId;
            questionnaire.Title = "My Questionnaire";
            _mocker.GetMock<IQuestionnaireServices>().Setup(s => s.FindById(questionnaireId))
                .Returns(questionnaire);

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.Single(result.Steps[0].Outputs);
            Assert.Equal("My Questionnaire", result.Steps[0].Outputs[0].ToolName);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithSteps uses API template name for API outputs")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_ApiTool_UsesTemplateNameAsToolName()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var apiTemplateId = 7;
            var apiTemplateName = "Busca dados";
            var cardDto = CardFixture.FindCardAnalysisDtoWithApiToolOutput(cardId, "Test Tool", apiTemplateId);

            var workflow = WorkflowFixture.FindMinimalAnalyzeWorkflow("W", "S", "API Tool", 4, HandlersTypes.API);

            _cardRepositoryMock.Setup(a => a.FindByIdWithDocumentAndWorkflow(cardId)).ReturnsAsync(cardDto);
            _mocker.GetMock<IWorkflowRepository>().Setup(r => r.FindByIdForAnalyze(It.IsAny<int>())).ReturnsAsync(workflow);
            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync([]);
            _mocker.GetMock<IApiTemplateServices>()
                .Setup(s => s.FindById(apiTemplateId))
                .ReturnsAsync(new ApiTemplateDto { Id = apiTemplateId, Name = apiTemplateName });

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.Equal("Test Tool", result.Steps[0].Outputs[0].Label);
            Assert.Equal(apiTemplateName, result.Steps[0].Outputs[0].ToolName);
        }

        [Fact(DisplayName = "ReprocessCard should handle automation service exceptions")]
        [Trait("ReprocessCard", "Failure")]
        public async Task ReprocessCard_AutomationServiceThrowsException_ThrowsException()
        {
            // Arrange
            var cardId = 1;
            var tenant = "test-tenant";
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            var step = CardFixture.FindValidStep();
            card.Step = step;

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithDocumentAndStep(cardId))
                .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(It.IsAny<Card>()))
                .Returns(true);
            _automationServices.Setup(s => s.ReprocessStepTool(It.IsAny<AutomationServicesDto>()))
                .ThrowsAsync(new Exception("Automation service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _cardServices.ReprocessCard(cardId, tenant, email));

            _cardRepositoryMock.Verify(repo => repo.Update(It.IsAny<Card>()), Times.Once);
        }

        [Fact(DisplayName = "FinalizeRangeAsync throws ArgumentNullException when request is null")]
        [Trait("FinalizeRangeAsync", "Fail")]
        public async Task FinalizeRangeAsync_NullRequest_ThrowsArgumentNullException()
        {
            //Act /Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _cardServices.FinalizeRangeAsync(null!));
        }

        [Fact(DisplayName = "FinalizeRangeAsync throws ArgumentException when CardIds is null")]
        [Trait("FinalizeRangeAsync", "Fail")]
        public async Task FinalizeRangeAsync_NullCardIds_ThrowsArgumentException()
        {
            //Arrange
            var request = new FinalizeRangeDto(1, null!);

            //Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _cardServices.FinalizeRangeAsync(request));

            //Assert
            Assert.Contains("CardIds", ex.Message, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "FinalizeRangeAsync throws ArgumentException when CardIds is empty")]
        [Trait("FinalizeRangeAsync", "Fail")]
        public async Task FinalizeRangeAsync_EmptyCardIds_ThrowsArgumentException()
        {
            //Arrange
            var request = new FinalizeRangeDto(1, new List<int>());

            //Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _cardServices.FinalizeRangeAsync(request));

            //Assert
            Assert.Contains("CardIds", ex.Message, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "FinalizeRangeAsync throws AppException when repository returns null")]
        [Trait("FinalizeRangeAsync", "Fail")]
        public async Task FinalizeRangeAsync_RepositoryReturnsNull_ThrowsAppException()
        {
            //Arrange
            _cardRepositoryMock
                .Setup(r => r.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync((List<Card>?)null);

            var request = new FinalizeRangeDto(2, new List<int> { 1 });

            //Act
            var ex = await Assert.ThrowsAsync<AppException>(() => _cardServices.FinalizeRangeAsync(request));

            //Assert
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal(CardLabel.NotFound, ex.LabelError);
        }

        [Fact(DisplayName = "FinalizeRangeAsync throws AppException when repository returns empty list")]
        [Trait("FinalizeRangeAsync", "Fail")]
        public async Task FinalizeRangeAsync_RepositoryReturnsEmptyList_ThrowsAppException()
        {
            //Arrange
            _cardRepositoryMock
                .Setup(r => r.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(new List<Card>());

            var request = new FinalizeRangeDto(2, new List<int> { 1 });

            //Act /Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _cardServices.FinalizeRangeAsync(request));

            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal(CardLabel.NotFound, ex.LabelError);
        }

        [Fact(DisplayName = "FinalizeRangeAsync throws AppException when fewer cards are returned than requested ids")]
        [Trait("FinalizeRangeAsync", "Fail")]
        public async Task FinalizeRangeAsync_PartialCardList_ThrowsAppException()
        {
            //Arrange
            var card = CardFixture.FindValidCard();
            _cardRepositoryMock
                .Setup(r => r.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(new List<Card> { card });

            var request = new FinalizeRangeDto(2, new List<int> { 1, 2 });

            //Act /Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _cardServices.FinalizeRangeAsync(request));

            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal(CardLabel.NotFound, ex.LabelError);
        }

        [Fact(DisplayName = "FinalizeRangeAsync updates card status and calls audit with Finalize action type")]
        [Trait("FinalizeRangeAsync", "Success")]
        public async Task FinalizeRangeAsync_ValidSingleCardWithStep_UpdatesStatusAndAudits()
        {
            // Arrange
            var statusId = 5;
            var card = CardFixture.FindValidCard();
            card.Step = CardFixture.FindValidStepWithWorkflow();

            _cardRepositoryMock
                .Setup(r => r.FindByCardIdsAsync(It.Is<IReadOnlyList<int>>(ids => ids.Count == 1)))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(r => r.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var request = new FinalizeRangeDto(statusId, new List<int> { card.Id });

            // Act
            var result = await _cardServices.FinalizeRangeAsync(request);

            // Assert
            Assert.True(result);
            Assert.Equal(statusId, card.StatusId);
            _cardRepositoryMock.Verify(r => r.UpdateList(It.IsAny<List<Card>>()), Times.Once);
            auditCardServiceMock.Verify(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                AuditCardActionType.Finalize,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "FinalizeRangeAsync updates all cards when multiple valid ids are provided")]
        [Trait("FinalizeRangeAsync", "Success")]
        public async Task FinalizeRangeAsync_MultipleCards_UpdatesAllAndPersistsInSingleCall()
        {
            // Arrange
            var statusId = 3;
            var card1 = CardFixture.FindCard(1, 1, "C1");
            card1.Step = CardFixture.FindValidStepWithWorkflow();
            var card2 = CardFixture.FindCard(2, 2, "C2");
            card2.Step = CardFixture.FindValidStepWithWorkflow();

            _cardRepositoryMock
                .Setup(r => r.FindByCardIdsAsync(It.Is<IReadOnlyList<int>>(ids => ids.Count == 2)))
                .ReturnsAsync(new List<Card> { card1, card2 });
            _cardRepositoryMock.Setup(r => r.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            _mocker.GetMock<IAuditCardService>()
                .Setup(s => s.CreateBatchAndSaveAsync(
                    It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                    It.IsAny<AuditCardActionType>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var request = new FinalizeRangeDto(statusId, new List<int> { 1, 2 });

            // Act
            var result = await _cardServices.FinalizeRangeAsync(request);

            // Assert
            Assert.True(result);
            Assert.All(new[] { card1, card2 }, c => Assert.Equal(statusId, c.StatusId));
            _cardRepositoryMock.Verify(r => r.UpdateList(It.Is<List<Card>>(cards => cards.Count == 2)), Times.Once);
        }

        [Fact(DisplayName = "FinalizeRangeAsync deduplicates CardIds and makes a single repository call")]
        [Trait("FinalizeRangeAsync", "Success")]
        public async Task FinalizeRangeAsync_DuplicateCardIds_DeduplicatesBeforeQuerying()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            card.Step = CardFixture.FindValidStepWithWorkflow();

            _cardRepositoryMock
                .Setup(r => r.FindByCardIdsAsync(It.Is<IReadOnlyList<int>>(ids => ids.Count == 1 && ids[0] == card.Id)))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(r => r.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            _mocker.GetMock<IAuditCardService>()
                .Setup(s => s.CreateBatchAndSaveAsync(
                    It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                    It.IsAny<AuditCardActionType>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var request = new FinalizeRangeDto(2, new List<int> { card.Id, card.Id, card.Id });

            // Act
            var result = await _cardServices.FinalizeRangeAsync(request);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(
                r => r.FindByCardIdsAsync(It.Is<IReadOnlyList<int>>(ids => ids.Count == 1)),
                Times.Once);
        }

        [Fact(DisplayName = "FinalizeRangeAsync skips audit when all cards have no Step")]
        [Trait("FinalizeRangeAsync", "Success")]
        public async Task FinalizeRangeAsync_CardsWithoutStep_SkipsAuditAndReturnsUpdateResult()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            card.Step = null;

            _cardRepositoryMock
                .Setup(r => r.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(r => r.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();

            var request = new FinalizeRangeDto(4, new List<int> { card.Id });

            // Act
            var result = await _cardServices.FinalizeRangeAsync(request);

            // Assert
            Assert.True(result);
            auditCardServiceMock.Verify(
                s => s.CreateBatchAndSaveAsync(
                    It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                    It.IsAny<AuditCardActionType>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact(DisplayName = "FinalizeRangeAsync audits only cards that have a Step")]
        [Trait("FinalizeRangeAsync", "Success")]
        public async Task FinalizeRangeAsync_MixedStepPresence_AuditsOnlyCardsWithStep()
        {
            // Arrange
            var cardWithStep = CardFixture.FindCard(1, 1, "WithStep");
            cardWithStep.Step = CardFixture.FindValidStepWithWorkflow();
            var cardWithoutStep = CardFixture.FindCard(2, 2, "NoStep");
            cardWithoutStep.Step = null;

            _cardRepositoryMock
                .Setup(r => r.FindByCardIdsAsync(It.Is<IReadOnlyList<int>>(ids => ids.Count == 2)))
                .ReturnsAsync(new List<Card> { cardWithStep, cardWithoutStep });
            _cardRepositoryMock.Setup(r => r.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(true);

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var request = new FinalizeRangeDto(3, new List<int> { 1, 2 });

            // Act
            var result = await _cardServices.FinalizeRangeAsync(request);

            // Assert
            Assert.True(result);
            auditCardServiceMock.Verify(s => s.CreateBatchAndSaveAsync(
                It.Is<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(list => list.Count == 1),
                AuditCardActionType.Finalize,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "FinalizeRangeAsync returns false when UpdateList returns false")]
        [Trait("FinalizeRangeAsync", "Fail")]
        public async Task FinalizeRangeAsync_UpdateListReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            card.Step = CardFixture.FindValidStepWithWorkflow();

            _cardRepositoryMock
                .Setup(r => r.FindByCardIdsAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(r => r.UpdateList(It.IsAny<List<Card>>())).ReturnsAsync(false);

            _mocker.GetMock<IAuditCardService>()
                .Setup(s => s.CreateBatchAndSaveAsync(
                    It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                    It.IsAny<AuditCardActionType>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var request = new FinalizeRangeDto(2, new List<int> { card.Id });

            // Act
            var result = await _cardServices.FinalizeRangeAsync(request);

            // Assert
            Assert.False(result);
        }
    }
}
