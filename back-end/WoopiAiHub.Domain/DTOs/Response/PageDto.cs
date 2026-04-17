using WoopiAiHub.Domain.Core;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class PageDto : IPagedDto<UserDto>
    {
        public IEnumerable<UserDto> Content { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
    }
}