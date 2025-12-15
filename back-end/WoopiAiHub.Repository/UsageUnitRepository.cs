using Google.Api;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class UsageUnitRepository : IUsageUnitRepository
    {
        private readonly ApplicationDbContext _context;

        public UsageUnitRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Find all usage units
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UsageUnitDto>> FindAllAsync()
        {
            return await _context.UsageUnits
                                 .AsNoTracking()
                                 .Select(uu => new UsageUnitDto
                                    {
                                        Id = uu.Id,
                                        Name = uu.Name,
                                        UsageTypeId = uu.UsageTypeId,
                                        UsageTypeName = uu.UsageType!.Name,
                                        ModelEmbeddingId = uu.ModelEmbeddingId,
                                        ModelEmbeddingName = uu.ModelEmbedding!.Name,
                                        Value = uu.Value
                                 })
                                 .ToListAsync();
        }

        /// <summary>
        /// Finds the total usage cost for a specified time period.
        /// </summary>
        /// <remarks>This method queries the data source for usage records within the specified time range
        /// and calculates the sum of their total usage.</remarks>
        /// <param name="periodStart">The start date and time of the period to calculate usage for. This value is inclusive.</param>
        /// <param name="periodEnd">The end date and time of the period to calculate usage for. This value is exclusive.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total usage as an integer.</returns>
        public async Task<List<UsageTotalByModelEmbeddingDto>> FindTotalUsageCostAsync(DateTime? periodStart, DateTime? periodEnd)
        {
            return await _context.UsageUnits
                .Where(uu => uu.ModelEmbeddingId != null)
                .Join(
                    _context.UsageMonths,
                    uu => uu.ModelEmbeddingId,
                    um => um.ModelEmbeddingId,
                    (uu, um) => new
                    {
                        uu.ModelEmbeddingId,
                        um.Total,
                        uu.Value
                    }
                )
                .GroupBy(x => x.ModelEmbeddingId)
                .Select(g => new UsageTotalByModelEmbeddingDto
                {
                    ModelEmbeddingId = g.Key,
                    Total = g.Sum(x => x.Total * x.Value)
                })
                .ToListAsync();
        }
    }
}
