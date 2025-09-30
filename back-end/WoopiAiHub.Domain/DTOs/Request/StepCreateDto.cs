namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class StepCreateDto : IStepDto
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public int ProfileId { get; set; }
        public int StatusId { get; set; }
        public ICollection<StepToolUpdateDto> StepTools { get; set; }
    }
}
