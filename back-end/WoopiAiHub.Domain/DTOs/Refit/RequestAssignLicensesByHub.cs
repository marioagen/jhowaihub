using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Refit
{
    public class RequestAssignLicensesByHub
    {
        public string UserEmail { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public Guid IdUser { get; set; } = Guid.Empty;
    }
}
