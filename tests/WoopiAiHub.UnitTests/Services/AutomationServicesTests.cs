using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(AutomationCollection))]
    public class AutomationServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly AutomationServices _service;

        public AutomationServicesTests()
        {
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<AutomationServices>();
        }

        [Fact(DisplayName = "PrepareExecutionAsync deve retornar true e criar execuções quando há cards e step tools válidos")]
        public async Task PrepareExecutionAsync_Sucesso_DeveCriarExecucoes()
        {
            // Arrange
            var workflows = WorkflowFixture.FindValidWorkflows(); ; // Crie um fixture que retorna workflows válidos
            var stepIds = workflows.SelectMany(wf => wf.Steps.Select(s => s.Id)).ToList();
            var stepTools = new List<StepTool> { new StepTool(1, DateTime.UtcNow, 1, 1, 1, 0, 0) };
            var activeCardIds = new List<int> { 10, 20 };
            var existingExecutions = new List<(int StepToolId, int CardId)>
            {
                (10,20)
            }; 

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();

            stepToolRepositoryMock.Setup(r => r.FindStepToolsByStepIdsAsync(stepIds)).ReturnsAsync(stepTools);
            cardRepositoryMock.Setup(r => r.FindActiveCardIdsInFirstStepAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(activeCardIds);
            stepToolExecutionRepositoryMock.Setup(r => r.FindExistingExecutionsAsync(activeCardIds)).ReturnsAsync(existingExecutions);
            stepToolExecutionRepositoryMock.Setup(r => r.CreateRangeAsync(It.IsAny<List<StepToolExecution>>())).ReturnsAsync(true);

            // Act
            var result = await _service.PrepareExecutionAsync(workflows);

            // Assert
            Assert.True(result);
            stepToolExecutionRepositoryMock.Verify(r => r.CreateRangeAsync(It.IsAny<List<StepToolExecution>>()), Times.Once);
        }

        [Fact(DisplayName = "PrepareExecutionAsync deve retornar false quando não há cards ativos")]
        public async Task PrepareExecutionAsync_Falha_SemCardsAtivos()
        {
            // Arrange
            var workflows = WorkflowFixture.FindValidWorkflows(); 
            var stepIds = workflows.SelectMany(wf => wf.Steps.Select(s => s.Id)).ToList();
            var stepTools = new List<StepTool> { new StepTool(1, DateTime.UtcNow, 1, 1, 1, 0, 0) };
            var activeCardIds = new List<int>(); // Nenhum card ativo

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();

            stepToolRepositoryMock.Setup(r => r.FindStepToolsByStepIdsAsync(stepIds)).ReturnsAsync(stepTools);
            cardRepositoryMock.Setup(r => r.FindActiveCardIdsInFirstStepAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(activeCardIds);

            // Act
            var result = await _service.PrepareExecutionAsync(workflows);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "PrepareExecutionAsync deve retornar false quando não há novas execuções para criar")]
        public async Task PrepareExecutionAsync_Falha_SemNovasExecucoes()
        {
            // Arrange
            var workflows = WorkflowFixture.FindValidWorkflows();
            var stepIds = workflows.SelectMany(wf => wf.Steps.Select(s => s.Id)).ToList();
            var stepTools = new List<StepTool> { new StepTool(1, DateTime.UtcNow, 1, 1, 1, 0, 0) };
            var activeCardIds = new List<int> { 10, 20 };
            var existingExecutions = new List<(int StepToolId, int CardId)>
            {
                (1, 10),
                (1, 20)
            }; // Todas execuções já existem

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();

            stepToolRepositoryMock.Setup(r => r.FindStepToolsByStepIdsAsync(stepIds)).ReturnsAsync(stepTools);
            cardRepositoryMock.Setup(r => r.FindActiveCardIdsInFirstStepAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(activeCardIds);
            stepToolExecutionRepositoryMock.Setup(r => r.FindExistingExecutionsAsync(activeCardIds)).ReturnsAsync(existingExecutions);

            // Act
            var result = await _service.PrepareExecutionAsync(workflows);

            // Assert
            Assert.False(result);
            stepToolExecutionRepositoryMock.Verify(r => r.CreateRangeAsync(It.IsAny<List<StepToolExecution>>()), Times.Never);
        }


        [Fact(DisplayName = "StartExecutionByStep should execute StepTool when StepTool has no dependencies")]
        [Trait("StartExecutionByStep", "Success")]
        public async Task StartExecutionByStep_ShouldExecuteStepTool_WhenStepToolHasNoDependencies()
        {
            // Arrange
            var stepTool = AutomationFixture.FindValidStepTool();
            var stepToolDependency = AutomationFixture.FindValidStepTool(5000);
            stepTool.UpdateDependencyStepTool(stepToolDependency);
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolType();
            stepTool.Tool = tool;
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var stepTools = new List<StepTool>() { stepTool };
            var step = WorkflowFixture.FindValidStep();
            step.StepTools = stepTools;
            var payload = AutomationFixture.FindValidExecutionMessageDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var input = "input";

            var toolOutputServicesMock = _mocker.GetMock<IToolOutputServices>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();

            toolFactoryHandlerServicesMock.Setup(s => s.GetHandler(It.IsAny<ToolType>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(automationDto, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payload);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>()));
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            await _service.StartExecutionByStepAsync(step, automationDto);

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<ToolType>()), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(automationDto,It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(payload.Queue, payload.Message), Times.Once);
        }

        [Fact(DisplayName = "StartExecutionByStep should execute StepTool when StepTool has dependencies")]
        [Trait("StartExecutionByStep", "Success")]
        public async Task StartExecutionByStep_ShouldNotExecuteStepTool_WhenStepToolHasDependencies()
        {
            // Arrange
            var step = WorkflowFixture.FindValidStep();
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var toolOutputServicesMock = _mocker.GetMock<IToolOutputServices>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<string>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();


            // Act
            await _service.StartExecutionByStepAsync(step, automationDto);

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Never);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<ToolType>()), Times.Never);
            handlerMock.Verify(h => h.BuildPayload(automationDto,It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "StartExecutionByStep fail when there is no StepTools")]
        [Trait("StartExecutionByStep", "Fail")]
        public async Task StartExecutionByStepAsync_Failure_NoStepTools()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var step = new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1)
            {
                Cards = new List<Card> { new Card(1, DateTime.UtcNow,2,1,"name",1,true, guid) },
                StepTools = new List<StepTool>() // Sem tools
            };
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            // Act & Assert
            await _service.StartExecutionByStepAsync(step,automationDto); // Não deve lançar exceção
        }

        [Fact(DisplayName = "StartExecutionByWorkflows should call StartExecutionByStep for steps with order 1")]
        [Trait("StartExecutionByWorkflows", "Success")]
        public async Task StartExecutionByWorkflows_ShouldCallStartExecutionByStep_ForStepsWithOrderOne()
        {
            // Arrange
            var step = WorkflowFixture.FindValidStep();
            step.Update(step.Name, 1, step.ProfileId, step.StatusId);
            var workflow = WorkflowFixture.FindValidWorkflow();
            workflow.Steps.Add(step);
            var workflows = new List<Workflow>() { workflow };
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var toolOutputServicesMock = _mocker.GetMock<IToolOutputServices>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<string>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();

            // Act
            await _service.StartExecutionByWorkflowsAsync(automationDto, workflows);

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Never);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<ToolType>()), Times.Never);
            handlerMock.Verify(h => h.BuildPayload(automationDto, It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "StartExecutionByCardAsync should execute StepTool for valid Step and Card")]
        [Trait("StartExecutionByCardAsync", "Success")]
        public async Task StartExecutionByCardAsync_ShouldExecuteStepTool_ForValidStepAndCard()
        {
            // Arrange
            var stepTool = AutomationFixture.FindValidStepTool();
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolType();
            stepTool.Tool = tool;

            var payload = AutomationFixture.FindValidExecutionMessageDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var handlerMock = _mocker.GetMock<IToolHandler>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();

            stepToolRepositoryMock.Setup(r => r.FindByStepIdAndOrderAsync(It.IsAny<int>(), 1)).ReturnsAsync(stepTool);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>())).Returns(Task.CompletedTask);
            toolFactoryHandlerServicesMock.Setup(s => s.GetHandler(It.IsAny<ToolType>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(automationDto, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payload);
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);

            // Act
            await _service.StartExecutionByCardAsync(automationDto);

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindByStepIdAndOrderAsync(It.IsAny<int>(), 1), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(stepTool.Id, It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(tool.ToolType), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(automationDto, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(payload.Queue, payload.Message), Times.Once);
        }


        [Fact(DisplayName = "StartExecutionByCardAsync should fail when there is no StepTool")]
        [Trait("StartExecutionByCardAsync", "Fail")]
        public async Task StartExecutionByCardAsync_Failure_NoStepTool()
        {
            // Arrange
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            stepToolRepositoryMock
                .Setup(r => r.FindByStepIdAndOrderAsync(It.IsAny<int>(), 1))
                .ReturnsAsync((StepTool)null);

            // Act & Assert
            await _service.StartExecutionByCardAsync(automationDto);
        }

        [Fact(DisplayName = "ContinueExecution should execute dependent StepTool for valid StepTool and Card")]
        [Trait("ContinueExecution", "Success")]
        public async Task ContinueExecution_ShouldExecuteDependentStepTool_ForValidStepToolAndCard()
        {
            // Arrange
            var dependentStepTool = AutomationFixture.FindValidStepTool();
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolType();
            dependentStepTool.Tool = tool;
            var automationDto = AutomationFixture.FindValidautomationServicesDto();
            var stepToolDto = AutomationFixture.FindValidStepToolDto();

            var payload = AutomationFixture.FindValidExecutionMessageDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var input = "input";

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var toolFactoryHandlerMock = _mocker.GetMock<IToolFactoryHandler>();
            var handlerMock = _mocker.GetMock<IToolHandler>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();

            stepToolRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(stepToolDto);
            stepToolRepositoryMock.Setup(r => r.FindDependentAsync(It.IsAny<int>())).ReturnsAsync(dependentStepTool);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>())).Returns(Task.CompletedTask);
            toolFactoryHandlerMock.Setup(s => s.GetHandler(It.IsAny<ToolType>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(automationDto, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payload);
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);

            // Act
            await _service.ContinueExecution(automationDto);

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindDependentAsync(It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(dependentStepTool.Id, It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            toolFactoryHandlerMock.Verify(s => s.GetHandler(tool.ToolType), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(automationDto, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(payload.Queue, payload.Message), Times.Once);
        }


        [Fact(DisplayName = "ContinueExecution should fail when there is no dependent StepTool")]
        [Trait("ContinueExecution", "Fail")]
        public async Task ContinueExecution_Failure_NoDependentStepTool()
        {
            // Arrange
            var automationDto = AutomationFixture.FindValidautomationServicesDto();
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            stepToolRepositoryMock
                .Setup(r => r.FindDependentAsync(It.IsAny<int>()))
                .ReturnsAsync((StepTool)null);

            // Act & Assert
            await _service.ContinueExecution(automationDto); // Não deve lançar exceção
        }

        [Fact(DisplayName = "ContinueExecution should advance to next step when card has AI profile")]
        [Trait("ContinueExecution", "AI Profile Advancement")]
        public async Task ContinueExecution_ShouldAdvanceToNextStep_WhenCardHasAiProfile()
        {
            // Arrange
            var dependentStepTool = AutomationFixture.FindValidStepTool();
            var stepToolDto = AutomationFixture.FindValidStepToolDto();
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolType();
            dependentStepTool.Tool = tool;
            
            var automationDto = AutomationFixture.FindValidautomationServicesDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var payload = AutomationFixture.FindValidExecutionMessageDto();

            // Create AI profile
            var aiProfile = new Domain.Models.Profile("IA", 1, DateTime.UtcNow);
            
            // Create current step with AI profile
            var currentStep = new Domain.Models.Step(1, DateTime.UtcNow, 1, "Current Step", 1, aiProfile.Id, 1);
            currentStep.Profile = aiProfile;
            
            // Create next step
            var nextStep = new Domain.Models.Step(2, DateTime.UtcNow, 1, "Next Step", 2, 2, 1);
            
            // Create card with AI step
            var card = new Domain.Models.Card(1, DateTime.UtcNow, currentStep.Id, 1, "Test Card", 1, true, null);
            card.Step = currentStep;
            
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolParameterRepositoryMock = _mocker.GetMock<IStepToolParameterRepository>();
            var stepToolOutputRepositoryMock = _mocker.GetMock<IStepToolOutputRepository>();
            var toolFactoryHandlerMock = _mocker.GetMock<IToolFactoryHandler>();
            var handlerMock = new Mock<IToolHandler>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();
            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            var stepRepositoryMock = _mocker.GetMock<IStepRepository>();

            // Setup mocks
            stepToolRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(stepToolDto);
            stepToolRepositoryMock.Setup(r => r.FindDependentAsync(It.IsAny<int>())).ReturnsAsync(dependentStepTool);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Domain.Models.StepToolExecution>())).Returns(Task.CompletedTask);
            stepToolParameterRepositoryMock.Setup(r => r.FindByStepToolId(It.IsAny<int>())).Returns("input");
            stepToolOutputRepositoryMock.Setup(r => r.FindByStepToolId(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync("output");
            toolFactoryHandlerMock.Setup(s => s.GetHandler(It.IsAny<Domain.Models.ToolType>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(automationDto, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payload);
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
            
            // Setup for AI profile advancement
            cardRepositoryMock.Setup(r => r.FindById(automationDto.CardId)).ReturnsAsync(card);
            stepRepositoryMock.Setup(r => r.FindByOrderAndWorkflowId(2, currentStep.WorkflowId)).ReturnsAsync(nextStep);
            cardRepositoryMock.Setup(r => r.Update(It.IsAny<Domain.Models.Card>())).Returns(true);

            // Act
            await _service.ContinueExecution(automationDto);

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindDependentAsync(It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(dependentStepTool.Id, It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Models.StepToolExecution>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
            
            // Verify AI profile advancement logic was called
            cardRepositoryMock.Verify(r => r.FindById(automationDto.CardId), Times.Once);
            stepRepositoryMock.Verify(r => r.FindByOrderAndWorkflowId(2, currentStep.WorkflowId), Times.Once);
            cardRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Models.Card>()), Times.Once);
        }

        [Fact(DisplayName = "ContinueExecution should not advance step when card has non-AI profile")]
        [Trait("ContinueExecution", "Non-AI Profile")]
        public async Task ContinueExecution_ShouldNotAdvanceStep_WhenCardHasNonAiProfile()
        {
            // Arrange
            var dependentStepTool = AutomationFixture.FindValidStepTool();
            var stepToolDto = AutomationFixture.FindValidStepToolDto();
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolType();
            dependentStepTool.Tool = tool;
            
            var automationDto = AutomationFixture.FindValidautomationServicesDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var payload = AutomationFixture.FindValidExecutionMessageDto();

            // Create normal (non-AI) profile
            var normalProfile = new Domain.Models.Profile("Normal Profile", 2, DateTime.UtcNow);
            
            // Create current step with normal profile
            var currentStep = new Domain.Models.Step(1, DateTime.UtcNow, 1, "Current Step", 1, normalProfile.Id, 1);
            currentStep.Profile = normalProfile;
            
            // Create card with normal step
            var card = new Domain.Models.Card(1, DateTime.UtcNow, currentStep.Id, 1, "Test Card", 1, true, null);
            card.Step = currentStep;
            
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolParameterRepositoryMock = _mocker.GetMock<IStepToolParameterRepository>();
            var stepToolOutputRepositoryMock = _mocker.GetMock<IStepToolOutputRepository>();
            var toolFactoryHandlerMock = _mocker.GetMock<IToolFactoryHandler>();
            var handlerMock = new Mock<IToolHandler>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();
            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            var stepRepositoryMock = _mocker.GetMock<IStepRepository>();

            // Setup mocks
            stepToolRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(stepToolDto);
            stepToolRepositoryMock.Setup(r => r.FindDependentAsync(It.IsAny<int>())).ReturnsAsync(dependentStepTool);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Domain.Models.StepToolExecution>())).Returns(Task.CompletedTask);
            stepToolParameterRepositoryMock.Setup(r => r.FindByStepToolId(It.IsAny<int>())).Returns("input");
            stepToolOutputRepositoryMock.Setup(r => r.FindByStepToolId(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync("output");
            toolFactoryHandlerMock.Setup(s => s.GetHandler(It.IsAny<Domain.Models.ToolType>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(automationDto, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payload);
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);
            
            // Setup for non-AI profile (should not advance)
            cardRepositoryMock.Setup(r => r.FindById(automationDto.CardId)).ReturnsAsync(card);

            // Act
            await _service.ContinueExecution(automationDto);

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindDependentAsync(It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(dependentStepTool.Id, It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Models.StepToolExecution>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            
            // Verify AI profile advancement logic was not triggered (no step update)
            cardRepositoryMock.Verify(r => r.FindById(automationDto.CardId), Times.Once);
            stepRepositoryMock.Verify(r => r.FindByOrderAndWorkflowId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            cardRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Models.Card>()), Times.Never);
        }
    }
}
