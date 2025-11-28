namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ITenantServices
    {
        Task<IEnumerable<string>> FindAllByUserEmail(string email);
        Task<string> InitializeTenant(string tenant);
        Task<string> FindPlanByName(string tenant);
    }
}