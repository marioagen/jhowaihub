namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class UpdateAssignedUserDto
    {
        public Guid? UserId { get; set; }
        public int CardId { get; set; }
    }
}
