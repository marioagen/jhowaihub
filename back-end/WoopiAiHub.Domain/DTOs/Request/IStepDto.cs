namespace WoopiAiHub.Domain.DTOs.Request
{
    public interface IStepDto
    {
        string Name { get; }
        int Order { get; }
        int ProfileId { get; }
        int StatusId { get; }
        ICollection<StepToolUpdateDto> StepTools { get; set; }
    }
}
