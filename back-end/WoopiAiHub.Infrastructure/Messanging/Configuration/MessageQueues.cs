namespace WoopiAiHub.Infrastructure.Messanging.Configuration
{
    public class MessageQueues
    {
        public string OcrQueue { get; set; } = string.Empty;
        public string EmbeddingQueue { get; set; } = string.Empty;
        public string AnswerQueue { get; set; } = string.Empty;
        public string OcrQueueExtratorResponse { get; set; } = string.Empty;
        public string EmbeddingQueueResponse { get; set; } = string.Empty;
        public string AnswerQueueResponse { get; set; } = string.Empty;

        public IEnumerable<string> Queues()
        {
            return 
            [
              OcrQueue,
              EmbeddingQueue,
              AnswerQueue,
              OcrQueueExtratorResponse,
              EmbeddingQueueResponse,
              AnswerQueueResponse,
            ];
        }
    }
}
