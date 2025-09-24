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
        
