using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ITenantServices
    {
        Task<IEnumerable<string>> FindAllByUserEmail(string email);

        Task<string> InitializeTenant(string tenant);
    }
}