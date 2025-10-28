using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class PaginatedListDto<T>
    {
        public IEnumerable<T> Content { get; set; } = [];
        public int CurrentPage { get; set; } = 1;
        public int PageCount { get; set; } = 0;
        public int RowCount { get; set; } = 0;
        public int PageSize { get; set; } = 10;
    }
}