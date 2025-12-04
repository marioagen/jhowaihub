using WoopiAiHub.Domain.DTOs.Refit;
using Refit;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface IEmbeddingsApi
    {
        [Post("/{tenant}/api/v5/index/{hash}/customquery")]
        Task<HttpResponseMessage> CustomQuery([AliasAs("tenant")] string tenant,
                                              [AliasAs("hash")] string hash,
                                              CustomQueryRequestRefitDto customQueryRequestDto,
                                              [Header("ApiKey")] string authorization);       

        [Delete("/{tenantName}/api/v5/index/{hash}")]
        Task<HttpResponseMessage> DeleteHash([AliasAs("tenantName")] string tenantName, 
                                             [AliasAs("hash")] string hash,
                                             string tenant,
                                             [Header("ApiKey")] string authorization);
    }
}