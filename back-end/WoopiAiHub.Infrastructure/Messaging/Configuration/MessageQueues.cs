namespace WoopiAiHub.Infrastructure.Messaging.Configuration
{
    public class MessageQueues
    {
        public string OcrQueue { get; set; } = string.Empty;
        public string EmbeddingQueue { get; set; } = string.Empty;
        public string ChatCompletion { get; set; } = string.Empty;
        public string AnswerQueue { get; set; } = string.Empty;
        public string OcrQueueAiHubResponse { get; set; } = string.Empty;
        public string EmbeddingQueueAiHubResponse { get; set; } = string.Empty;
        public string AnswerQueueAiHubResponse { get; set; } = string.Empty;
        public string ChatCompletionQueueResponse { get; set; } = string.Empty;

        public IEnumerable<string> Queues()
        {
            return 
            [
              OcrQueue,
              EmbeddingQueue,
              ChatCompletion,
              AnswerQueue,
              OcrQueueAiHubResponse,
              EmbeddingQueueAiHubResponse,
              AnswerQueueAiHubResponse,
              ChatCompletionQueueResponse

            ];
        }
    }
}
