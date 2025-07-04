using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public class UserCreateDto
    {

        public string Name { get;  set; } = string.Empty;

        public string Email { get;  set; } = string.Empty;

        public ICollection<int> TeamIds { get; set; } // Use os IDs dos times
    }

}

