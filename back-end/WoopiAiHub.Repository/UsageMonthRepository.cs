using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class UsageMonthRepository : IUsageMonthRepository
    {
        private readonly ApplicationDbContext _context;

        public UsageMonthRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<UsageMonth?> FindByKeyAsync(int usageTypeId, int modelEmbeddingId, Guid userId, DateTime month)
        {
            // For daily records, we need to match the exact day
            var dayStart = month.Date;
            var dayEnd = dayStart.AddDays(1);

            return await _context.usageMonths
                .FirstOrDefaultAsync(um =>
                    um.UsageTypeId == usageTypeId &&
                    um.ModelEmbeddingId == modelEmbeddingId &&
                    um.UserId == userId &&
                    um.Created >= dayStart &&
                    um.Created < dayEnd);
        }


        public async Task UpsertAsync(UsageMonth entity)
        {
            var existing = await FindByKeyAsync(
                entity.UsageTypeId,
                entity.ModelEmbeddingId,
                entity.UserId,
                entity.Created);

            if (existing != null)
            {
                // Update existing record
                await _context.usageMonths
                    .Where(um => um.Id == existing.Id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(um => um.Total, existing.Total + entity.Total));
            }
            else
            {
                // Insert new record
                await _context.usageMonths.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> FindTotalUsageAsync(DateTime periodStart, DateTime periodEnd)
        {
            var total = await _context.usageMonths
                .Where(um => um.Created >= periodStart && um.Created < periodEnd)
                .SumAsync(um => um.Total);

            return total;
        }
    }
}
