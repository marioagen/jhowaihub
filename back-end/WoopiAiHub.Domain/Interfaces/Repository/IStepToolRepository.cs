using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolRepository
    {
        bool DeleteByIds(IEnumerable<int> ids);
        bool DeleteByStepId(int stepId);
        Task<StepToolDto?> FindById(int id);
        Task<StepTool?> FindByIdWithParameters(int id);
        IQueryable<StepToolDto> FindByIds(ICollection<int> ids);
        Task<bool> Create(StepTool stepTool);
        Task<bool> CreateRangeAsync(List<StepTool> stepTools);
        Task<bool> Update(StepTool stepTool);
        IQueryable<StepToolDto> FindAll();
        Task<List<StepTool>> FindStepToolsByStepIdsAsync(IEnumerable<int> stepIds);
        Task<StepTool?> FindDependentAsync(int id);
        Task<StepTool?> FindByStepIdAndOrderAsync(int stepId, int order);
        ICollection<StepTool> FindStepToolsByStepId(int stepId);
    }
}
