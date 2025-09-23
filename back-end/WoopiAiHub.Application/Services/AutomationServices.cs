using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class AutomationServices : IAutomationServices
    {
        private readonly IStepToolRepository _stepToolRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IToolFactoryHandlerServices _toolFactoryHandlerServices;
        private readonly IToolOutputServices _toolOutputServices;
        private readonly IMessagePublisher<string> _messagePublisher;

        public AutomationServices(IStepToolExecutionRepository stepToolExecutionRepository
,                                 IStepToolRepository stepToolRepository,
                                  IToolFactoryHandlerServices toolFactoryHandlerServices,
                                  IToolOutputServices toolOutputServices,
                                  IMessagePublisher<string> messagePublisher)
        {
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolRepository = stepToolRepository;
            _toolFactoryHandlerServices = toolFactoryHandlerServices;
            _toolOutputServices = toolOutputServices;
            _messagePublisher = messagePublisher;
        }

        /// <summary>
        /// Prepare execution creating step tool executions when steps have tools
        /// </summary>
        /// <param name="workflows"></param>
        /// <returns></returns>
        public async Task PrepareExecution(ICollection<Workflow> workflows)
        {
            foreach (var workflow in workflows) {
                foreach (var step in workflow.Steps.OrderBy(o => o.Order))
                {
                    var stepTools = _stepToolRepository.FindStepToolsByStepId(step.Id);
                    foreach (var stepTool in stepTools.OrderBy(o => o.Order))
                    {
                        foreach (var card in step.Cards) {
                            var stepToolExecution = new StepToolExecution(0, DateTime.Now, stepTool.Id, StatusExecution.Pending, card.Id);
                            await _stepToolExecutionRepository.Create(stepToolExecution);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Start executions on firsts steps
        /// </summary>
        /// <param name="workflows"></param>
        /// <returns></returns>
        public async Task StartExecutionByWorkflows(ICollection<Workflow> workflows)
        {
            foreach (var workflow in workflows)
            {
                foreach (var step in workflow.Steps.Where(o => o.Order == 1))
                {                    
                    await StartExecutionByStep(step);
                }
            }
        }

        /// <summary>
        /// Start step fisrt tool execution 
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task StartExecutionByStep(Step step)
        {
            var stepTool = step.StepTools.FirstOrDefault(s => !s.DependsOnStepToolId.HasValue);

            if (stepTool is not null)
            {                
                var input = _toolOutputServices.GetInput(stepTool.Id);
                var handler = _toolFactoryHandlerServices.GetHandler(stepTool.Tool!.ToolType!);
                var payload = handler.BuildPayload(input);

                var stepToolExecution = await _stepToolExecutionRepository.FindByStepToolIdAsync(stepTool.Id);
                stepToolExecution.UpdateStatusExecution(StatusExecution.Running);
                await _stepToolExecutionRepository.UpdateAsync(stepToolExecution);

                await _messagePublisher.PublishAsync(payload.Queue, payload.Message);
            }
        }
    }
}
