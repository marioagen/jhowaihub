using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public class TeamDtoDb
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public virtual ICollection<User>? Users { get; set; }

    }
}
