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

        /// <summary>
        /// Retrieves a list of unprocessed daily usage records.
        /// </summary>
        /// <remarks>The returned list is ordered by the creation date of the records in ascending order. 
        /// Each record includes associated user information and is retrieved without tracking changes  in the database
        /// context.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of  <see
        /// cref="UsageDaily"/> objects that have not been processed. If no unprocessed records  are found, the list
        /// will be empty.</returns>
        public async Task<List<UsageDaily>> FindUnprocessedAsync()
        {
            return await _context.UsageDailies
                .Where(ud => !ud.Processed)
                .OrderBy(ud => ud.Created)
                .Include(ud => ud.User)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a list of usage records created on or before the specified cutoff date.
        /// </summary>
        /// <remarks>The returned records are retrieved without tracking, meaning they will not be tracked
        /// by the database context.</remarks>
        /// <param name="cutoffDate">The date used to filter records. Only records with a creation date less than or equal to this value will be
        /// included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="UsageDaily"/> objects ordered by their creation date in ascending order. The list will be empty if no
        /// matching records are found.</returns>
        public async Task<List<UsageDaily>> FindOldRecordsAsync(DateTime cutoffDate)
        {
            return await _context.UsageDailies
                .Where(ud => ud.Created <= cutoffDate)
                .OrderBy(ud => ud.Created)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Marks the specified usage records as processed.
        /// </summary>
        /// <remarks>This method updates the <c>Processed</c> property of the usage records with the
        /// specified identifiers to <see langword="true"/>. Ensure that the provided identifiers correspond to existing
        /// records in the database.</remarks>
        /// <param name="ids">A collection of identifiers representing the usage records to be marked as processed. Cannot be null or
        /// empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task MarkAsProcessedAsync(IEnumerable<int> ids)
        {
            await _context.UsageDailies
                .Where(ud => ids.Contains(ud.Id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(ud => ud.Processed, true));
        }

        /// <summary>
        /// Deletes multiple records from the database based on the specified collection of IDs.
        /// </summary>
        /// <remarks>This method performs a bulk delete operation, which is optimized for removing
        /// multiple records at once. Ensure that the provided <paramref name="ids"/> collection contains valid IDs that
        /// exist in the database.</remarks>
        /// <param name="ids">A collection of IDs representing the records to be deleted. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task BulkDeleteAsync(IEnumerable<int> ids)
        {
            await _context.UsageDailies
                .Where(ud => ids.Contains(ud.Id))
                .ExecuteDeleteAsync();
        }
    }
}
