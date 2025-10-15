using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Hubs;
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
        private readonly ICardRepository _cardRepository;
        private readonly IStepRepository _stepRepository;
        private readonly IHubNotifier _hubNotifier;

        public AutomationServices(IStepToolExecutionRepository stepToolExecutionRepository,
                                  IStepToolRepository stepToolRepository,
                                  IToolFactoryHandler toolFactoryHandler,
                                  IStepToolOutputRepository stepToolOutputRepository,
                                  IStepToolParameterRepository stepToolParameterRepository,
                                  IMessagePublisher<object> messagePublisher,
                                  ILogger<AutomationServices> logger,
                                  ICardRepository cardRepository,
                                  IStepRepository stepRepository,
                                  IHubNotifier hubNotifier)
        {
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolRepository = stepToolRepository;
            _toolFactoryHandler = toolFactoryHandler;
            _stepToolOutputRepository = stepToolOutputRepository;
            _stepToolParameterRepository = stepToolParameterRepository;
            _messagePublisher = messagePublisher;
            _logger = logger;
            _cardRepository = cardRepository;
            _stepRepository = stepRepository;
            _hubNotifier = hubNotifier;
        }

        /// <summary>
        /// Prepare execution creating step tool executions when steps have tools
        /// </summary>
        /// <param name="workflows"></param>
        /// <returns></returns>
        public async Task<bool> PrepareExecutionAsync(ICollection<Workflow> workflows)
        {
            var stepIds = workflows.SelectMany(wf => wf.Steps.Select(s => s.Id)).ToList();
            var allStepTools = await _stepToolRepository.FindStepToolsByStepIdsAsync(stepIds);

            var activeCardIds = await FindActiveCardIdsAsync(workflows);
            if (!activeCardIds.Any())
                return false;

            var existing = await _stepToolExecutionRepository.FindExistingExecutionsAsync(activeCardIds);

            var newExecutions = BuildNewExecutions(workflows, allStepTools, activeCardIds, existing);
            if (!newExecutions.Any())
                return false;

            await _stepToolExecutionRepository.CreateRangeAsync(newExecutions);
            return true;
        }

        /// <summary>
        /// Asynchronously retrieves the IDs of active cards that are in the first step of the specified workflows.
        /// </summary>
        /// <remarks>Only cards associated with the first step of each workflow are considered. The method
        /// filters  and evaluates these cards to determine their active status.</remarks>
        /// <param name="workflows">A collection of workflows to evaluate. Each workflow may contain steps and associated cards.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of IDs  for the
        /// active cards found in the first step of the provided workflows.</returns>
        private async Task<ICollection<int>> FindActiveCardIdsAsync(ICollection<Workflow> workflows)
        {
            var candidateCardIds = workflows.SelectMany(w => w.Steps.Where(s => s.Order == 1).SelectMany(s => s.Cards))
                                            .Select(c => c.Id)
                                            .ToList();

            return await _cardRepository.FindActiveCardIdsInFirstStepAsync(candidateCardIds);
        }

        /// <summary>
        /// Builds a list of new <see cref="StepToolExecution"/> objects based on the provided workflows, step tools,
        /// active card IDs, and existing executions.
        /// </summary>
        /// <remarks>This method evaluates each workflow's first step and its associated cards to
        /// determine which cards are active and do not already have an execution for a given step tool. New executions
        /// are created for such cards and step tools.</remarks>
        /// <param name="workflows">A collection of workflows containing steps and cards to evaluate.</param>
        /// <param name="allStepTools">An enumerable of all available step tools, which will be ordered by their execution order.</param>
        /// <param name="activeCardIds">A collection of card IDs that are considered active and eligible for execution.</param>
        /// <param name="existing">A collection of tuples representing existing executions, where each tuple contains a step tool ID and a card
        /// ID.</param>
        /// <returns>A list of <see cref="StepToolExecution"/> objects representing new executions that do not already exist in
        /// the <paramref name="existing"/> collection.</returns>
        private List<StepToolExecution> BuildNewExecutions(ICollection<Workflow> workflows,
                                                           IEnumerable<StepTool> allStepTools,
                                                           ICollection<int> activeCardIds,
                                                           ICollection<(int StepToolId, int CardId)> existing)
        {
            var executions = new List<StepToolExecution>();

            foreach (var workflow in workflows)
            {
                var stepTools = allStepTools.OrderBy(st => st.Order);

                foreach (var stepTool in stepTools)
                {
                    foreach (var card in workflow.Steps
                                                 .Where(s => s.Order == 1)
                                                 .SelectMany(s => s.Cards)
                                                 .Where(c => activeCardIds.Contains(c.Id)))
                    {
                        if (!existing.Any(ex => ex.CardId == card.Id && ex.StepToolId == stepTool.Id))
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

            return executions;
        }

        /// <summary>
        /// Start executions on firsts steps
        /// </summary>
        /// <param name="workflows"></param>
        /// <returns></returns>
        public async Task StartExecutionByWorkflowsAsync(AutomationServicesDto automationServicesDto, ICollection<Workflow> workflows)
        {
            var firstSteps = workflows.SelectMany(wf => wf.Steps.Where(s => s.Order == 1)).ToList();
            await Parallel.ForEachAsync(firstSteps, async (step, ct) =>
            {
                try
                {
                    await StartExecutionByStepAsync(step, automationServicesDto);
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
        public async Task StartExecutionByStepAsync(Step step, AutomationServicesDto automationServicesDto)
        {
            var tasks = step.StepTools
                            .Where(st => !st.DependsOnStepToolId.HasValue)
                            .OrderBy(st => st.Order)
                            .SelectMany(st => step.Cards.Select(card => RunStepToolExecutionAsync(st, automationServicesDto, card.Id)));

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
        public async Task StartExecutionByCardAsync(AutomationServicesDto automationServicesDto)
        {
            var stepTool = await _stepToolRepository.FindByStepIdAndOrderAsync(automationServicesDto.StepId.GetValueOrDefault(), 1);
            if (stepTool != null)
                await RunStepToolExecutionAsync(stepTool, automationServicesDto, 0);
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
        private async Task RunStepToolExecutionAsync(StepTool stepTool, AutomationServicesDto automationServicesDto, int cardId)
        {
            var resolvedCardId = cardId > 0 ? cardId : automationServicesDto.CardId;
            var execution = await _stepToolExecutionRepository
                                  .FindByStepToolIdAndCardIdAsync(stepTool.Id, resolvedCardId);

            if (execution is null)
                return;

            execution.UpdateStatusExecution(StatusExecution.Running);
            await _stepToolExecutionRepository.UpdateAsync(execution);

            var input = _stepToolParameterRepository.FindByStepToolId(stepTool.Id);

            string output = string.Empty;
            if (stepTool.DependsOnStepTool != null)
                output = await _stepToolOutputRepository.FindByStepToolId(stepTool.DependsOnStepTool.Id, resolvedCardId);

            var handler = _toolFactoryHandler.GetHandler(stepTool.Tool!.ToolType!);
            var enrichedDto = EnrichDtoWithExecutionData(automationServicesDto, stepTool.Id, resolvedCardId);
            var payload = await handler.BuildPayload(enrichedDto, input, output);

            await _messagePublisher.PublishAsync(payload.Queue, payload.Message);
        }

        /// <summary>
        /// Updates the specified <see cref="AutomationServicesDto"/> instance with execution data  based on the
        /// provided step tool ID and card ID.
        /// </summary>
        /// <param name="dto">The <see cref="AutomationServicesDto"/> instance to be enriched.  If the <c>StepToolId</c> or <c>CardId</c>
        /// properties are greater than zero, their values remain unchanged. Otherwise, they are updated with the
        /// provided <paramref name="stepToolId"/> or <paramref name="cardId"/>.</param>
        /// <param name="stepToolId">The step tool ID to use if the <c>StepToolId</c> property of <paramref name="dto"/> is not set (i.e., less
        /// than or equal to zero).</param>
        /// <param name="cardId">The card ID to use if the <c>CardId</c> property of <paramref name="dto"/> is not set (i.e., less than or
        /// equal to zero).</param>
        /// <returns>A new <see cref="AutomationServicesDto"/> instance with updated <c>StepToolId</c> and <c>CardId</c> values.</returns>
        private AutomationServicesDto EnrichDtoWithExecutionData(AutomationServicesDto dto, int stepToolId, int cardId)
        {
            return dto with
            {
                StepToolId = dto.StepToolId > 0 ? dto.StepToolId : stepToolId,
                CardId = dto.CardId > 0 ? dto.CardId : cardId
            };
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
        public async Task ContinueExecution(AutomationServicesDto automationServicesDto)
        {
            var stepTool = await _stepToolRepository.FindById(automationServicesDto.StepToolId);
            var dependentStepTool = await _stepToolRepository.FindDependentAsync(automationServicesDto.StepToolId);

            if (dependentStepTool == null || stepTool.Step.Order.Equals(dependentStepTool.Step.Order) is false)
            {
                // Se não há StepTool dependente ou os steps são diferentes, significa que é a última StepTool
                // Verifica se precisa avançar o step para perfis de IA
                await CheckAndAdvanceAiProfileStepAsync(automationServicesDto);
                return;
            }

            var execution = await _stepToolExecutionRepository
                .FindByStepToolIdAndCardIdAsync(dependentStepTool.Id, automationServicesDto.CardId);
            if (execution == null)
                return;

            execution.UpdateStatusExecution(StatusExecution.Running);
            await _stepToolExecutionRepository.UpdateAsync(execution);
            var input = _stepToolParameterRepository.FindByStepToolId(stepTool.Id);

            string output = await _stepToolOutputRepository.FindByStepToolId(dependentStepTool.DependsOnStepTool.Id, execution.CardId);

            var handler = _toolFactoryHandler.GetHandler(dependentStepTool.Tool.ToolType);
            var payload = await handler.BuildPayload(automationServicesDto, input, output);

            await _messagePublisher.PublishAsync(payload.Queue, payload.Message);
        }



        /// <summary>
        /// Verifica se o card está em um step cujo perfil responsável é o perfil de IA
        /// e avança automaticamente o card para o próximo step do workflow.
        /// </summary>
        /// <param name="automationServicesDto">DTO contendo informações do card e step</param>
        /// <returns>Task que representa a operação assíncrona</returns>
        private async Task CheckAndAdvanceAiProfileStepAsync(AutomationServicesDto automationServicesDto)
        {
            try
            {
                // Busca o card para obter informações do step atual
                var card = await _cardRepository.FindById(automationServicesDto.CardId);
                if (card?.Step?.Profile == null)
                    return;

                // Verifica se o perfil responsável pelo step atual é o perfil de IA
                if (card.Step.Profile.Name != "IA")
                    return;

                _logger.LogInformation("Card {CardId} está no perfil IA. Avançando automaticamente para o próximo step.", automationServicesDto.CardId);

                // Busca o próximo step no workflow
                var nextStepOrder = card.Step.Order + 1;
                var nextStep = await _stepRepository.FindByOrderAndWorkflowId(nextStepOrder, card.Step.WorkflowId);
                
                if (nextStep == null)
                {
                    _logger.LogInformation("Não há próximo step para o card {CardId}. Workflow finalizado.", automationServicesDto.CardId);
                    return;
                }

                // Atualiza o card para o próximo step
                card.UpdateStepAndSatus(nextStep.Id, nextStep.StatusId);
                var updated = _cardRepository.Update(card);

                if (updated)
                {
                    _logger.LogInformation("Card {CardId} avançado automaticamente do step {CurrentStep} para o step {NextStep}", 
                        automationServicesDto.CardId, card.Step.Order, nextStep.Order);

                    // Inicia as execuções do próximo step
                    var nextStepDto = automationServicesDto with
                    {
                        StepId = nextStep.Id
                    };
                    await StartExecutionByCardAsync(nextStepDto);

                    // Notifica o front-end sobre a mudança do step via SignalR
                    await _hubNotifier.CardProgessAsync(automationServicesDto.Email, automationServicesDto.CardId, 0.0, nextStep.Id);
                    
                    _logger.LogInformation("Notificação enviada para o usuário {Email} sobre avanço do card {CardId} para o step {StepId}", 
                        automationServicesDto.Email, automationServicesDto.CardId, nextStep.Id);
                }
                else
                {
                    _logger.LogError("Falha ao atualizar o card {CardId} para o próximo step", automationServicesDto.CardId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar avançar automaticamente o step do card {CardId}", automationServicesDto.CardId);
            }
        }
    }
}

