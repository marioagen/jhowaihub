namespace WoopiAiHub.Infrastructure.Messaging.Configuration
{
    public class MessageQueues
    {
        public string OcrQueue { get; set; } = string.Empty;
        public string EmbeddingQueue { get; set; } = string.Empty;
        public string ChatCompletionQueue { get; set; } = string.Empty;
        public string AnswerQueue { get; set; } = string.Empty;
        public string OcrQueueAiHubResponse { get; set; } = string.Empty;
        public string EmbeddingQueueAiHubResponse { get; set; } = string.Empty;
        public string AnswerQueueAiHubResponse { get; set; } = string.Empty;
        public string AutomationQueueConsumer {  get; set; } = string.Empty;
        public string AutomationQueueResponse { get; set; } = string.Empty;
        public string ChatCompletionQueueAiHubResponse { get; set; } = string.Empty;

        public IEnumerable<string> Queues()
        {
            return 
            [
              OcrQueue,
              EmbeddingQueue,
              ChatCompletionQueue,
              AnswerQueue,
              OcrQueueAiHubResponse,
              EmbeddingQueueAiHubResponse,
              AnswerQueueAiHubResponse,
              AutomationQueueConsumer,
              AutomationQueueResponse,
              ChatCompletionQueueAiHubResponse
            ];
        }
    }
}
