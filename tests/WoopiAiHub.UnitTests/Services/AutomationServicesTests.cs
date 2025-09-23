using Moq;
using Moq.AutoMock;
using System.Reflection.Metadata;
using WoopiAiHub.Application.Services;
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
            var workflow = WorkflowFixture.FindValidWorkflow();
            var workflows = new List<Workflow>() { workflow };
            var stepTool = AutomationFixture.FindValidStepTool();
            var stepTools = new List<StepTool>() { stepTool };

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();

            stepToolRepositoryMock.Setup(s => s.FindStepToolsByStepIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(stepTools);
            stepToolExecutionRepositoryMock.Setup(s => s.CreateRangeAsync(It.IsAny<List<StepToolExecution>>())).ReturnsAsync(true);

            // Act
            await _service.PrepareExecutionAsync(workflows);

            // Assert
            stepToolRepositoryMock.Verify(s => s.FindStepToolsByStepIdsAsync(It.IsAny<IEnumerable<int>>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(s => s.CreateRangeAsync(It.IsAny<List<StepToolExecution>>()), Times.Once);
        }

        [Fact(DisplayName = "StartExecutionByStep should execute StepTool when StepTool has no dependencies")]
        [Trait("StartExecutionByStep", "Success")]
        public async Task StartExecutionByStep_ShouldExecuteStepTool_WhenStepToolHasNoDependencies()
        {
            // Arrange
            var stepTool = AutomationFixture.FindValidStepTool();
            var stepToolDependency =  AutomationFixture.FindValidStepTool(5000);
            stepTool.UpdateDependencyStepTool(stepToolDependency);
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolType();
            stepTool.Tool = tool;

            var stepTools = new List<StepTool>() { stepTool };
            var step = WorkflowFixture.FindValidStep();
            step.StepTools = stepTools;
            var payload = AutomationFixture.FindValidExecutionMessageDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution;
            var input = "input";

            var toolOutputServicesMock = _mocker.GetMock<IToolOutputServices>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandlerServices>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<string>>();
            var handlerMock = _mocker.GetMock<IToolHandlerServices>();

            toolOutputServicesMock.Setup(s => s.GetInput(It.IsAny<int>(), It.IsAny<int>())).Returns(input);
            toolFactoryHandlerServicesMock.Setup(s => s.GetHandler(It.IsAny<ToolType>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(It.IsAny<string>())).Returns(payload);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>()));
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            await _service.StartExecutionByStepAsync(step);

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            toolOutputServicesMock.Verify(o => o.GetInput(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<ToolType>()), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(It.IsAny<string>()), Times.Once);

        }

        [Fact(DisplayName = "StartExecutionByStep should execute StepTool when StepTool has dependencies")]
        [Trait("StartExecutionByStep", "Success")]
        public async Task StartExecutionByStep_ShouldNotExecuteStepTool_WhenStepToolHasDependencies()
        {
            // Arrange
            var step = WorkflowFixture.FindValidStep();

            var toolOutputServicesMock = _mocker.GetMock<IToolOutputServices>();
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandlerServices>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<string>>();
            var handlerMock = _mocker.GetMock<IToolHandlerServices>();

            // Act
            await _service.StartExecutionByStepAsync(step);

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Never);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            toolOutputServicesMock.Verify(o => o.GetInput(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<ToolType>()), Times.Never);
            handlerMock.Verify(h => h.BuildPayload(It.IsAny<string>()), Times.Never);
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
            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandlerServices>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<string>>();
            var handlerMock = _mocker.GetMock<IToolHandlerServices>();

            // Act
            await _service.StartExecutionByWorkflowsAsync(workflows);

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Never);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            toolOutputServicesMock.Verify(o => o.GetInput(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<ToolType>()), Times.Never);
            handlerMock.Verify(h => h.BuildPayload(It.IsAny<string>()), Times.Never);
        }
    }
}
