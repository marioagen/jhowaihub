using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WoopiAiHub.Domain.DTOs
{
    public class FindPagedRequestDto
    {
        [FromQuery(Name = "search")]
        public string? Search {  get; set; }

        [FromQuery(Name = "page")]
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0.")]
        public int Page {  get; set; } = 1;

        [FromQuery(Name = "pagesize")]
        [Range(0, int.MaxValue, ErrorMessage = "PageSize must be equal 0 or greater than 0.")]
        public int PageSize { get; set; } = 0;

        [FromQuery(Name = "isascending")]
        public bool IsAscending { get; set; } = true;
    }
}
