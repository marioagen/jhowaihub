using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class TenantLlmModelSettingsRepository : ITenantLlmModelSettingsRepository
    {
        private readonly ApplicationDbContext _context;

        public TenantLlmModelSettingsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TenantLlmModelSetting>> GetAllAsync()
        {
            return await _context.TenantLlmModelSettings
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpsertAsync(IEnumerable<TenantLlmModelSetting> settings)
        {
            foreach (var setting in settings)
            {
                var existing = await _context.TenantLlmModelSettings
                    .FirstOrDefaultAsync(x => x.Scope == setting.Scope);

                if (existing is null)
                {
                    await _context.TenantLlmModelSettings.AddAsync(setting);
                    continue;
                }

                existing.ModelName = setting.ModelName;
                existing.UpdatedAt = setting.UpdatedAt;
                existing.UpdatedByEmail = setting.UpdatedByEmail;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteByScopesAsync(IEnumerable<string> scopes)
        {
            var scopeList = scopes.ToList();
            if (scopeList.Count == 0)
            {
                return;
            }

            var rows = await _context.TenantLlmModelSettings
                .Where(x => scopeList.Contains(x.Scope))
                .ToListAsync();

            if (rows.Count == 0)
            {
                return;
            }

            _context.TenantLlmModelSettings.RemoveRange(rows);
            await _context.SaveChangesAsync();
        }
    }
}
