namespace WoopiAiIntegrationServices.Domain.Dtos.Request
{
    public record class QuestionAIGatewayDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
    }
}
