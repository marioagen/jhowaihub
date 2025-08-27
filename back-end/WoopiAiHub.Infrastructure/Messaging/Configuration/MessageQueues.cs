namespace WoopiAiHub.Infrastructure.Messaging.Configuration
{
    public class MessageQueues
    {
        public string OcrQueue { get; set; } = string.Empty;
        public string EmbeddingQueue { get; set; } = string.Empty;
        public string AnswerQueue { get; set; } = string.Empty;
        public string OcrQueueAiHubResponse { get; set; } = string.Empty;
        public string EmbeddingQueueAiHubResponse { get; set; } = string.Empty;
        public string AnswerQueueAiHubResponse { get; set; } = string.Empty;

        public IEnumerable<string> Queues()
        {
            return 
            [
              OcrQueue,
              EmbeddingQueue,
              AnswerQueue,
              OcrQueueAiHubResponse,
              EmbeddingQueueAiHubResponse,
              AnswerQueueAiHubResponse,
            ];
        }
    }
}
