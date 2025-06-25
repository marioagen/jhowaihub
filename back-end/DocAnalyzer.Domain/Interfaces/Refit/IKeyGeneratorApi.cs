using Microsoft.AspNetCore.Mvc;
using DocAnalyzer.Domain.DTOs.Refit;
using Refit;

namespace DocAnalyzer.Domain.Interfaces.Refit
{
    public interface IKeyGeneratorApi
    {
        [Get("/api/Key/Getkey")]
        Task<string> GetKey([Header("KeyAccess")] string KeyAccess, 
                            string tenant);
    }
}