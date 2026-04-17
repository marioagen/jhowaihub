using WoopiAiHub.Domain.Interfaces.Repository.Core;

namespace WoopiAiHub.Domain.Interfaces.Services.Core
{
    public partial interface IDataService<TContext>
        where TContext : IDbContext
    {
        
    }
}