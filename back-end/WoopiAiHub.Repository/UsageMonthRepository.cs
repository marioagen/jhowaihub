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

        public async Task<UsageMonth?> FindByKeyAsync(int usageTypeId, int modelEmbeddingId, Guid userId, DateTime month, CancellationToken ct = default)
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
                    um.Created < dayEnd, ct);
        }


        public async Task UpsertAsync(UsageMonth entity, CancellationToken ct = default)
        {
            var existing = await FindByKeyAsync(
                entity.UsageTypeId,
                entity.ModelEmbeddingId,
                entity.UserId,
                entity.Created,
                ct);

            if (existing != null)
            {
                // Update existing record
                await _context.usageMonths
                    .Where(um => um.Id == existing.Id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(um => um.Total, existing.Total + entity.Total), ct);
            }
            else
            {
                // Insert new record
                await _context.usageMonths.AddAsync(entity, ct);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<Dictionary<string, int>> GetTotalUsageByTenantsAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
        {
            // This method will aggregate usage by tenant
            // Since tenant info is in a separate database per tenant architecture,
            // we'll return the total for the current tenant context
            var total = await _context.usageMonths
                .Where(um => um.Created >= periodStart && um.Created < periodEnd)
                .SumAsync(um => um.Total, ct);

            // Return with a placeholder key - the calling service will know the tenant name from context
            return new Dictionary<string, int> { { "current_tenant", total } };
        }
    }
}
