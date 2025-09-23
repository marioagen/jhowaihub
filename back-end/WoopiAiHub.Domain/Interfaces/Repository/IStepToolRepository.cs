using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolRepository
    {
        public bool DeleteByIds(ICollection<int> ids);
        public Task<StepToolDto?> FindById(int id);
        public IQueryable<StepToolDto> FindByIds(ICollection<int> ids);
        public Task<bool> Create(StepTool stepTool);
        public Task<bool> Update(StepToolDto stepToolDto);
        public IQueryable<StepToolDto> FindAll();
        Task<List<StepTool>> FindStepToolsByStepIdsAsync(IEnumerable<int> stepIds);
        Task<StepTool?> FindDependentAsync(int id);
        Task<StepTool?> FindByStepIdAndOrderAsync(int stepId, int order);
    }
}
