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

        /// <summary>
        /// Inserts a collection of <see cref="UsageLog"/> entities into the database in a single batch operation.
        /// </summary>
        /// <remarks>This method performs a bulk insert operation, which is more efficient than inserting
        /// entities individually. Ensure that the provided collection is not null and contains valid entities to avoid
        /// runtime exceptions.</remarks>
        /// <param name="logs">The collection of <see cref="UsageLog"/> entities to insert. Cannot be null.</param>
        /// <param name="ct">An optional <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task BulkInsertAsync(IEnumerable<UsageLog> logs, CancellationToken ct = default)
        {
            await _context.UsageLogs.AddRangeAsync(logs, ct);
            await _context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Determines whether a usage log entry with the specified ID and creation date exists in the database.
        /// </summary>
        /// <remarks>This method performs an asynchronous database query to determine the existence of the
        /// specified usage log entry.</remarks>
        /// <param name="originalId">The unique identifier of the usage log entry to check.</param>
        /// <param name="created">The creation date and time of the usage log entry to check.</param>
        /// <param name="ct">An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns><see langword="true"/> if a usage log entry with the specified ID and creation date exists; otherwise, <see
        /// langword="false"/>.</returns>
        public async Task<bool> ExistsAsync(int originalId, DateTime created, CancellationToken ct = default)
        {
            return await _context.UsageLogs
                .AnyAsync(ul => ul.Id == originalId && ul.Created == created, ct);
        }
    }
}
