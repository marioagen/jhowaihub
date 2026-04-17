namespace WoopiAiHub.Domain.Core
{
    public interface IPagedDto<IDto>
    {
        IEnumerable<IDto> Content { get; set; }
        int CurrentPage { get; set; }
        int PageCount { get; set; }
        int RowCount { get; set; }
    }
}