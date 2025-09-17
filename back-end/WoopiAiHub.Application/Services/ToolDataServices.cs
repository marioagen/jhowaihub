using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class ToolDataServices : IToolDataServices
    {
        private readonly IToolDataRepository _toolDataRepository;

        public ToolDataServices(IToolDataRepository toolDataRepository)
        {
            _toolDataRepository = toolDataRepository;
        }

        /// <summary>
        /// Asynchronously retrieves all tool data records.
        /// </summary>
        /// <remarks>This method retrieves all tool data records from the underlying data source. The
        /// result may be empty  if no records are found.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  IEnumerable{T} of
        /// ToolDataDto objects representing all tool data records.</returns>
        public async Task<IEnumerable<ToolDataDto>> FindAllAsync()
        {
            return await _toolDataRepository.FindAllAsync();
        }
    }
}
