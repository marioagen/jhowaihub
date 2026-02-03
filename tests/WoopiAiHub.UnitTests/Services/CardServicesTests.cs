using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
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
            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync(card);
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

            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync(card);
            _automationServices.Setup(s => s.StartExecutionByCardAsync(automationDto)).Returns(Task.CompletedTask);
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                updateDto.WorkflowId)).ReturnsAsync(step);

            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);


            _stepToolRepositoryMock.Setup(repo => repo.FindByStepIdAndOrderAsync(1, 1))
                .ReturnsAsync(It.IsAny<StepTool>());

            // Act
            var result = await _cardServices.UpdateStepAndStatus(updateDto, "tenant", "email");

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
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

            _cardRepositoryMock.Setup(repo => repo.FindById(cardId))
                .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            //Act
            var result = await _cardServices.UnassignUser(cardId);

            //Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
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
            _cardRepositoryMock.Verify(repo => repo.FindById(updateAssignedUserDto.CardId), Times.Once);
        }

        [Fact(DisplayName = "Tests update AssignedUser when User not in Team and throws AppException")]
        [Trait("AssignUser", "Fail")]
        public async Task AssignUser_UserNotInTeam_ThrowsAppException()
        {
            // Arrange
            var card = CardFixture.FindValidCard();
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1);
            card.Step.Workflow = WorkflowFixture.FindValidWorkflow();
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();

            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                .ReturnsAsync(card);

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<AppException>(() => _cardServices.AssignUser(updateAssignedUserDto));
            Assert.Equal(Domain.Enum.ErrorCode.NotFound, exception.ErrorCode);
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
            card.Step = new Step(1, DateTime.Now, 1, "Step", 1, 1, 1);
            card.Step.Workflow = WorkflowFixture.FindValidWorkflow();
            card.Step.Workflow.Teams = new List<Team>() { DocumentFixture.FindValidTeam() };
            var updateAssignedUserDto = CardFixture.FindValidUpdateAssignedUserDto();
            updateAssignedUserDto.UserId = userId;

            _cardRepositoryMock.Setup(repo => repo.FindById(updateAssignedUserDto.CardId))
                .ReturnsAsync(card);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            // Act
            var result = await _cardServices.AssignUser(updateAssignedUserDto);

            // Assert
            Assert.True(result);
            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsSuccess")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_Success()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var document = DocumentFixture.FindValidDocument();

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });

            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);

            var outputValue = "{\"Campo1\": \"Valor1\", \"Campo2\": \"Valor2\"}";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"doc-{document.Id}", result.DocumentId);
            Assert.Equal(document.Name, result.Name);
            Assert.Equal(document.Description, result.Description);
            Assert.Equal(document.ReferenceFile, result.ReferenceFile);
            Assert.NotEmpty(result.Steps);
            cardRepository.Verify(a => a.FindById(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFail")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_Fail()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync((Card)null!);

            // Act / Assert
            await Assert.ThrowsAsync<AppException>(() => _cardServices.FindByIdAnalyzeWithSteps(cardId, headers));
            cardRepository.Verify(a => a.FindById(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFailsWhenDocumentNotFound")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_FailsWhenDocumentNotFound()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var card = new Card(cardId, DateTime.Now, 1, 1, "Card Test", 1, null);

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _cardServices.FindByIdAnalyzeWithSteps(cardId, headers));
            cardRepository.Verify(a => a.FindById(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFailsWhenWorkflowNotFound")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_FailsWhenWorkflowNotFound()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var document = DocumentFixture.FindValidDocument();
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, null);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _cardServices.FindByIdAnalyzeWithSteps(cardId, headers));
            cardRepository.Verify(a => a.FindById(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFiltersOCROutputs")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_FiltersOCROutputs()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var document = DocumentFixture.FindValidDocument();

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var ocrTool = new Tool(1, DateTime.Now, "OCR Tool", true, 1, 1, 1, false, null, null);
            var ocrToolType = new ToolType(1, DateTime.Now, "OCR", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            typeof(Tool).GetProperty("ToolType")!.SetValue(ocrTool, ocrToolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, ocrTool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });

            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);

            var outputValue = "{\"text\": \"OCR Result\"}";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            // OCR outputs should be filtered out, so no outputs in the step
            Assert.Empty(result.Steps[0].Outputs);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFiltersEmbeddingsOutputs")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_FiltersEmbeddingsOutputs()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var document = DocumentFixture.FindValidDocument();

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var embeddingsTool = new Tool(1, DateTime.Now, "Embeddings Tool", true, 3, 1, 1, false, null, null);
            var embeddingsToolType = new ToolType(3, DateTime.Now, "Embeddings", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            typeof(Tool).GetProperty("ToolType")!.SetValue(embeddingsTool, embeddingsToolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, embeddingsTool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });

            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);

            var outputValue = "{\"embedding\": \"[0.1, 0.2, 0.3]\"}";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            // Embeddings outputs should be filtered out
            Assert.Empty(result.Steps[0].Outputs);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsParsesJsonOutput")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_ParsesJsonOutput()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var document = DocumentFixture.FindValidDocument();

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });

            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);

            var outputValue = "{\"Nome\": \"João Silva\", \"Email\": \"joao@example.com\"}";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

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
            var document = DocumentFixture.FindValidDocument();

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });

            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);

            var outputValue = "This is a plain text response without JSON structure";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Single(result.Steps[0].Outputs);
            Assert.Equal("Test Tool", result.Steps[0].Outputs[0].Label);
            Assert.Equal(outputValue, result.Steps[0].Outputs[0].Value);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsHandlesMultipleSteps")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_HandlesMultipleSteps()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var document = DocumentFixture.FindValidDocument();

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step1 = new Step(1, DateTime.Now, 1, "Step 1", 1, 1, 1);
            var step2 = new Step(2, DateTime.Now, 2, "Step 2", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool1 = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            var stepTool2 = new StepTool(2, DateTime.Now, 2, 1, 1, 0, 0);

            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool1, tool);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool2, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step1, new List<StepTool> { stepTool1 });
            typeof(Step).GetProperty("StepTools")!.SetValue(step2, new List<StepTool> { stepTool2 });
            typeof(Step).GetProperty("Workflow")!.SetValue(step1, workflow);
            typeof(Step).GetProperty("Workflow")!.SetValue(step2, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step1, step2 });

            var card = new Card(cardId, DateTime.Now, step2.Id, document.Id, "Card Test", 1, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step1);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);

            var output1 = new StepToolOutput(1, DateTime.Now, 1, cardId, "{\"Field1\": \"Value1\"}");
            var output2 = new StepToolOutput(2, DateTime.Now, 2, cardId, "{\"Field2\": \"Value2\"}");
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output1, stepTool1);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output2, stepTool2);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output1, output2 });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

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
            var document = DocumentFixture.FindValidDocument();

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });

            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput>());

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Empty(result.Steps[0].Outputs);
            // Last processed step should still be the last step even if no outputs
            Assert.Equal("1", result.LastProcessedStepId);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsHandlesInvalidJsonGracefully")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_HandlesInvalidJsonGracefully()
        {
            // Arrange
            var cardId = 1;
            var headers = DocumentFixture.FindValidHeadersDto();
            var document = DocumentFixture.FindValidDocument();

            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);

            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });

            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);

            var outputValue = "{\"field\": \"value\", invalid json";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            stepToolExecutionRepository
                .Setup(r => r.FindByStepToolByCardIdAsync(cardId))
                .ReturnsAsync(new List<StepToolExecution>());

            // Act
            var result = await _cardServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Single(result.Steps[0].Outputs);
            // Should fall back to plain text display
            Assert.Equal("Test Tool", result.Steps[0].Outputs[0].Label);
            Assert.Equal(outputValue, result.Steps[0].Outputs[0].Value);
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

            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                updateDto.WorkflowId)).ReturnsAsync(step);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(true);

            _automationServices.Setup(s => s.StartExecutionByCardAsync(It.IsAny<AutomationServicesDto>()))
                .ThrowsAsync(new Exception("Automation service failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _cardServices.UpdateStepAndStatus(updateDto, "tenant", "email"));

            _cardRepositoryMock.Verify(repo => repo.Update(card), Times.Exactly(2));
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

            _cardRepositoryMock.Setup(repo => repo.FindById(updateDto.CardId)).ReturnsAsync(card);
            _stepRepositoryMock.Setup(repo => repo.FindByOrderAndWorkflowId(updateDto.NextStepOrder,
                updateDto.WorkflowId)).ReturnsAsync(step);
            _cardRepositoryMock.Setup(repo => repo.Update(card)).Returns(false);

            // Act
            var result = await _cardServices.UpdateStepAndStatus(updateDto, "tenant", "email");

            // Assert
            Assert.True(result);
            _automationServices.Verify(s => s.StartExecutionByCardAsync(It.IsAny<AutomationServicesDto>()), Times.Never);
        }
    }
}
