using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository;

namespace WoopiAiHub.Application.Services
{
    public class AutomationServices : IAutomationServices
    {
        private readonly IStepToolRepository _stepToolRepository;

        public AutomationServices(IStepToolRepository stepToolRepository,
                                  IToolRepository toolRepository,
                                  IStepRepository stepRepository)
        {
            _stepToolRepository = stepToolRepository;
        }
        /// <summary>
        /// Find all questions
        /// </summary>
        /// <returns></returns>
        /// 
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
