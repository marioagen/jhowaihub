namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class Phase1Dto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<TeamDto> Teams { get; set; } = [];
    }
}
