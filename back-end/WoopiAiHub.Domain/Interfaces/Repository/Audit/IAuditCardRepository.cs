using WoopiAiHub.Domain.Models.Audit;

namespace WoopiAiHub.Domain.Interfaces.Repository.Audit
{
    public interface IAuditCardRepository
    {
        Task AddAsync(AuditCard auditCard, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<AuditCard> auditCards, CancellationToken cancellationToken = default);
    }
}
