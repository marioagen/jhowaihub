using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IValidateStep
    {
        void ValidateCreateStep(ICollection<StepCreateDto> stepsCreateDto);
        void ValidateUpdateStep(Workflow workflow, ICollection<StepUpdateDto> stepsUpdateDto);

        Task ValidateDeleteStep(ICollection<int> stepIds);
    }
}
