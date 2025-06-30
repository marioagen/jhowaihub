namespace WoopiAiHub.Domain.DTOs.Response
{
    public class TeamPagedResultDto
    {
        public IEnumerable<TeamDto> Content { get; set; } = Enumerable.Empty<TeamDto>();
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
    }
}
