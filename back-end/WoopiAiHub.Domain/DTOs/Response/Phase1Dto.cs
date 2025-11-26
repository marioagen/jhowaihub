using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class Phase1Dto
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<TeamDto> Teams { get; set; } = [];
    }
}
