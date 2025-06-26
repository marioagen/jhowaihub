using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public class UpdateHistoryDto
    {
        public int IdDocument { get; set; }
        public string OldOutput { get; set; }
        public string UpdatedOutput { get; set; }
    }
}
