using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.Interfaces.Repository;
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
        /// Find all usage units.
        /// </summary>
        /// <remarks>
        /// The SQL projection selects only the columns needed (no entity tracking, no extra joins
        /// fetching whole related rows), and the materialized rows are then mapped to the DTO in
        /// memory. The two-step shape is required because <see cref="decimal.ToString(IFormatProvider?)"/>
        /// is not translatable to SQL by EF Core, and we need it to preserve the exact scale stored
        /// in the column (e.g. "0.000000790" with the trailing zero) — which would otherwise be lost
        /// when serialized as a JSON number on the way to the frontend.
        /// </remarks>
        public async Task<IEnumerable<UsageUnitDto>> FindAllAsync()
        {
            var rows = await _context.UsageUnits
                                     .AsNoTracking()
                                     .Select(uu => new
                                     {
                                         uu.Id,
                                         uu.Name,
                                         uu.UsageTypeId,
                                         UsageTypeName = uu.UsageType != null ? uu.UsageType.Name : null,
                                         uu.ModelEmbeddingId,
                                         ModelEmbeddingName = uu.ModelEmbedding != null ? uu.ModelEmbedding.Name : null,
                                         uu.Value
                                     })
                                     .ToListAsync();

            return rows.Select(r => new UsageUnitDto
            {
                Id = r.Id,
                Name = r.Name,
                UsageTypeId = r.UsageTypeId,
                UsageTypeName = r.UsageTypeName ?? string.Empty,
                ModelEmbeddingId = r.ModelEmbeddingId,
                ModelEmbeddingName = r.ModelEmbeddingName ?? string.Empty,
                Value = r.Value.ToString(CultureInfo.InvariantCulture)
            });
        }
    }
}
