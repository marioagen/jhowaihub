using Moq;
using Moq.AutoMock;
using Newtonsoft.Json;
using Refit;
using System.Net;
using WoopiAiHub.Application.Services.Automation;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Connector;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services.Automation
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

        [Fact(DisplayName = "PrepareExecutionAsync should return true and create executions when there are valid cards and step tools")]
        [Trait("PrepareExecutionAsync", "Success")]
        public async Task PrepareExecutionAsync_Success_ShouldCreateExecutions()
        {
            // Arrange
            var workflows = WorkflowFixture.FindValidWorkflows();
            var stepIds = workflows.SelectMany(wf => wf.Steps.Select(s => s.Id)).ToList();
            var stepTool = WorkflowFixture.FindValidStepTool();
            stepTool.Step = WorkflowFixture.FindValidStep(workflows.First().Id);

            var stepTools = new List<StepTool> { stepTool };
            var activeCardIds = new List<int> { 10, 20 };
            var existingExecutions = new List<(int StepToolId, int CardId)>
            {
                (1, 10),
                (1, 20)
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

        [Fact(DisplayName = "PrepareExecutionAsync should return false when there are no active cards.")]
        [Trait("PrepareExecutionAsync", "Fail")]
        public async Task PrepareExecutionAsync_Failure_NoActiveCards()
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

        [Fact(DisplayName = "PrepareExecutionAsync should return false when there are no new executions to create")]
        [Trait("PrepareExecutionAsync", "Fail")]
        public async Task PrepareExecutionAsync_Failure_NoNewExecutions()
        {
            // Arrange
            var workflows = WorkflowFixture.FindValidWorkflows();
            var stepIds = workflows.SelectMany(wf => wf.Steps.Select(s => s.Id)).ToList();
            var stepTool = WorkflowFixture.FindValidStepTool();
            stepTool.Step = WorkflowFixture.FindValidStep();
            var stepTools = new List<StepTool> { stepTool };
            var activeCardIds = new List<int>();
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

            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();

            toolFactoryHandlerServicesMock.Setup(s => s.GetHandler(It.IsAny<string>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(automationDto, It.IsAny<StepToolParameter>(), It.IsAny<List<StepToolOutput>>(), It.IsAny<StepToolExecution>())).ReturnsAsync(payload);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>()));
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            await _service.StartExecutionByStepAsync(step, automationDto);

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<string>()), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(automationDto,It.IsAny<StepToolParameter>(), It.IsAny<List<StepToolOutput>>(), It.IsAny<StepToolExecution>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(payload.Queue, payload.Message!), Times.Once);
        }

        [Fact(DisplayName = "StartExecutionByStep should execute StepTool when StepTool has dependencies")]
        [Trait("StartExecutionByStep", "Success")]
        public async Task StartExecutionByStep_ShouldNotExecuteStepTool_WhenStepToolHasDependencies()
        {
            // Arrange
            var step = WorkflowFixture.FindValidStep();
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<string>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();

            // Act
            await _service.StartExecutionByStepAsync(step, automationDto);

            // Assert
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Never);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<string>()), Times.Never);
            handlerMock.Verify(h => h.BuildPayload(automationDto,It.IsAny<StepToolParameter>(), It.IsAny<List<StepToolOutput>>(), It.IsAny<StepToolExecution>()), Times.Never);
        }

        [Fact(DisplayName = "StartExecutionByStep fail when there is no StepTools")]
        [Trait("StartExecutionByStep", "Fail")]
        public async Task StartExecutionByStepAsync_Failure_NoStepTools()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var step = new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1)
            {
                Cards = new List<Card> { new Card(1, DateTime.UtcNow,2,1,"name",1, guid) },
                StepTools = new List<StepTool>()
            };
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            // Act
            var exception = await Record.ExceptionAsync(() => _service.StartExecutionByStepAsync(step, automationDto));

            // Assert
            Assert.Null(exception);
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

            var toolFactoryHandlerServicesMock = _mocker.GetMock<IToolFactoryHandler>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<string>>();
            var handlerMock = _mocker.GetMock<IToolHandler>();
            var usageDailyServicesMock = _mocker.GetMock<IUsageDailyServices>();

            usageDailyServicesMock.Setup(s => s.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                                  .ReturnsAsync(true);

            // Act
            await _service.StartExecutionByWorkflowsAsync(automationDto, workflows);

            // Assert
            usageDailyServicesMock.Verify(s => s.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Never);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<string>()), Times.Never);
            handlerMock.Verify(h => h.BuildPayload(automationDto, It.IsAny<StepToolParameter>(), It.IsAny<List<StepToolOutput>>(), It.IsAny<StepToolExecution>()), Times.Never);
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
            toolFactoryHandlerServicesMock.Setup(s => s.GetHandler(It.IsAny<string>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(automationDto, It.IsAny<StepToolParameter>(), It.IsAny<List<StepToolOutput>>(), It.IsAny<StepToolExecution>())).ReturnsAsync(payload);
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);

            // Act
            await _service.StartExecutionByCardAsync(automationDto);

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindByStepIdAndOrderAsync(It.IsAny<int>(), 1), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(stepTool.Id, It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            toolFactoryHandlerServicesMock.Verify(s => s.GetHandler(It.IsAny<string>()), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(automationDto, It.IsAny<StepToolParameter>(), It.IsAny<List<StepToolOutput>>(), It.IsAny<StepToolExecution>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(payload.Queue, payload.Message!), Times.Once);
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
                .ReturnsAsync((StepTool?)null);

            // Act
            var exception = await Record.ExceptionAsync(() => _service.StartExecutionByCardAsync(automationDto));

            // Assert
            Assert.Null(exception);
        }

        [Fact(DisplayName = "ContinueExecution should execute dependent StepTool for valid StepTool and Card")]
        [Trait("ContinueExecution", "Success")]
        public async Task ContinueExecution_ShouldExecuteDependentStepTool_ForValidStepToolAndCard()
        {
            // Arrange
            var dependentStepTool = AutomationFixture.FindValidStepTool();
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolTypeWithName("n8n");
            dependentStepTool.Tool = tool;
            var automationDto = AutomationFixture.FindValidautomationServicesDto();
            var stepToolDto = AutomationFixture.FindValidStepToolDto();

            var payload = AutomationFixture.FindValidExecutionMessageDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var toolFactoryHandlerMock = _mocker.GetMock<IToolFactoryHandler>();
            var handlerMock = _mocker.GetMock<IToolHandler>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();

            stepToolRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(stepToolDto);
            stepToolRepositoryMock.Setup(r => r.FindDependentAsync(It.IsAny<int>())).ReturnsAsync(dependentStepTool);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>())).Returns(Task.CompletedTask);
            toolFactoryHandlerMock.Setup(s => s.GetHandler(It.IsAny<string>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(It.IsAny<AutomationServicesDto>(), It.IsAny<StepToolParameter>(), It.IsAny<List<StepToolOutput>>(), It.IsAny<StepToolExecution>())).ReturnsAsync(payload);
            messagePublisherMock.Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);

            // Act
            await _service.ContinueExecution(automationDto);

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindDependentAsync(It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.FindByStepToolIdAndCardIdAsync(dependentStepTool.Id, It.IsAny<int>()), Times.Once);
            stepToolExecutionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            toolFactoryHandlerMock.Verify(s => s.GetHandler(It.IsAny<string>()), Times.Once);
            handlerMock.Verify(h => h.BuildPayload(It.IsAny<AutomationServicesDto>(), It.IsAny<StepToolParameter>(), It.IsAny<List<StepToolOutput>>(), It.IsAny<StepToolExecution>()), Times.Once);
            messagePublisherMock.Verify(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
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
                .ReturnsAsync((StepTool?)null);

            // Act
            var exception = await Record.ExceptionAsync(() => _service.ContinueExecution(automationDto));

            // Assert
            Assert.Null(exception);
        }

        [Fact(DisplayName = "FindN8nWorkflowsByToolId should throw an AppException when the tool is not found")]
        [Trait("FindN8nWorkflowsByToolId", "Fail")]
        public async Task FindN8nWorkflowsByToolId_ToolNotFound_ThrowsAppException()
        {
            // Arrange
            var _toolRepositoryMock = _mocker.GetMock<IToolRepository>();
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Tool?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.FindN8nWorkflowsByToolId(1));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
        }

        [Fact(DisplayName = "FindN8nWorkflowsByToolId should throw an AppException when the tool is not an N8N tool.")]
        [Trait("FindN8nWorkflowsByToolId", "Fail")]
        public async Task FindN8nWorkflowsByToolId_ToolIsNotN8n_ThrowsAppException()
        {
            // Arrange
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = new ToolType(1, DateTime.Now, "tool", "description", true);

            var _toolRepositoryMock = _mocker.GetMock<IToolRepository>();
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(tool);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.FindN8nWorkflowsByToolId(1));
            Assert.Equal(ErrorCode.InvalidValue, exception.ErrorCode);
        }

        [Fact(DisplayName = "FindN8nWorkflowsByToolId should throw an AppException when the API call fails.")]
        [Trait("FindN8nWorkflowsByToolId", "Fail")]
        public async Task FindN8nWorkflowsByToolId_ApiCallFails_ThrowsAppException()
        {
            // Arrange
            var keyVaultValue = Guid.NewGuid().ToString();
            var tool = ToolFixture.FindValidToolModelWithEmptyConnector();
            tool.ToolType = new ToolType(1, DateTime.Now, ConnectorNames.N8N, "description", true);
            var response = new ApiResponse<string>(new HttpResponseMessage(HttpStatusCode.BadRequest), string.Empty, new RefitSettings());

            var toolRepositoryMock = _mocker.GetMock<IToolRepository>();
            toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(tool);

            var apiClientMock = _mocker.GetMock<In8NConnector>();
            apiClientMock.Setup(x => x.FindWorkflows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(response);

            var apiClientFactoryMock = _mocker.GetMock<IApiClientFactory>();
            apiClientFactoryMock.Setup(factory => factory.Create(It.IsAny<string>()))
                                .Returns(apiClientMock.Object);

            var encryptionService = _mocker.GetMock<IEncryptionService>();
            encryptionService.Setup(k => k.Decrypt(It.IsAny<string>()))
                             .Returns(keyVaultValue);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.FindN8nWorkflowsByToolId(1));
            toolRepositoryMock.Verify(repo => repo.FindModelByIdAsync(It.IsAny<int>()), Times.Once);
            apiClientMock.Verify(x => x.FindWorkflows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            encryptionService.Verify(k => k.Decrypt(It.IsAny<string>()), Times.Once);
            Assert.Equal(ErrorCode.RefitApiError, exception.ErrorCode);
        }

        [Fact(DisplayName = "FindN8nWorkflowsByToolId succeeds and returns connector DTOs.")]
        [Trait("FindN8nWorkflowsByToolId", "Success")]
        public async Task FindN8nWorkflowsByToolId_Success_ReturnsConnectorDtos()
        {
            // Arrange
            var keyVaultValue = Guid.NewGuid().ToString();
            var tool = ToolFixture.FindValidToolModelWithEmptyConnector();
            tool.ToolType = new ToolType(1, DateTime.Now, ConnectorNames.N8N, "description", true);

            var webhookDataDtoList = AutomationFixture.FindValidWebhookDataDto();
            var responseContent = JsonConvert.SerializeObject(webhookDataDtoList);
            var response = new ApiResponse<string>(new HttpResponseMessage(HttpStatusCode.OK), responseContent, new RefitSettings());

            var _toolRepositoryMock = _mocker.GetMock<IToolRepository>();
            _toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(tool);

            var apiClientMock = _mocker.GetMock<In8NConnector>();            
            apiClientMock.Setup(api => api.FindWorkflows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(response);

            var apiClientFactoryMock = _mocker.GetMock<IApiClientFactory>();
            apiClientFactoryMock.Setup(factory => factory.Create(It.IsAny<string>()))
                .Returns(apiClientMock.Object);

            var encryptionService = _mocker.GetMock<IEncryptionService>();
            encryptionService.Setup(k => k.Decrypt(It.IsAny<string>()))
                             .Returns(keyVaultValue);

            // Act
            var result = await _service.FindN8nWorkflowsByToolId(1);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ConnectorDto>>(result);
        }

        [Fact(DisplayName = "FindN8nWebhookInputs should throw an AppException when the tool is not found")]
        [Trait("FindN8nWebhookInputs", "Fail")]
        public async Task FindN8nWebhookInputs_ToolNotFound_ThrowsAppException()
        {
            // Arrange
            var webhookInputDto = new WebhookInputDto { ToolId = 1, WorkflowId = Guid.NewGuid() };

            var toolRepositoryMock = _mocker.GetMock<IToolRepository>();
            toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Tool?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.FindN8nWebhookInputs(webhookInputDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
        }

        [Fact(DisplayName = "FindN8nWebhookInputs should throw an AppException when the tool is not an N8N tool")]
        [Trait("FindN8nWebhookInputs", "Fail")]
        public async Task FindN8nWebhookInputs_ToolIsNotN8n_ThrowsAppException()
        {
            // Arrange
            var webhookInputDto = new WebhookInputDto { ToolId = 1, WorkflowId = Guid.NewGuid() };
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = new ToolType(1, DateTime.Now, string.Empty, string.Empty, true);

            var toolRepositoryMock = _mocker.GetMock<IToolRepository>();
            toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(tool);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.FindN8nWebhookInputs(webhookInputDto));
            Assert.Equal(ErrorCode.InvalidValue, exception.ErrorCode);
        }

        [Fact(DisplayName = "FindN8nWebhookInputs should throw an AppException when the API call fails")]
        [Trait("FindN8nWebhookInputs", "Fail")]
        public async Task FindN8nWebhookInputs_ApiCallFails_ThrowsAppException()
        {
            // Arrange
            var webhookInputDto = new WebhookInputDto { ToolId = 1, WorkflowId = Guid.NewGuid() };
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = new ToolType(1, DateTime.Now, ConnectorNames.N8N, string.Empty, true);

            var response = new ApiResponse<string>(new HttpResponseMessage(HttpStatusCode.BadRequest), string.Empty, new RefitSettings());

            var toolRepositoryMock = _mocker.GetMock<IToolRepository>();
            toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                              .ReturnsAsync(tool);

            var apiClientMock = _mocker.GetMock<In8NConnector>();
            apiClientMock.Setup(api => api.FindWorkflowInputs(It.IsAny<string>()))
                         .ReturnsAsync(response);

            var apiClientFactoryMock = _mocker.GetMock<IApiClientFactory>();
            apiClientFactoryMock.Setup(factory => factory.Create(It.IsAny<string>()))
                                .Returns(apiClientMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.FindN8nWebhookInputs(webhookInputDto));
            Assert.Equal(ErrorCode.RefitApiError, exception.ErrorCode);
        }

        [Fact(DisplayName = "FindN8nWebhookInputs succeeds and returns form field DTOs.")]
        [Trait("FindN8nWebhookInputs", "Success")]
        public async Task FindN8nWebhookInputs_Success_ReturnsFormFieldDtos()
        {
            // Arrange
            var webhookInputDto = new WebhookInputDto { ToolId = 1, WorkflowId = Guid.NewGuid() };
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = new ToolType(1, DateTime.Now, ConnectorNames.N8N, string.Empty, true);

            var content = AutomationFixture.FindValidJson();
            var response = new ApiResponse<string>(new HttpResponseMessage(HttpStatusCode.OK), content, new RefitSettings());

            var toolRepositoryMock = _mocker.GetMock<IToolRepository>();
            toolRepositoryMock.Setup(repo => repo.FindModelByIdAsync(It.IsAny<int>()))
                              .ReturnsAsync(tool);

            var apiClientMock = _mocker.GetMock<In8NConnector>();

            apiClientMock.Setup(api => api.FindWorkflowInputs(It.IsAny<string>()))
                         .ReturnsAsync(response);

            var apiClientFactoryMock = _mocker.GetMock<IApiClientFactory>();
            apiClientFactoryMock.Setup(factory => factory.Create(It.IsAny<string>()))
                                .Returns(apiClientMock.Object);

            // Act
            var result = await _service.FindN8nWebhookInputs(webhookInputDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<FormFieldDto>>(result);
        }

        [Fact(DisplayName = "ContinueExecution should advance to next step when card has AI profile")]
        [Trait("ContinueExecution", "AI Profile Advancement")]
        public async Task ContinueExecution_ShouldAdvanceToNextStep_WhenCardHasAiProfile()
        {
            // Arrange
            var stepToolDto = AutomationFixture.FindValidStepToolDto();
            stepToolDto.Step!.Order = 2;
            var automationDto = AutomationFixture.FindValidautomationServicesDto();
            var stepTool = AutomationFixture.FindValidStepTool();

            var aiProfile = new Domain.Models.Profile("Avanço automático", 1, DateTime.UtcNow);

            var currentStep = new Domain.Models.Step(1, DateTime.UtcNow, 1, "Current Step", 1, aiProfile.Id, 1);
            currentStep.Profile = aiProfile;

            var nextStep = new Domain.Models.Step(2, DateTime.UtcNow, 1, "Next Step", 2, 2, 1);

            var card = new Domain.Models.Card(1, DateTime.UtcNow, currentStep.Id, 1, "Test Card", 1, null);
            card.Step = currentStep;
            
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            var stepRepositoryMock = _mocker.GetMock<IStepRepository>();
            var hubNotifierMock = _mocker.GetMock<IHubNotifier>();

            stepToolRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(stepToolDto);
            stepToolRepositoryMock.Setup(r => r.FindDependentAsync(It.IsAny<int>())).ReturnsAsync(stepTool);

            cardRepositoryMock.Setup(r => r.FindByIdWithStepAndProfile(It.IsAny<int>())).ReturnsAsync(card);
            stepRepositoryMock.Setup(r => r.FindByOrderAndWorkflowId(2, currentStep.WorkflowId)).ReturnsAsync(nextStep);
            cardRepositoryMock.Setup(r => r.Update(It.IsAny<Domain.Models.Card>())).Returns(true);
            hubNotifierMock.Setup(h => h.CardProgessAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            await _service.ContinueExecution(automationDto);

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindDependentAsync(It.IsAny<int>()), Times.Once);           
            cardRepositoryMock.Verify(r => r.FindByIdWithStepAndProfile(It.IsAny<int>()), Times.Once);
            stepRepositoryMock.Verify(r => r.FindByOrderAndWorkflowId(2, currentStep.WorkflowId), Times.Once);
            cardRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Models.Card>()), Times.Once);
            hubNotifierMock.Verify(h => h.CardProgessAsync(automationDto.Email, automationDto.CardId, 100.0, nextStep.Id, It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "ContinueExecution should not advance step when card has non-AI profile")]
        [Trait("ContinueExecution", "Non-AI Profile")]
        public async Task ContinueExecution_ShouldNotAdvanceStep_WhenCardHasNonAiProfile()
        {
            // Arrange
            var stepToolDto = AutomationFixture.FindValidStepToolDto();
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var normalProfile = new Domain.Models.Profile("Normal Profile", 2, DateTime.UtcNow);

            var currentStep = new Domain.Models.Step(1, DateTime.UtcNow, 1, "Current Step", 1, normalProfile.Id, 1);
            currentStep.Profile = normalProfile;

            var card = new Domain.Models.Card(1, DateTime.UtcNow, currentStep.Id, 1, "Test Card", 1, null);
            card.Step = currentStep;
            
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            var stepRepositoryMock = _mocker.GetMock<IStepRepository>();
            stepToolRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).ReturnsAsync(stepToolDto);
            stepToolRepositoryMock.Setup(r => r.FindDependentAsync(It.IsAny<int>())).ReturnsAsync((StepTool?)null);
           
            cardRepositoryMock.Setup(r => r.FindByIdWithStepAndProfile(automationDto.CardId)).ReturnsAsync(card);

            // Act
            await _service.ContinueExecution(automationDto);

            // Assert
            stepToolRepositoryMock.Verify(r => r.FindDependentAsync(It.IsAny<int>()), Times.Once);
            
            cardRepositoryMock.Verify(r => r.FindByIdWithStepAndProfile(automationDto.CardId), Times.Once);
            stepRepositoryMock.Verify(r => r.FindByOrderAndWorkflowId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            cardRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Models.Card>()), Times.Never);
        }
    }
}
