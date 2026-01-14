using Google.Api;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
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
        /// <param name="dateFilterDto"></param>
        /// <returns></returns>
        public async Task<IEnumerable<UsageUnitDto>> FindAllAsync(DateFilterDto? dateFilterDto = null)
        {
            var query = _context.UsageUnits.AsNoTracking();

            if (dateFilterDto != null)
            {
                var startDate = DateHelper.ParseDate(dateFilterDto.Start);
                if (startDate.HasValue)
                {
                    query = query.Where(x => x.Created.Date >= startDate.Value.Date);
                }

                var endDate = DateHelper.ParseDate(dateFilterDto.End);
                if (endDate.HasValue)
                {
                    query = query.Where(x => x.Created.Date <= endDate.Value.Date);
                }
            }

            return await query
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
    }
}
