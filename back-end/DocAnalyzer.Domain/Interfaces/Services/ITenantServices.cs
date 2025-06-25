using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.DTOs.Refit;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.Models;
using DocAnalyzer.Domain.Utils;

namespace DocAnalyzer.Domain.Interfaces.Services
{
    public interface ITenantServices
    {
        Task<IEnumerable<string>> FindAllByUserEmail(string email);

        Task<string> InitializeTenant(string tenant);
    }
}