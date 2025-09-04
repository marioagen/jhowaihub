namespace WoopiAiHub.Domain.DTOs.Refit
{
    public class CustomQueryRequestRefitDto
    {
        public string Question { get; set; }
        public int? kValue { get; set; }
        public string? Model { get; set; }
        public string? Template { get; set; }
        public int? Temperature { get; set; }
        public string? Refine_template { get; set; }
        public int? Max_tokens { get; set; }
        public string? SearchMode { get; set; }
        public string? Tenant { get; set; }
    }
}
