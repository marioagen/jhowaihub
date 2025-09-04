using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public class DocumentInputDto
    {
        public int Id { get; set; }

        public string Input { get; set; } = string.Empty;
    }
}
