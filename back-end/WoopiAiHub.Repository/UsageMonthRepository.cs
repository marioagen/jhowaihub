using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
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
            _context = context;
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
        public async Task<UsageMonth?> FindByKeyAsync(int usageTypeId,
            int? modelEmbeddingId,
            Guid userId,
            DateTime month)
        {
            var dayStart = month.Date;
            var dayEnd = dayStart.AddDays(1);

            return await _context.UsageMonths
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
                await _context.UsageMonths
                    .Where(um => um.Id == existing.Id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(um => um.Total, existing.Total + entity.Total));
            }
            else
            {
                await _context.UsageMonths.AddAsync(entity);
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
            // Query for records where ModelEmbeddingId is NULL (join only on UsageTypeId)
            var totalWithoutModelEmbedding = await _context.UsageMonths
                .Where(um => um.Created >= periodStart && um.Created < periodEnd && um.ModelEmbeddingId == null)
                .SelectMany(
                    um => _context.UsageUnits.Where(uu =>
                        uu.ModelEmbeddingId == null && uu.UsageTypeId == um.UsageTypeId),
                    (um, uu) => (decimal)um.Total * uu.Value
                )
                .SumAsync();

            // Query for records where ModelEmbeddingId is NOT NULL (join on both UsageTypeId and ModelEmbeddingId)
            var totalWithModelEmbedding = await _context.UsageMonths
                .Where(um => um.Created >= periodStart && um.Created < periodEnd && um.ModelEmbeddingId != null)
                .SelectMany(
                    um => _context.UsageUnits.Where(uu =>
                        uu.ModelEmbeddingId != null && uu.UsageTypeId == um.UsageTypeId &&
                        uu.ModelEmbeddingId == um.ModelEmbeddingId),
                    (um, uu) => (decimal)um.Total * uu.Value
                )
                .SumAsync();

            return (int)Math.Floor(totalWithoutModelEmbedding + totalWithModelEmbedding);
        }

        /// <summary>
        /// Finds usage data by usage type.
        /// </summary>
        /// <param name="usageTypeId"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByUsageType(string usageType,
            DateTime? start,
            DateTime? end)
        {
            var query = _context.UsageMonths
                .Where(x => x.UsageType!.Name.Equals(usageType));

            if (start.HasValue)
                query = query.Where(x => x.Created.Date >= start.Value.Date);

            if (end.HasValue)
                query = query.Where(x => x.Created.Date <= end.Value.Date);

            var result = await query.GroupBy(x => x.Created.Date)
                .Select(g => new DashboardUsageDto(g.Key.Date.ToString("dd/MM"), g.Sum(x => x.Total)))
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Finds usage data by model embedding ID.
        /// </summary>
        /// <param name="modelEmbeddingId"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(int modelEmbeddingId,
            DateTime? start,
            DateTime? end)
        {
            var query = _context.UsageMonths
                .Where(x => x.ModelEmbeddingId == modelEmbeddingId);

            if (start.HasValue)
                query = query.Where(x => x.Created.Date >= start.Value.Date);

            if (end.HasValue)
                query = query.Where(x => x.Created.Date <= end.Value.Date);

            var result = await query
                .GroupBy(x => x.Created.Date)
                .Select(g => new DashboardUsageDto(g.Key.Date.ToString("dd/MM"), g.Sum(x => x.Total)))
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Finds used model embeddings.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<ModelEmbeddingDto>> FindUsedModelEmbeddings()
        {
            var result = await _context.UsageMonths
                .Where(x => x.ModelEmbeddingId != null)
                .Select(x => x.ModelEmbedding)
                .Distinct()
                .Select(x => new ModelEmbeddingDto { Id = x!.Id, Name = x.Name })
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Finds the total usage cost for a specified time period.
        /// </summary>
        /// <remarks>This method queries the data source for usage records within the specified time range
        /// and calculates the sum of their total usage.</remarks>
        /// <param name="periodStart">The start date and time of the period to calculate usage for. This value is inclusive.</param>
        /// <param name="periodEnd">The end date and time of the period to calculate usage for. This value is exclusive.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total usage as an integer.</returns>
        public async Task<decimal> FindTotalUsageCostAsync(DateTime? periodStart, DateTime? periodEnd)
        {
            var query = _context.UsageMonths
                .Where(um => um.ModelEmbeddingId != null);

            if (periodStart.HasValue)
                query = query.Where(x => x.Created.Date >= periodStart.Value.Date);

            if (periodEnd.HasValue)
                query = query.Where(x => x.Created.Date <= periodEnd.Value.Date);

            return await query
                .Join(
                    _context.UsageUnits,
                    um => um.ModelEmbeddingId,
                    uu => uu.ModelEmbeddingId,
                    (um, uu) => (decimal)um.Total * uu.Value
                )
                .SumAsync();
        }
    }
}
