using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class UserEnabledResponse
    {
        public Guid ReferenceUser { get; set; }
    }
}
