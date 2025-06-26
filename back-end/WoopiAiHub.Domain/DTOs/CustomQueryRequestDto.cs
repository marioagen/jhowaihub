namespace WoopiAiHub.Domain.DTOs
{
    public class CustomQueryRequestDto
    {
        public string question { get; set; }
        public int? kValue { get; set; }
        public string? model { get; set; }
        public string? template { get; set; }
        public int? temperature { get; set; }
        public List<MessageDto>? prepend_messages { get; set; }
        public string? refine_template { get; set; }
    }
}
