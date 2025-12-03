namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// Step DTO for Phase 3 - includes step tools/ferramentas
    /// </summary>
    public record class StepPhase3Dto
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public ICollection<StepToolUpdateDto> StepTools { get; set; } = [];
    }
}
