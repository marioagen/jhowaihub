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
        /// Adds an audit card entry to the context. Call <see cref="SaveChangesAsync"/> to persist.
        /// </summary>
        public void Add(AuditCard auditCard)
        {
            _context.AuditCards.Add(auditCard);
        }

        /// <summary>
        /// Persists all pending changes in the context (e.g. audit entries added via <see cref="Add"/>).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
