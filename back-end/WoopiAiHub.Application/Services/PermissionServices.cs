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
        private const string WorkflowStepGroup = "Workflow-Step";
        private readonly IPermissionRepository _permissionRepository;

        public PermissionServices(IPermissionRepository permissionRepository)
        {
            this._permissionRepository = permissionRepository;
        }

        /// <summary>
        /// Find all permissions
        /// </summary>
        /// <returns></returns>
        public ICollection<PermissionDto> FindAll()
        {
            return _permissionRepository.FindAll();
        }

        /// <summary>
        /// Find all permissions
        /// </summary>
        /// <returns></returns>
        public ICollection<PermissionDto> FindWorkflowPermissions()
        {
            return _permissionRepository.FindAll()
                .Where(p => p.Group == WorkflowStepGroup)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Group = p.Group,
                    Description = p.Description
                })
                .ToList();
        }
    }
}
