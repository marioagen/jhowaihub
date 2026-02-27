using WoopiAiHub.Domain.Models.Audit;

namespace WoopiAiHub.Domain.Interfaces.Repository.Audit
{
    public interface IAuditCardRepository
    {
        void Add(AuditCard auditCard);
    }
}
