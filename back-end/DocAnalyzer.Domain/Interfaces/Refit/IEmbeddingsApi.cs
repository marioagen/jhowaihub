using DocAnalyzer.Domain.DTOs.Refit;
using Refit;
using DocAnalyzer.Domain.DTOs;

namespace DocAnalyzer.Domain.Interfaces.Refit
{
    public interface IEmbeddingsApi
    {
        [Post("/api/v3/index/{hash}/customquery")]
        Task<HttpResponseMessage> CustomQuery([AliasAs("hash")] string hash,
                                              CustomQueryRequestRefitDto customQueryRequestDto,
                                              [Header("ApiKey")] string authorization);
        
        [Post("/api/v3/index/{hash}/document")]
        Task<string> AddDocuments([AliasAs("hash")] string hash,
                                  AddDocumentsRequestRefitDto request,
                                  [Header("ApiKey")] string authorization);

        [Delete("/api/v3/index/{hash}")]
        Task<HttpResponseMessage> DeleteHash([AliasAs("hash")] string hash,
                                              string tenant,
                                              [Header("ApiKey")] string authorization);


    }
}