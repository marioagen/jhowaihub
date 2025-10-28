using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IPermissionRepository
    {
        public Task<List<Permission>> FindByIdsAsync(List<int> ids);
        public ICollection<PermissionDto> FindAll();
        public ICollection<PermissionDto> FindWorkflowPermissions();
        public Task<Dictionary<string, List<string>>> FindUserPermissionsAsync(string email);
    }
}
