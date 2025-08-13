using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class StatusServices : IStatusServices
    {
        private readonly IStatusRepository _statusRepository;

        public StatusServices(IStatusRepository statusRepository)
        {
            _statusRepository = statusRepository;
        }

        /// <summary>
        /// Retrieves all status.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<StatusDto>> FindAll()
        {
            return await _statusRepository.FindAll();
        }
    }
}
