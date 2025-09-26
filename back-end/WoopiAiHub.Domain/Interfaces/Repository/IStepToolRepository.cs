using WoopiAiHub.Domain.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolRepository
    {
        bool DeleteByIds(ICollection<int> ids);
        bool DeleteByStepId(int stepId);
        Task<StepToolDto?> FindById(int id);
        IQueryable<StepToolDto> FindByIds(ICollection<int> ids);
        Task<bool> Create(StepTool stepTool);
        Task<bool> CreateRangeAsync(List<StepTool> stepTools);
        Task<bool> Update(StepToolDto stepToolDto);
        IQueryable<StepToolDto> FindAll();
        Task<List<StepTool>> FindStepToolsByStepIdsAsync(IEnumerable<int> stepIds);
        Task<StepTool?> FindDependentAsync(int id);
        Task<StepTool?> FindByStepIdAndOrderAsync(int stepId, int order);
        ICollection<StepTool> FindStepToolsByStepId(int stepId);
    }
}
