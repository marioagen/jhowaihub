using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class UsageTypeServices : IUsageTypeServices
    {
        private readonly IUsageTypeRepository _usageTypeRepository;

        public UsageTypeServices(IUsageTypeRepository usageTypeRepository)
        {
            _usageTypeRepository = usageTypeRepository;
        }

        /// <summary>
        /// Find usage type by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<UsageType?> FindByNameAsync(string name)
        {
            return await _usageTypeRepository.FindByNameAsync(name);
        }
    }
}
