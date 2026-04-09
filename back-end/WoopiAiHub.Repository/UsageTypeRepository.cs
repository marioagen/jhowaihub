using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class UsageTypeRepository : IUsageTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public UsageTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Find usage type by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<UsageType?> FindByNameAsync(string name)
        {
            return await _context.UsageTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);
        }

        /// <summary>
        /// Find usage type by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UsageType?> FindByIdAsync(int id)
        {
            return await _context.UsageTypes.FindAsync(id);
        }

        /// <summary>
        /// Add a new usage type
        /// </summary>
        /// <param name="usageType"></param>
        /// <returns></returns>
        public async Task<bool> AddAsync(UsageType usageType)
        {
            await _context.UsageTypes.AddAsync(usageType);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Update an existing usage type
        /// </summary>
        /// <param name="usageType"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(UsageType usageType)
        {
            _context.UsageTypes.Update(usageType);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Delete a usage type by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var usageType = await FindByIdAsync(id);
            if (usageType == null) return false;

            _context.UsageTypes.Remove(usageType);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
