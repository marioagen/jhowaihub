using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class UserPagedResultDto
    {
      public IEnumerable<UserPagedDto> Content { get; set; } = Enumerable.Empty<UserPagedDto>();
      public int CurrentPage { get; set; }
      public int PageCount { get; set; }
      public int RowCount { get; set; }
    }
}
