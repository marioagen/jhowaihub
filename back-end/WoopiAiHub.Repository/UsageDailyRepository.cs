using Microsoft.EntityFrameworkCore;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<UsageDaily>> FindUnprocessedAsync()
        {
            return await _context.UsageDailies
                .Where(ud => !ud.Processed)
                .OrderBy(ud => ud.Created)
                .Include(ud => ud.User)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<UsageDaily>> GetOldRecordsAsync(DateTime cutoffDate, int batchSize, CancellationToken ct = default)
        {
            return await _context.UsageDailies
                .Where(ud => ud.Created <= cutoffDate)
                .OrderBy(ud => ud.Created)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task MarkAsProcessedAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            await _context.UsageDailies
                .Where(ud => ids.Contains(ud.Id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(ud => ud.Processed, true), ct);
        }

        public async Task BulkDeleteAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            await _context.UsageDailies
                .Where(ud => ids.Contains(ud.Id))
                .ExecuteDeleteAsync(ct);
        }
    }
}
