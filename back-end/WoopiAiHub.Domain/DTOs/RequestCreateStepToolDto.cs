namespace WoopiAiHub.Domain.DTOs
{
    public record class RequestCreateStepToolDto
    {
        public int ToolId { get; set; }
        public int StepId { get; set; }
        public int Order { get; set; }
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
    }
}
