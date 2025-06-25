using Microsoft.AspNetCore.Mvc;

namespace DocAnalyzer.Domain.DTOs.Request
{
    public class TypeDocCreateDto
    {
        [FromQuery(Name = "name")]
        public string Name { get; set; } = string.Empty;
    }
}
