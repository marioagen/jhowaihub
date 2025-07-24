using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IPermissionServices
    {
        ICollection<PermissionDto> FindAll();
    }
}
