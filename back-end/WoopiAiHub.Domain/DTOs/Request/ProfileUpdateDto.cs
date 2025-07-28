using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class ProfileUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<int> PermissionsIds { get; set; } = new List<int>();
    }
}
