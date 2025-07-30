using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IPermissionRepository
    {
        public Task<List<Permission>> FindByIdsAsync(List<int> ids);
        public ICollection<PermissionDto> FindAll();
        public Task<List<string>> GetUserPermissionsAsync(string email);
    }
}
