using Refit;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface IServicesWoopiAiApi
    {
        [Multipart]
        [Post("/api/anonimization")]
        Task<HttpResponseMessage> ProcessAnonymization(
            [Query("documentName")] string documentName,
            [AliasAs("file")] ByteArrayPart file,
            [Query("uriResponse")] string uriResponse,
            [Query("woopiAiPromptId")] int? woopiAiPromptId,
            [Header("X-Email")] string email,
            [Header("Api-Key")] string apiKey
        );
    }
}
