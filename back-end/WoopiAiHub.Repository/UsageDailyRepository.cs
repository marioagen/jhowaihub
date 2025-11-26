using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class UsageDailyRepository : IUsageDailyRepository
    {
        private readonly ApplicationDbContext _context;

        public UsageDailyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Find usage daily by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UsageDaily?> FindByIdAsync(int id)
        {
            return await _context.UsageDailies.FindAsync(id);
        }

        /// <summary>
        /// Add a new usage daily record
        /// </summary>
        /// <param name="usageDaily"></param>
        /// <returns></returns>
        public async Task<bool> AddAsync(UsageDaily usageDaily)
        {
            await _context.UsageDailies.AddAsync(usageDaily);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Update an existing usage daily record
        /// </summary>
        /// <param name="usageDaily"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(UsageDaily usageDaily)
        {
            _context.UsageDailies.Update(usageDaily);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Delete a usage daily record by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var usageDaily = await FindByIdAsync(id);
            if (usageDaily == null) return false;

            _context.UsageDailies.Remove(usageDaily);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
