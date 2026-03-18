namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Load-more result wrapper: items for the current page and a flag indicating if more are available.
    /// Used by auditor endpoints that use take 10, 20, 30 … pattern.
    /// </summary>
    /// <typeparam name="T">Item type (e.g. UserAuditorSummaryDto).</typeparam>
    public class AuditorLoadMoreResultDto<T>
    {
        public IEnumerable<T>? Items { get; set; }
        public bool HasMore { get; set; }
    }
}
