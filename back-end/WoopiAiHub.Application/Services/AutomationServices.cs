using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Migrations;

namespace WoopiAiHub.Application.Services
{
    public class AutomationServices : IAutomationServices
    {
        private readonly IStepToolRepository _stepToolRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IToolFactoryHandlerServices _toolFactoryHandlerServices;
        private readonly IToolOutputServices _toolOutputServices;
        private readonly IMessagePublisher<string> _messagePublisher;
        private readonly ILogger<AutomationServices> _logger;

        public AutomationServices(IStepToolExecutionRepository stepToolExecutionRepository,
                                  IStepToolRepository stepToolRepository,
                                  IToolFactoryHandlerServices toolFactoryHandlerServices,
                                  IToolOutputServices toolOutputServices,
                                  IMessagePublisher<string> messagePublisher,
                                  ILogger<AutomationServices> logger)
        {
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolRepository = stepToolRepository;
            _toolFactoryHandlerServices = toolFactoryHandlerServices;
            _toolOutputServices = toolOutputServices;
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        /// <summary>
        /// Prepare execution creating step tool executions when steps have tools
        /// </summary>
        /// <param name="workflows"></param>
        /// <returns></returns>
        public void PrepareExecutionAsync(ICollection<Workflow> workflows)
        {
            var executions = new List<StepToolExecution>();
            var stepIds = workflows.SelectMany(wf => wf.Steps.Select(s => s.Id)).ToList();
            var allStepTools = _stepToolRepository.FindStepToolsByStepIdsAsync(stepIds).Result;

            foreach (var workflow in workflows)
            {
                foreach (var step in workflow.Steps.OrderBy(s => s.Order))
                {
                    var stepTools = allStepTools.Where(st => st.StepId == step.Id)
                                                .OrderBy(st => st.Order);

                    foreach (var stepTool in stepTools)
                    {
                        foreach (var card in step.Cards)
                        {
                            executions.Add(new StepToolExecution(
                                0,
                                DateTime.UtcNow,
                                stepTool.Id,
                                StatusExecution.Pending,
                                card.Id));
                        }
                    }
                }
            }

            if (executions.Any())
                _stepToolExecutionRepository.CreateRangeAsync(executions);
        }

        /// <summary>
        /// Start executions on firsts steps
        /// </summary>
        /// <param name="workflows"></param>
        /// <returns></returns>
        public async Task StartExecutionByWorkflowsAsync(ICollection<Workflow> workflows)
        {
            var firstSteps = workflows.SelectMany(wf => wf.Steps.Where(s => s.Order == 1)).ToList();

            await Parallel.ForEachAsync(firstSteps, async (step, ct) =>
            {
                try
                {
                    await StartExecutionByStepAsync(step);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao iniciar execuções do Step {StepId}", step.Id);
                }
            });
        }

        /// <summary>
        /// Executes the specified step by running its tools in parallel for each associated card.
        /// </summary>
        /// <remarks>Tools without dependencies are executed first, in the order specified by their
        /// <c>Order</c> property.  Each tool is executed for every card associated with the step. The execution is
        /// performed asynchronously  and in parallel for all eligible tools and cards.</remarks>
        /// <param name="step">The step containing the tools and cards to execute. Cannot be <see langword="null"/>.</param>
        /// <returns></returns>
        public async Task StartExecutionByStepAsync(Step step)
        {
            var tasks = step.StepTools
                            .Where(st => !st.DependsOnStepToolId.HasValue)
                            .OrderBy(st => st.Order)
                            .SelectMany(st => step.Cards.Select(card => RunStepToolExecutionAsync(st, card.Id)));

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Initiates the execution of a step tool associated with the specified step and card.
        /// </summary>
        /// <remarks>This method retrieves the first step tool for the specified step and, if found,
        /// executes it using the provided card identifier. If no step tool is found, the method completes without
        /// performing any action.</remarks>
        /// <param name="stepId">The identifier of the step to execute.</param>
        /// <param name="cardId">The identifier of the card associated with the execution.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task StartExecutionByCardAsync(int stepId, int cardId)
        {
            var stepTool = await _stepToolRepository.FindByStepIdAndOrderAsync(stepId, 1);
            if (stepTool != null)
                await RunStepToolExecutionAsync(stepTool, cardId);
        }

        /// <summary>
        /// Executes the specified step tool for the given card asynchronously.
        /// </summary>
        /// <remarks>This method retrieves the execution record for the specified step tool and card. If
        /// no execution record is found, the method exits without performing any action. If an execution record is
        /// found, its status is updated to <see cref="StatusExecution.Running"/> before proceeding with the execution.
        /// The method builds the necessary payload using the tool's input and publishes it to the appropriate message
        /// queue.</remarks>
        /// <param name="stepTool">The step tool to be executed. This parameter cannot be null.</param>
        /// <param name="cardId">The identifier of the card associated with the step tool execution.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task RunStepToolExecutionAsync(StepTool stepTool, int cardId)
        {
            var execution = await _stepToolExecutionRepository
                                  .FindByStepToolIdAndCardIdAsync(stepTool.Id, cardId);

            if (execution is null)
                return;

            execution.UpdateStatusExecution(StatusExecution.Running);
            await _stepToolExecutionRepository.UpdateAsync(execution);

            var input = _toolOutputServices.GetInput(stepTool.Id);
            var handler = _toolFactoryHandlerServices.GetHandler(stepTool.Tool!.ToolType!);
            var payload = handler.BuildPayload(input, stepTool.Id, cardId);

            await _messagePublisher.PublishAsync(payload.Queue, payload.Message);
        }

        /// <summary>
        /// Continues the execution of a dependent step tool for the specified card.
        /// </summary>
        /// <remarks>This method checks for a dependent step tool associated with the specified <paramref
        /// name="stepToolId"/>. If a dependent step tool exists and its execution is found, the execution status is
        /// updated to "Running", and a payload is built and published to the appropriate message queue.</remarks>
        /// <param name="stepToolId">The identifier of the step tool whose dependent tool's execution should be continued.</param>
        /// <param name="cardId">The identifier of the card associated with the execution.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ContinueExecution(int stepToolId, int cardId)
        {
            var dependentStepTool = await _stepToolRepository.FindDependentAsync(stepToolId);

            if (dependentStepTool != null)
            {
                var execution = await _stepToolExecutionRepository
                    .FindByStepToolIdAndCardIdAsync(dependentStepTool.Id, cardId);
                if (execution == null) return;

                execution.UpdateStatusExecution(StatusExecution.Running);
                await _stepToolExecutionRepository.UpdateAsync(execution);

                var input = _toolOutputServices.GetInput(dependentStepTool.Id);
                var handler = _toolFactoryHandlerServices.GetHandler(dependentStepTool.Tool.ToolType);
                var payload = handler.BuildPayload(input, dependentStepTool.Id, cardId);

                await _messagePublisher.PublishAsync(payload.Queue, payload.Message);
            }
        }

        /// <summary>
        /// Start step fisrt tool execution 
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public ICollection<StepTool> FindStepToolsByStepId(int stepId)
        {
            return _stepToolRepository.FindStepToolsByStepId(stepId);
        }

        public ICollection<StepToolDto> FindAll()
        {
            return _stepToolRepository.FindAll().ToList();
        }

        /// <summary>
        /// Find a question by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<StepToolDto> FindById(int id)
        {
            return await _stepToolRepository.FindById(id);
        }

        /// <summary>
        /// Delete questions by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            var idsSteps = _stepToolRepository.FindByIds(ids);
            {
                if (!idsSteps.Any())
                {
                    throw new Exception("No StepTools found with the provided IDs.");
                }
            }
            var result = _stepToolRepository.DeleteByIds(ids);
            return result;
        }

        /// <summary>
        /// Update question by dto
        /// </summary>
        /// <param name="updatequestionDto"></param>
        /// <returns></returns>
        public async Task<bool> Update(int id,
                                       string input)
        {
            var stepToolResult = await _stepToolRepository.FindById(id);
            if (stepToolResult == null)
            {
                throw new Exception("StepTool not found");
            }
            stepToolResult.Parameters.First().Value = input;
            var result = await _stepToolRepository.Update(stepToolResult);

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepToolCreateDto"></param>
        /// <returns></returns>
        public async Task<bool> CreateAsync(StepToolCreateDto stepToolCreateDto)
        {
            var stepTool = new StepTool(
                0,
                DateTime.UtcNow,
                stepToolCreateDto.StepId,
                stepToolCreateDto.ToolId,
                stepToolCreateDto.Order,
                stepToolCreateDto.PositionX,
                stepToolCreateDto.PositionY
             );

            return await _stepToolRepository.Create(stepTool);
        }
    }
}
        
