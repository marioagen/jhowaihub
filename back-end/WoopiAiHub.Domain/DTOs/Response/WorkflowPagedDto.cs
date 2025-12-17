namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class WorkflowPagedDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public String? Login { get; set; } = null;
        public bool IsAllUsers { get; set; } = true;
        public string? OrderBy { get; set; } = null;
        public int? TeamId { get; set; }
        public Guid? UserId { get; set; }

    }
}
