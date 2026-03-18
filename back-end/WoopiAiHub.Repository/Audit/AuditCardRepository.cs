using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Models.Audit;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository.Audit
{
    public class AuditCardRepository : IAuditCardRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditCardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds an audit card entry to the context and persists immediately.
        /// </summary>
        public async Task AddAsync(AuditCard auditCard, CancellationToken cancellationToken = default)
        {
            _context.AuditCards.Add(auditCard);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Adds multiple audit card entries to the context and persists immediately.
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<AuditCard> auditCards, CancellationToken cancellationToken = default)
        {
            _context.AuditCards.AddRange(auditCards);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes audit cards associated with the provided card IDs.
        /// </summary>
        public async Task DeleteByCardIdsAsync(List<int> cardIds)
        {
            if (cardIds == null || !cardIds.Any())
                return;

            var auditCards = await _context.AuditCards.Where(ac => cardIds.Contains(ac.CardId)).ToListAsync();
            if (auditCards.Any())
            {
                _context.AuditCards.RemoveRange(auditCards);
                await _context.SaveChangesAsync();
            }
        }
    }
}
