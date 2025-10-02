using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;

namespace WoopiAiHub.Application.Services
{
    public class ToolOutputServices : IToolOutputServices
    {
        private readonly IStepToolOutputRepository _stepToolOutputRepository;

        public ToolOutputServices(IStepToolOutputRepository stepToolOutputRepository)
        {
            _stepToolOutputRepository = stepToolOutputRepository;
        }
    }
}
