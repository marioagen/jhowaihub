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
        /// Adds an audit card entry to the context. Persistence occurs when the unit of work is committed.
        /// </summary>
        public void Add(AuditCard auditCard)
        {
            _context.AuditCards.Add(auditCard);
        }
    }
}
