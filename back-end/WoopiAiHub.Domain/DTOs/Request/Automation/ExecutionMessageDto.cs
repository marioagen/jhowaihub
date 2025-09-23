namespace WoopiAiHub.Domain.DTOs.Request.Automation
{
    public record class ExecutionMessageDto
    {
        public string Queue { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
