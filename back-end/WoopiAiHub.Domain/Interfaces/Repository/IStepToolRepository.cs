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
        public bool DeleteByIds(ICollection<int> ids);
        public Task<StepToolDto?> FindById(int id);
        public IQueryable<StepToolDto> FindByIds(ICollection<int> ids);
        public Task<bool> Create(StepTool stepTool);
        public Task<bool> Update(StepToolDto stepToolDto);
        public IQueryable<StepToolDto> FindAll();
        public ICollection<StepTool> FindStepToolsByStepId(int stepId);
    }
}
