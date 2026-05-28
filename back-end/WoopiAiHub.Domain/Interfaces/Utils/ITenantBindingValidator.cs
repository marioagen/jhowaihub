using Microsoft.AspNetCore.Http;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface ITenantBindingValidator
    {
        Task<bool> TryValidateRequestBindingAsync(HttpContext context, CancellationToken cancellationToken = default);
        string FindAndValidateTenant(string tenant, ICollection<TenantAccessDto> tenants);
    }
}
