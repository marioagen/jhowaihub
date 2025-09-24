using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IAutomationServices
    {
        Task PrepareExecution(ICollection<Workflow> workflows);
        Task StartExecutionByWorkflows(ICollection<Workflow> workflows);
        Task StartExecutionByStep(Step step);
        ICollection<StepTool> FindStepToolsByStepId(int stepId);
        ICollection<StepToolDto> FindAll();
        Task<StepToolDto> FindById(int id);
        bool DeleteByIds(List<int> ids);
        Task<bool> Update(int id,
                          string input);
        Task<bool> CreateAsync(StepToolCreateDto stepToolCreateDto);
    }
}
