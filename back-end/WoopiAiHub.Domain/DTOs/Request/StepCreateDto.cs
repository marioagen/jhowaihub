namespace WoopiAiHub.Domain.DTOs.Request
{
    public record struct StepCreateDto
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public int ProfileId { get; set; }
        public int StatusId { get; set; }
    }
}
