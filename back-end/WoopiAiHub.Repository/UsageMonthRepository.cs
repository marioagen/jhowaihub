using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
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
        /// Finds usage data by usage type.
        /// </summary>
        /// <param name="usageTypeId"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByUsageType(int usageTypeId)
        {
            var result = await _context.UsageMonths
                .Where(x => x.UsageTypeId == usageTypeId)
                .GroupBy(x => x.Created.Date)
                .Select(g => new DashboardUsageDto(g.Key.Date.ToString("dd/MM"), g.Sum(x => x.Total)))
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Finds usage data by model embedding ID.
        /// </summary>
        /// <param name="modelEmbeddingId"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(int modelEmbeddingId)
        {
            var result = await _context.UsageMonths
                .Where(x => x.ModelEmbeddingId == modelEmbeddingId)
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
    }
}
