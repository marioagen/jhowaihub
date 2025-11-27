using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class UsageLogRepository : IUsageLogRepository
    {
        private readonly ApplicationDbContext _context;

        public UsageLogRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task BulkInsertAsync(IEnumerable<UsageLog> logs, CancellationToken ct = default)
        {
            await _context.UsageLogs.AddRangeAsync(logs, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> ExistsAsync(int originalId, DateTime created, CancellationToken ct = default)
        {
            return await _context.UsageLogs
                .AnyAsync(ul => ul.Id == originalId && ul.Created == created, ct);
        }
    }
}
