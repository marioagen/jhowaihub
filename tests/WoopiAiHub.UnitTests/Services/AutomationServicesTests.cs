using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
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

        [Fact(DisplayName = "PrepareExecution should create StepToolExecutions")]
        [Trait("PrepareExecution", "Success")]
        public async Task PrepareExecution_ShouldCreateStepToolExecutions()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var workflows = new List<Workflow>
                    {
                        new Workflow(1, DateTime.UtcNow, 1, "WF")
                        {
                            Steps = new List<Step>
                            {
                                new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1)
                               {
                                  Cards = new List<Card> { new Card(1, DateTime.UtcNow,1,1,"name",1,true, guid) },
                                  StepTools = new List<StepTool> { new StepTool(1, DateTime.UtcNow, 1, 1, 1, 0, 0) }
                               }
                            }
                       }
            };

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();

            stepToolRepositoryMock
                          .Setup(r => r.FindStepToolsByStepIdsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(workflows.SelectMany(w => w.Steps.SelectMany(s => s.StepTools)).ToList());

            stepToolExecutionRepositoryMock
               .Setup(r => r.CreateRangeAsync(It.IsAny<List<StepToolExecution>>()))
                          .ReturnsAsync(true);

            // Act
            var result = await _service.PrepareExecutionAsync(workflows);

            // Assert
            Assert.True(result);
            stepToolRepositoryMock.Verify(s => s.FindStepToolsByStepIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.CreateRangeAsync(It.IsAny<List<StepToolExecution>>()), Times.Once);
        }

        [Fact(DisplayName = "PrepareExecution should return false ")]
        [Trait("PrepareExecution", "Fail")]
        public async Task PrepareExecutionAsync_Failure_ReturnsFalse()
        {
            // Arrange
            var workflows = new List<Workflow>
            {
                new Workflow(1, DateTime.UtcNow, 1, "WF")
                {
                    Steps = new List<Step>
                    {
                        new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1)
                        {
                            Cards = new List<Card>(),
                            StepTools = new List<StepTool>()
                        }
                    }
                }
            };
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();

            stepToolRepositoryMock
                .Setup(r => r.FindStepToolsByStepIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<StepTool>());

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
            handlerMock.Setup(h => h.BuildPayload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payload);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>()));
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            await _service.StartExecutionByStepAsync(step, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<ToolType>()), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(payload.Queue, payload.Message), Times.Once);
        }

        [Fact(DisplayName = "StartExecutionByStep should execute StepTool when StepTool has dependencies")]
        [Trait("StartExecutionByStep", "Success")]
        public async Task StartExecutionByStep_ShouldNotExecuteStepTool_WhenStepToolHasDependencies()
        {
            // Arrange
            var step = WorkflowFixture.FindValidStep();

            var toolOutputServicesMock = _mocker.GetMock<IToolOutputServices>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<string>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();

            // Act
            await _service.StartExecutionByStepAsync(step, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Never);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<ToolType>()), Times.Never);
            handlerMock.Verify(h => h.BuildPayload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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

            // Act & Assert
            await _service.StartExecutionByStepAsync(step,It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()); // Não deve lançar exceção
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

            var toolOutputServicesMock = _mocker.GetMock<IToolOutputServices>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<string>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();

            // Act
            await _service.StartExecutionByWorkflowsAsync("test", "test", "test", workflows);

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Never);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<ToolType>()), Times.Never);
            handlerMock.Verify(h => h.BuildPayload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
            var input = "input";

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var toolOutputServicesMock = _mocker.GetMock<IToolOutputServices>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();

            stepToolRepositoryMock.Setup(r => r.FindByStepIdAndOrderAsync(It.IsAny<int>(), 1)).ReturnsAsync(stepTool);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>()));
            toolFactoryHandlerServicesMock.Setup(s => s.GetHandler(It.IsAny<ToolType>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payload);
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>()));

            // Act
            await _service.StartExecutionByCardAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindByStepIdAndOrderAsync(1, 1), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(stepTool.Id, 1), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(tool.ToolType), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(It.IsAny<string>(), It.IsAny<string>(),input, stepTool.Id, 1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(payload.Queue, payload.Message), Times.Once);
        }

        [Fact(DisplayName = "StartExecutionByCardAsync should fail when there is no StepTool")]
        [Trait("StartExecutionByCardAsync", "Fail")]
        public async Task StartExecutionByCardAsync_Failure_NoStepTool()
        {
            // Arrange
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();

            stepToolRepositoryMock
                .Setup(r => r.FindByStepIdAndOrderAsync(It.IsAny<int>(), 1))
                .ReturnsAsync((StepTool)null);

            // Act & Assert
            await _service.StartExecutionByCardAsync(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
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

            var payload = AutomationFixture.FindValidExecutionMessageDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var input = "input";

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var toolOutputServicesMock = _mocker.GetMock<IToolOutputServices>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();

            stepToolRepositoryMock.Setup(r => r.FindDependentAsync(It.IsAny<int>())).ReturnsAsync(dependentStepTool);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>()));
            toolFactoryHandlerServicesMock.Setup(s => s.GetHandler(It.IsAny<ToolType>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),  It.IsAny<string>())).ReturnsAsync(payload);
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>()));

            // Act
            await _service.ContinueExecution(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindDependentAsync(1), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(dependentStepTool.Id, 1), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(tool.ToolType), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(It.IsAny<string>(), It.IsAny<string>(),input, dependentStepTool.Id, 1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(payload.Queue, payload.Message), Times.Once);
        }

        [Fact(DisplayName = "ContinueExecution should fail when there is no dependent StepTool")]
        [Trait("ContinueExecution", "Fail")]
        public async Task ContinueExecution_Failure_NoDependentStepTool()
        {
            // Arrange
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            stepToolRepositoryMock
                .Setup(r => r.FindDependentAsync(It.IsAny<int>()))
                .ReturnsAsync((StepTool)null);

            // Act & Assert
            await _service.ContinueExecution(1, 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()); // Não deve lançar exceção
        }
    }
}
