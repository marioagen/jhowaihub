using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository;

namespace WoopiAiHub.Application.Services
{
    public class AutomationServices : IAutomationServices
    {
        private readonly IStepToolRepository _stepToolRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IToolFactoryHandler _toolFactoryHandler;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IStepToolParameterRepository _stepToolParameterRepository;
        private readonly IMessagePublisher<object> _messagePublisher;
        private readonly ILogger<AutomationServices> _logger;
        private string _tenant = string.Empty;
        private string _referenceFile = string.Empty;
        private string _email = string.Empty;

        public AutomationServices(IStepToolExecutionRepository stepToolExecutionRepository,
                                  IStepToolRepository stepToolRepository,
                                  IToolFactoryHandler toolFactoryHandler,
                                  IStepToolOutputRepository stepToolOutputRepository,
                                  IStepToolParameterRepository stepToolParameterRepository,
                                  IMessagePublisher<object> messagePublisher,
                                  ILogger<AutomationServices> logger)
        {
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolRepository = stepToolRepository;
            _toolFactoryHandler = toolFactoryHandler;
            _stepToolOutputRepository = stepToolOutputRepository;
            _stepToolParameterRepository = stepToolParameterRepository;
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        /// <summary>
        /// Prepare execution creating step tool executions when steps have tools
        /// </summary>
        /// <param name="workflows"></param>
        /// <returns></returns>
        public async Task<bool> PrepareExecutionAsync(ICollection<Workflow> workflows)
        {
            var executions = new List<StepToolExecution>();
            var stepIds = workflows.SelectMany(wf => wf.Steps.Select(s => s.Id)).ToList();
            var allStepTools = await _stepToolRepository.FindStepToolsByStepIdsAsync(stepIds);

            foreach (var workflow in workflows)
            {
                    var stepTools = allStepTools.OrderBy(st => st.Order);

                    foreach (var stepTool in stepTools)
                    {
                        foreach (var card in workflow.Steps.Where(u => u.Order.Equals(1)).SelectMany(u => u.Cards))
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

            if (executions.Any())
            {
                await _stepToolExecutionRepository.CreateRangeAsync(executions);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Start executions on firsts steps
        /// </summary>
        /// <param name="workflows"></param>
        /// <returns></returns>
        public async Task StartExecutionByWorkflowsAsync(string tenant, string referenceFile, string email, ICollection<Workflow> workflows)
        {
            _tenant = tenant;
            _referenceFile = referenceFile;
            var firstSteps = workflows.SelectMany(wf => wf.Steps.Where(s => s.Order == 1)).ToList();
            await Parallel.ForEachAsync(firstSteps, async (step, ct) =>
            {
                try
                {
                    await StartExecutionByStepAsync(step, tenant, referenceFile, email);
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
        public async Task StartExecutionByStepAsync(Step step, string tenant, string referenceFile, string email)
        {
            var tasks = step.StepTools
                            .Where(st => !st.DependsOnStepToolId.HasValue)
                            .OrderBy(st => st.Order)
                            .SelectMany(st => step.Cards.Select(card => RunStepToolExecutionAsync(st, card.Id, tenant, referenceFile, email)));

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
        public async Task StartExecutionByCardAsync(int stepId, int cardId, string tenant, string referenceFile, string email)
        {
            var stepTool = await _stepToolRepository.FindByStepIdAndOrderAsync(stepId, 1);
            if (stepTool != null)
                await RunStepToolExecutionAsync(stepTool, cardId, tenant, referenceFile, email);
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
        private async Task RunStepToolExecutionAsync(StepTool stepTool, int cardId, string tenant, string referenceFile, string email)
        {
            _tenant = tenant;
            _referenceFile = referenceFile;
            _email = email;
            var execution = await _stepToolExecutionRepository
                                  .FindByStepToolIdAndCardIdAsync(stepTool.Id, cardId);

            if (execution is null)
                return;

            execution.UpdateStatusExecution(StatusExecution.Running);
            await _stepToolExecutionRepository.UpdateAsync(execution);

            var input = _stepToolParameterRepository.FindByStepToolId(stepTool.Id);

            string output = string.Empty;
            if (stepTool.DependsOnStepTool != null)
                output = await _stepToolOutputRepository.FindByStepToolId(stepTool.DependsOnStepTool.Id);

            var handler = _toolFactoryHandler.GetHandler(stepTool.Tool!.ToolType!);
            var payload = await handler.BuildPayload(_tenant, _referenceFile, input, stepTool.Id, cardId, _email, output);

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
        public async Task ContinueExecution(int stepToolId,
                                            int cardId,
                                            string tenant,
                                            string email,
                                            string referenceFile)
        {
            _tenant = tenant;
            _referenceFile = referenceFile;
            _email = email;

            var stepTool = await _stepToolRepository.FindById(stepToolId);
            var dependentStepTool = await _stepToolRepository.FindDependentAsync(stepToolId);

            if (dependentStepTool == null)
                return;

            if (stepTool.Step.Order.Equals(dependentStepTool.Step.Order) is false)
                return;

            var execution = await _stepToolExecutionRepository
                .FindByStepToolIdAndCardIdAsync(dependentStepTool.Id, cardId);
            if (execution == null)
                return;

            execution.UpdateStatusExecution(StatusExecution.Running);
            await _stepToolExecutionRepository.UpdateAsync(execution);
            var input = _stepToolParameterRepository.FindByStepToolId(stepTool.Id);

            string output =await _stepToolOutputRepository.FindByStepToolId(dependentStepTool.DependsOnStepTool.Id);

            var handler = _toolFactoryHandler.GetHandler(dependentStepTool.Tool.ToolType);
            var payload = await handler.BuildPayload(
                _tenant, _referenceFile, input, dependentStepTool.Id, cardId, email, output);

            await _messagePublisher.PublishAsync(payload.Queue, payload.Message);
        }

    }
}

