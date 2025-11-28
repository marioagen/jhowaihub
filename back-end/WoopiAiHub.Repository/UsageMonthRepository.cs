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

        /// <summary>
        /// Asynchronously retrieves a <see cref="UsageMonth"/> record that matches the specified criteria.
        /// </summary>
        /// <remarks>This method filters records based on the exact day specified in the <paramref
        /// name="month"/> parameter. The time component of <paramref name="month"/> is ignored, and only records
        /// created within the specified day are considered.</remarks>
        /// <param name="usageTypeId">The identifier for the usage type to filter by.</param>
        /// <param name="modelEmbeddingId">The identifier for the model embedding to filter by.</param>
        /// <param name="userId">The unique identifier of the user associated with the record.</param>
        /// <param name="month">The date representing the month and day to filter by. Only records created on this specific day are
        /// considered.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the matching <see
        /// cref="UsageMonth"/> if found; otherwise, <see langword="null"/>.</returns>
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

        /// <summary>
        /// Inserts a new record or updates an existing record in the database based on the specified entity.
        /// </summary>
        /// <remarks>If a record with matching keys already exists, the method updates the <see
        /// cref="UsageMonth.Total"/> property by adding the value from the provided entity. If no matching record is
        /// found, the method inserts the provided entity as a new record.</remarks>
        /// <param name="entity">The <see cref="UsageMonth"/> entity to insert or update. The entity must include valid identifiers for <see
        /// cref="UsageMonth.UsageTypeId"/>, <see cref="UsageMonth.ModelEmbeddingId"/>, <see cref="UsageMonth.UserId"/>,
        /// and <see cref="UsageMonth.Created"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Asynchronously calculates the total usage for a specified time period.
        /// </summary>
        /// <remarks>This method queries the data source for usage records within the specified time range
        /// and calculates the sum of their total usage.</remarks>
        /// <param name="periodStart">The start date and time of the period to calculate usage for. This value is inclusive.</param>
        /// <param name="periodEnd">The end date and time of the period to calculate usage for. This value is exclusive.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total usage as an integer.</returns>
        public async Task<int> FindTotalUsageAsync(DateTime periodStart, DateTime periodEnd)
        {
            var total = await _context.usageMonths
                .Where(um => um.Created >= periodStart && um.Created < periodEnd)
                .SumAsync(um => um.Total);

            return total;
        }
    }
}
