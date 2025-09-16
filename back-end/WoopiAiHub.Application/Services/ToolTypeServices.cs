using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class ToolTypeServices : IToolTypeServices
    {
        private readonly IToolTypeRepository _toolTypeRepository;

        public ToolTypeServices(IToolTypeRepository toolTypeRepository)
        {
            _toolTypeRepository = toolTypeRepository;
        }

        /// <summary>
        /// Asynchronously retrieves all tool types.
        /// </summary>
        /// <remarks>This method retrieves all tool types from the underlying data source. The result may
        /// be an empty  collection if no tool types are available.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  IEnumerable{T} of
        /// ToolTypeDto representing all tool types.</returns>
        public async Task<IEnumerable<ToolTypeDto>> FindAllAsync()
        {
            return await _toolTypeRepository.FindAllAsync();
        }
    }
}
