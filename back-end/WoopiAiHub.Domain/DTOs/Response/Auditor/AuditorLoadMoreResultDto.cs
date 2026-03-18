namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public class AuditorLoadMoreResultDto<T>
    {
        public IEnumerable<T>? Items { get; set; }
        public bool HasMore { get; set; }
    }
}
