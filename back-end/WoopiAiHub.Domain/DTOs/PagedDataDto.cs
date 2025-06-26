using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public class PagedDataDto
    {
        public int Page { get; set; }
        public string? Search { get; set; }
        public bool IsAscending { get; set; }
    }
}
