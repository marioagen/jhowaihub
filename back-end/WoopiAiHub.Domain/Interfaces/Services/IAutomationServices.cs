using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IAutomationServices
    {
        public ICollection<StepToolDto> FindAll();
        public Task<StepToolDto> FindById(int id);
        public bool DeleteByIds(List<int> ids);
        public Task<bool> Update(int id,
                                 string input);
        public Task<bool> CreateAsync(StepToolCreateDto stepToolCreateDto);
    }
}
