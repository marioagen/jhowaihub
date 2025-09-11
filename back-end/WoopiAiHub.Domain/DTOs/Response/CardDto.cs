using WoopiAiHub.Domain.Enum;
namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class CardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public int StepId { get; set; }
        public int Order { get; set; }
        public int DocumentId { get; set; }
        public UserDto? AssignedUser { get; set; }
        public ProfileDto Profile { get; set; } = new();
        public DocumentStatus StatusDocument { get; set; } = new();
        public StatusDto Status { get; set; } = new();
        public DateTime Created { get; set; }
    }
}
