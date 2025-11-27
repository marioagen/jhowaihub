using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class UsageUnitRepository : IUsageUnitRepository
    {
        private readonly ApplicationDbContext _context;

        public UsageUnitRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Find all usage units
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UsageUnit>> FindAllAsync()
        {
            return await _context.UsageUnits.ToListAsync();
        }
    }
}
