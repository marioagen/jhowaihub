using WoopiAiHub.Domain.Models;

public interface ITenantRepository
{
    bool CreateDatabase();
    bool CreateUniqueTenant(Tenant tenant);
    Tenant? FindByMarketPlaceId(Guid marketplaceId);
    bool Update(Tenant tenant);
}