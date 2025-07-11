using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.DTOs.Refit;
using Refit;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface IKeyGeneratorApi
    {
        [Get("/api/Key/Getkey")]
        Task<string> GetKey([Header("KeyAccess")] string KeyAccess, 
                            string tenant);
    }
}