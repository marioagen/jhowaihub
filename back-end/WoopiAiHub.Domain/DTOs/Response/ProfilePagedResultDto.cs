namespace WoopiAiHub.Domain.DTOs.Response
{
    public  class ProfilePagedResultDto
    {
        public IEnumerable<ProfileDto> Content { get; set; } = Enumerable.Empty<ProfileDto>();
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
    }
}
