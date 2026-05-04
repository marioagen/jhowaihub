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
        /// The query is materialized before projection so that <see cref="decimal.ToString(IFormatProvider?)"/>
        /// runs in C# (not SQL). This preserves the exact scale stored in the column
        /// (e.g. "0.000000790" with the trailing zero), which would otherwise be lost
        /// when serialized as a JSON number on the way to the frontend.
        /// </remarks>
        public async Task<IEnumerable<UsageUnitDto>> FindAllAsync()
        {
            var rows = await _context.UsageUnits
                                     .AsNoTracking()
                                     .Include(uu => uu.UsageType)
                                     .Include(uu => uu.ModelEmbedding)
                                     .ToListAsync();

            return rows.Select(uu => new UsageUnitDto
            {
                Id = uu.Id,
                Name = uu.Name,
                UsageTypeId = uu.UsageTypeId,
                UsageTypeName = uu.UsageType?.Name ?? string.Empty,
                ModelEmbeddingId = uu.ModelEmbeddingId,
                ModelEmbeddingName = uu.ModelEmbedding?.Name ?? string.Empty,
                Value = uu.Value.ToString(CultureInfo.InvariantCulture)
            });
        }
    }
}
