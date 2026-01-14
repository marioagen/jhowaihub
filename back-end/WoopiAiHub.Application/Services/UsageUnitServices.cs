using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;

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
        /// <param name="dateFilterDto"></param>
        /// <returns></returns>
        public async Task<IEnumerable<UsageUnitDto>> FindAllAsync(DateFilterDto? dateFilterDto = null)
        {
            return await _usageUnitRepository.FindAllAsync(dateFilterDto);
        }
    }
}
