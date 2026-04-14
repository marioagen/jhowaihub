using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
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

            _cardServices = _mocker.CreateInstance<CardServices>();
        }

        [Fact(DisplayName = "Tests update Step and Status and throws an AppException when Card not found")]
        [Trait("UpdateStepAndStatus", "Fail")]
        public async Task UpdateStepAndStatus_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var updateDto = CardFixture.FindValidUpdateCardStepStatusDto();
            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _cardServices.UpdateStepAndStatus(updateDto, It.IsAny<string>(), It.IsAny<string>()));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(CardLabel.NotFound, exception.LabelError);
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

            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);


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
            _cardRepositoryMock.Setup(repo => repo.FindById(cardId))
                .ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _cardServices.UnassignUser(cardId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Card not found", exception.Message);
        }

        [Fact(DisplayName = "Tests update UnassignUser to null successfully")]
        [Trait("UnassignUser", "Success")]
        public async Task UnassignUser_Success()
        {
            //Arrange
            var cardId = 1;
            var card = CardFixture.FindValidCard();
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            //Act
            var result = await _cardServices.UnassignUser(cardId);

            //Assert
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
            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                .ReturnsAsync((Card?)null);

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignUser(updateAssignedUserDto));
            Assert.Equal(Domain.Enum.ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Card not found", exception.Message);
        }

        [Fact(DisplayName = "Tests update AssignedUser throws ArgumentNullException when userId is empty")]
        [Trait("AssignUser", "Fail")]
        public async Task UpdateAssignedUser_UserIdIsEmpty_ThrowsArgumentNullException()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            updateAssignedUserDto.UserId = Guid.Empty;

            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            // Act
            var exception =
                await Assert.ThrowsAsync<ArgumentNullException>(() => _cardServices.AssignUser(updateAssignedUserDto));

            // Assert
            _cardRepositoryMock.Verify(repo => repo.FindById(updateAssignedUserDto.CardId), Times.Never);
        }

        [Fact(DisplayName = "Tests update AssignedUser when User not in Team and throws AppException")]
        [Trait("AssignUser", "Fail")]
        public async Task AssignUser_UserNotInTeam_ThrowsAppException()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };
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
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };
            card.Step.Workflow.Teams = [DocumentFixture.FindValidTeam()];
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            updateAssignedUserDto.UserId = userId;

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(updateAssignedUserDto.CardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);

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
            //Arrange
            var cardId = 1;
            var documentBatchId = 100;
            var card = new Card(cardId, DateTime.UtcNow, 1, 1, "Card Name", 1, Guid.NewGuid(), documentBatchId);
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() };

            var batchCards = new List<Card> 
            { 
                card,
                new Card(2, DateTime.UtcNow, 1, 2, "Card 2", 1, Guid.NewGuid(), documentBatchId) 
                    { Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() } },
                new Card(3, DateTime.UtcNow, 1, 3, "Card 3", 1, Guid.NewGuid(), documentBatchId) 
                    { Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() } }
            };

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(batchCards);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardRepositoryMock = _mocker.GetMock<IAuditCardRepository>();
            auditCardRepositoryMock.Setup(a => a.AddRangeAsync(It.IsAny<IEnumerable<Domain.Models.Audit.AuditCard>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            //Act
            var result = await _cardServices.UnassignUser(cardId);

            //Assert
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
                var card = new Card(1, DateTime.UtcNow, 1, 1, "Card Name", 1, null, documentBatchId);
                card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1)
                {
                    Workflow = WorkflowFixture.FindValidWorkflow()
                };
                card.Step.Workflow.Teams = [DocumentFixture.FindValidTeam()];

                var batchCards = new List<Card> 
                { 
                    card,
                    new Card(2, DateTime.UtcNow, 1, 2, "Card 2", 1, null, documentBatchId) 
                        { Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() } },
                    new Card(3, DateTime.UtcNow, 1, 3, "Card 3", 1, null, documentBatchId) 
                        { Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() } }
                };

                var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
                updateAssignedUserDto.UserId = userId;

                _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(updateAssignedUserDto.CardId))
                    .ReturnsAsync(batchCards);
                _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);

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
                var card = new Card(1, DateTime.UtcNow, 1, 1, "Card Name", 1, null, documentBatchId);
                card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() };

                var batchCards = new List<Card> 
                { 
                    card,
                    new Card(2, DateTime.UtcNow, 1, 2, "Card 2", 1, null, documentBatchId) 
                        { Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() } },
                    new Card(3, DateTime.UtcNow, 1, 3, "Card 3", 1, null, documentBatchId) 
                        { Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() } }
                };

                _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId)).ReturnsAsync(batchCards);
                _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);

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

            var workflow = new Workflow(1, DateTime.Now, [], "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", string.Empty, true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            tool.ToolType = toolType;
            stepTool.Tool = tool;
            step.StepTools = [stepTool];
            step.Workflow = workflow;
            workflow.Steps = [step];

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

            var workflow = new Workflow(1, DateTime.Now, [], "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var ocrTool = new Tool(1, DateTime.Now, "OCR Tool", true, 1, 1, 1, false, null, null);
            var ocrToolType = new ToolType(1, DateTime.Now, "OCR", string.Empty, true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            ocrTool.ToolType = ocrToolType;
            stepTool.Tool = ocrTool;
            step.StepTools = [stepTool];
            step.Workflow = workflow;
            workflow.Steps = [step];

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

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var embeddingsTool = new Tool(1, DateTime.Now, "Embeddings Tool", true, 3, 1, 1, false, null, null);
            var embeddingsToolType = new ToolType(3, DateTime.Now, "Embeddings", string.Empty, true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            embeddingsTool.ToolType = embeddingsToolType;
            stepTool.Tool = embeddingsTool;
            step.StepTools = [stepTool];
            step.Workflow = workflow;
            workflow.Steps = [step];

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

            var workflow = new Workflow(1, DateTime.Now, [], "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", string.Empty, true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            tool.ToolType = toolType;
            stepTool.Tool = tool;
            step.StepTools = [stepTool];
            step.Workflow = workflow;
            workflow.Steps = [step];

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

            var workflow = new Workflow(1, DateTime.Now, [], "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", string.Empty, true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            tool.ToolType = toolType;
            stepTool.Tool = tool;
            step.StepTools = [stepTool];
            step.Workflow = workflow;
            workflow.Steps = [step];

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

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step1 = new Step(1, DateTime.Now, 1, "Step 1", 1, 1, 1);
            var step2 = new Step(2, DateTime.Now, 2, "Step 2", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", string.Empty, true);
            var stepTool1 = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            var stepTool2 = new StepTool(2, DateTime.Now, 2, 1, 1, 0, 0);

            tool.ToolType = toolType;
            stepTool1.Tool = tool;
            stepTool2.Tool = tool;
            step1.StepTools = [stepTool1];
            step2.StepTools = [stepTool2];
            step1.Workflow = workflow;
            step2.Workflow = workflow;
            workflow.Steps = [step1, step2];

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

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", string.Empty, true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            tool.ToolType = toolType;
            stepTool.Tool = tool;
            step.StepTools = [stepTool];
            step.Workflow = workflow;
            workflow.Steps = [step];

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

            var workflow = new Workflow(1, DateTime.Now, [], "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", string.Empty, true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            tool.ToolType = toolType;
            stepTool.Tool = tool;
            step.StepTools = [stepTool];
            step.Workflow = workflow;
            workflow.Steps = [step];

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
            var expectedDto = new CardHeaderDto
            {
                CardName = "Test Card",
                WorkflowName = "Test Workflow"
            };

            _cardRepositoryMock.Setup(repo => repo.FindHeaderInfoAsync(cardId))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _cardServices.FindHeaderInfoAsync(cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.CardName, result.CardName);
            Assert.Equal(expectedDto.WorkflowName, result.WorkflowName);
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
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);
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
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(false);

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
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };
            _cardRepositoryMock
                .Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);

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

        [Fact(DisplayName = "SetFailingCard should throw AppException when card not found")]
        [Trait("SetFailingCard", "Failure")]
        public async Task SetFailingCard_CardNotFound_ThrowsAppException()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync((Card?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => 
                _cardServices.SetFailingCard(cardId, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(CardLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "SetFailingCard should throw AppException when fail status not found")]
        [Trait("SetFailingCard", "Failure")]
        public async Task SetFailingCard_FailStatusNotFound_ThrowsAppException()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();

            var statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync(card);
            statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync((Status?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => 
                _cardServices.SetFailingCard(cardId, email));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
        }

        [Fact(DisplayName = "SetFailingCard should update card status to fail")]
        [Trait("SetFailingCard", "Success")]
        public async Task SetFailingCard_Success_UpdatesCardStatusToFail()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            var failStatus = new Status("Fail", "#FF0000", 1, DateTime.Now);

            var statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync(card);
            statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync(failStatus);
            unitOfWorkMock.Setup(u => u.BeginTransaction()).Callback(() => { });
            unitOfWorkMock.Setup(u => u.Commit()).Callback(() => { });
            _cardRepositoryMock.Setup(repo => repo.Update(It.IsAny<Card>())).Returns(true);

            // Act
            await _cardServices.SetFailingCard(cardId, email);

            // Assert
            _cardRepositoryMock.Verify(repo => repo.Update(It.IsAny<Card>()), Times.Once);
            unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "SetFailingCard should handle null email gracefully")]
        [Trait("SetFailingCard", "Success")]
        public async Task SetFailingCard_NullEmail_SucceedsWithoutNotification()
        {
            // Arrange
            var cardId = 1;
            string? email = null;
            var card = CardFixture.FindValidCard();
            var failStatus = new Status("Fail", "#FF0000", 1, DateTime.Now);

            var statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync(card);
            statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync(failStatus);
            unitOfWorkMock.Setup(u => u.BeginTransaction()).Callback(() => { });
            unitOfWorkMock.Setup(u => u.Commit()).Callback(() => { });
            _cardRepositoryMock.Setup(repo => repo.Update(It.IsAny<Card>())).Returns(true);

            // Act
            await _cardServices.SetFailingCard(cardId, email);

            // Assert
            _cardRepositoryMock.Verify(repo => repo.Update(It.IsAny<Card>()), Times.Once);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "SetFailingCard should rollback transaction on error")]
        [Trait("SetFailingCard", "Failure")]
        public async Task SetFailingCard_OnError_RollsBackTransaction()
        {
            // Arrange
            var cardId = 1;
            var email = "test@example.com";
            var card = CardFixture.FindValidCard();
            var failStatus = new Status("Fail", "#FF0000", 1, DateTime.Now);

            var statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _cardRepositoryMock.Setup(repo => repo.FindByIdWithExecutions(cardId))
                .ReturnsAsync(card);
            statusRepositoryMock.Setup(repo => repo.FindByName(It.IsAny<string>()))
                .ReturnsAsync(failStatus);
            unitOfWorkMock.Setup(u => u.BeginTransaction()).Callback(() => { });
            unitOfWorkMock.Setup(u => u.Rollback()).Callback(() => { });

            // Simulate error during update
            _cardRepositoryMock.Setup(repo => repo.Update(It.IsAny<Card>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => 
                _cardServices.SetFailingCard(cardId, email));

            unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            unitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
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

        [Fact(DisplayName = "AssignRange throws AppException when card batch is not found (null)")]
        [Trait("AssignRange", "Fail")]
        public async Task AssignRange_CardNotFound_Null_ThrowsAppException()
        {
            var cardId = 1;
            var userId = Guid.NewGuid();
            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync((List<Card>?)null);

            var ex = await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignRange(userId, cardId));
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal(CardLabel.NotFound, ex.LabelError);
        }

        [Fact(DisplayName = "AssignRange throws AppException when card batch is empty")]
        [Trait("AssignRange", "Fail")]
        public async Task AssignRange_CardNotFound_EmptyList_ThrowsAppException()
        {
            var cardId = 1;
            var userId = Guid.NewGuid();
            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(new List<Card>());

            var ex = await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignRange(userId, cardId));
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal(CardLabel.NotFound, ex.LabelError);
        }

        [Fact(DisplayName = "AssignRange assigns user on single card and audits Assign")]
        [Trait("AssignRange", "Success")]
        public async Task AssignRange_ValidSingleCard_ReturnsTrue_AndAuditsAssign()
        {
            var userId = Guid.Parse("20c41dd6-1518-468b-8b0c-b5d8c0d31dec");
            var cardId = 1;
            var card = CardFixture.FindValidCard();
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateRange(It.IsAny<List<Card>>())).Returns(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _cardServices.AssignRange(userId, cardId);

            Assert.True(result);
            Assert.Equal(userId, card.AssignedUserId);
            _cardRepositoryMock.Verify(repo => repo.UpdateRange(It.IsAny<List<Card>>()), Times.Once);
            auditCardServiceMock.Verify(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                AuditCardActionType.Assign,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "AssignRange assigns user to all cards in document batch")]
        [Trait("AssignRange", "DocumentBatch")]
        public async Task AssignRange_WithDocumentBatch_UpdatesAllBatchCards()
        {
            var userId = Guid.Parse("20c41dd6-1518-468b-8b0c-b5d8c0d31dec");
            var documentBatchId = 100;
            var card = new Card(1, DateTime.UtcNow, 1, 1, "Card 1", 1, null, documentBatchId);
            card.Step = new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };

            var batchCards = new List<Card>
            {
                card,
                new Card(2, DateTime.UtcNow, 1, 2, "Card 2", 1, null, documentBatchId)
                {
                    Step = new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() }
                },
                new Card(3, DateTime.UtcNow, 1, 3, "Card 3", 1, null, documentBatchId)
                {
                    Step = new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() }
                }
            };

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(1))
                .ReturnsAsync(batchCards);
            _cardRepositoryMock.Setup(repo => repo.UpdateRange(It.IsAny<List<Card>>())).Returns(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _cardServices.AssignRange(userId, 1);

            Assert.True(result);
            Assert.All(batchCards, c => Assert.Equal(userId, c.AssignedUserId));
            _cardRepositoryMock.Verify(repo => repo.UpdateRange(It.Is<List<Card>>(l => l.Count == 3)), Times.Once);
        }

        [Fact(DisplayName = "AssignRange returns false when UpdateRange returns false")]
        [Trait("AssignRange", "Success")]
        public async Task AssignRange_UpdateRangeReturnsFalse_ReturnsFalse()
        {
            var userId = Guid.NewGuid();
            var cardId = 1;
            var card = CardFixture.FindValidCard();
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateRange(It.IsAny<List<Card>>())).Returns(false);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _cardServices.AssignRange(userId, cardId);

            Assert.False(result);
        }

        [Fact(DisplayName = "UnassignRange throws AppException when card batch is not found")]
        [Trait("UnassignRange", "Fail")]
        public async Task UnassignRange_CardNotFound_ThrowsAppException()
        {
            var cardId = 1;
            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync((List<Card>?)null);

            var ex = await Assert.ThrowsAsync<AppException>(() => _cardServices.UnassignRange(cardId));
            Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            Assert.Equal(CardLabel.NotFound, ex.LabelError);
        }

        [Fact(DisplayName = "UnassignRange clears assigned user on single card and audits Unassign")]
        [Trait("UnassignRange", "Success")]
        public async Task UnassignRange_ValidSingleCard_ReturnsTrue_AndAuditsUnassign()
        {
            var cardId = 1;
            var card = CardFixture.FindValidCard();
            card.UpdateAssignedUser(Guid.NewGuid());
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(new List<Card> { card });
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _cardServices.UnassignRange(cardId);

            Assert.True(result);
            Assert.Null(card.AssignedUserId);
            auditCardServiceMock.Verify(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                AuditCardActionType.Unassign,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "UnassignRange clears all cards in document batch")]
        [Trait("UnassignRange", "DocumentBatch")]
        public async Task UnassignRange_WithDocumentBatch_ClearsAllBatchCards()
        {
            var documentBatchId = 100;
            var cardId = 1;
            var card = new Card(cardId, DateTime.UtcNow, 1, 1, "Card 1", 1, Guid.NewGuid(), documentBatchId);
            card.Step = new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };

            var batchCards = new List<Card>
            {
                card,
                new Card(2, DateTime.UtcNow, 1, 2, "Card 2", 1, Guid.NewGuid(), documentBatchId)
                {
                    Step = new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1) { Workflow = WorkflowFixture.FindValidWorkflow() }
                }
            };

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(cardId))
                .ReturnsAsync(batchCards);
            _cardRepositoryMock.Setup(repo => repo.UpdateList(It.IsAny<List<Card>>())).Returns(true);

            var currentUserServiceMock = _mocker.GetMock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            currentUserServiceMock.Setup(s => s.Id).Returns(Guid.NewGuid());

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _cardServices.UnassignRange(cardId);

            Assert.True(result);
            Assert.All(batchCards, c => Assert.Null(c.AssignedUserId));
            _cardRepositoryMock.Verify(repo => repo.UpdateList(It.Is<List<Card>>(l => l.Count == 2)), Times.Once);
        }

        [Fact(DisplayName = "AssignRangeAsync throws ArgumentException when CardIds is empty")]
        [Trait("AssignRangeAsync", "Fail")]
        public async Task AssignRangeAsync_EmptyCardIds_ThrowsArgumentException()
        {
            var request = new AssignRangeDto(Guid.NewGuid(), new List<int>());
            await Assert.ThrowsAsync<ArgumentException>(() => _cardServices.AssignRangeAsync(request));
        }

        [Fact(DisplayName = "AssignRangeAsync throws ArgumentNullException when request is null")]
        [Trait("AssignRangeAsync", "Fail")]
        public async Task AssignRangeAsync_NullRequest_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _cardServices.AssignRangeAsync(null!));
        }

        [Fact(DisplayName = "AssignRangeAsync processes distinct card ids only once each")]
        [Trait("AssignRangeAsync", "Success")]
        public async Task AssignRangeAsync_DuplicateCardIds_CallsAssignPerDistinctId()
        {
            var userId = Guid.NewGuid();
            var request = new AssignRangeDto(userId, new List<int> { 1, 1, 2 });

            var card1 = new Card(1, DateTime.UtcNow, 1, 1, "C1", 1, null, null);
            card1.Step = new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };
            var card2 = new Card(2, DateTime.UtcNow, 1, 2, "C2", 1, null, null);
            card2.Step = new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };

            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(1))
                .ReturnsAsync(new List<Card> { card1 });
            _cardRepositoryMock.Setup(repo => repo.FindCardOrBatchWithStepWorkflowAsync(2))
                .ReturnsAsync(new List<Card> { card2 });
            _cardRepositoryMock.Setup(repo => repo.UpdateRange(It.IsAny<List<Card>>())).Returns(true);

            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            auditCardServiceMock.Setup(s => s.CreateBatchAndSaveAsync(
                It.IsAny<IReadOnlyList<(int cardId, int workflowId, int documentId)>>(),
                It.IsAny<AuditCardActionType>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _cardServices.AssignRangeAsync(request);

            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithStepWorkflowAsync(1), Times.Once);
            _cardRepositoryMock.Verify(repo => repo.FindCardOrBatchWithStepWorkflowAsync(2), Times.Once);
        }

        [Fact(DisplayName = "UnassignRangeAsync throws ArgumentException when CardIds is empty")]
        [Trait("UnassignRangeAsync", "Fail")]
        public async Task UnassignRangeAsync_EmptyCardIds_ThrowsArgumentException()
        {
            var request = new UnassignRangeDto(new List<int>());
            await Assert.ThrowsAsync<ArgumentException>(() => _cardServices.UnassignRangeAsync(request));
        }

        [Fact(DisplayName = "UnassignRangeAsync throws ArgumentNullException when request is null")]
        [Trait("UnassignRangeAsync", "Fail")]
        public async Task UnassignRangeAsync_NullRequest_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _cardServices.UnassignRangeAsync(null!));
        }

        [Fact(DisplayName = "ReprocessCard should return true on success")]
        [Trait("ReprocessCard", "Success")]
        public async Task ReprocessCard_ReturnsTrue()
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
    }
}
