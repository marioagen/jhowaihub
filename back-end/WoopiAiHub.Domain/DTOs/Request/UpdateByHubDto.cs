using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public class UpdateByHubDto
    {
        public string UserEmail { get; set; } = string.Empty;

        public string Tenant { get; set; } = string.Empty;

        public Guid Reference_user { get; set; }
    }
}
