using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Repository;

namespace WoopiAiHub.Application.Services
{
    public class PermissionServices :  IPermissionServices
    {        
        private readonly IPermissionRepository _permissionRepository;

        public PermissionServices(IPermissionRepository permissionRepository)
        {
            this._permissionRepository = permissionRepository;
        }

        /// <summary>
        /// Find all permissions
        /// </summary>
        /// <returns></returns>
        public List<GroupedPermissionsDto> FindAll()
        {
            var permissions = _permissionRepository.FindAll();

            return permissions
                .GroupBy(p => p.Group)
                .Select(g => new GroupedPermissionsDto
                {
                    Group = g.Key,
                    Permissions = g.ToList()
                })
                .ToList();
        }

        /// <summary>
        /// Find workflow permissions
        /// </summary>
        /// <returns></returns>
        public ICollection<PermissionDto> FindWorkflowPermissions()
        {
            return _permissionRepository.FindWorkflowPermissions();            
        }
    }
}
