using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class UsageUnitServices : IUsageUnitServices
    {
        private readonly IUsageUnitRepository _usageUnitRepository;

        public UsageUnitServices(IUsageUnitRepository usageUnitRepository)
        {
            _usageUnitRepository = usageUnitRepository;
        }

        /// <summary>
        /// Find all usage units
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UsageUnit>> FindAllAsync()
        {
            return await _usageUnitRepository.FindAllAsync();
        }
    }
}
