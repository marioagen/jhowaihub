using Refit;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface In8NConnector
    {
        [Get("/api/v1/workflows")]
        Task<ApiResponse<string>> FindWorkflows([Header("X-N8N-API-KEY")] string apiKey,
                                                [Query] string active = "true",
                                                [Query] string excludePinnedData = "true");
        [Get("/webhook/{webhookId}")]
        [Headers("Content-Type: application/json")] 
        Task<ApiResponse<string>> FindWorkflowInputs([AliasAs("webhookId")] string webhookId);

    }
}
