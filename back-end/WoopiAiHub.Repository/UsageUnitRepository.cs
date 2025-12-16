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
    }
}
