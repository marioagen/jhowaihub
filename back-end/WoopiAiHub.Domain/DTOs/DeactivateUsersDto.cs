using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public class DeactivateUsersDto
    {
        public IEnumerable<Guid> ReferenceUsers { get; set; } = new List<Guid>();
    }

}
