using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class UserPagedResultDto
    {
      public IEnumerable<UserDtoPaged> Content { get; set; } = Enumerable.Empty<UserDtoPaged>();
      public int CurrentPage { get; set; }
      public int PageCount { get; set; }
      public int RowCount { get; set; }
    }
}
