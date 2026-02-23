using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class PermissionServices :  IPermissionServices
    {        
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRepository _userRepository;

        public PermissionServices(IPermissionRepository permissionRepository,
            IUserRepository userRepository)
        {
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="group"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public async Task<bool> UserHasPermissionAsync(string email, string group, string permission)
        {
            var userProfile = await _userRepository.FindUserProfilesByEmailAsync(email);
            if (userProfile == null || userProfile.Count == 0)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, "User not found", UserLabel.NotFound);
            }
            bool isAdmin = userProfile.Contains("admin");
            var permissions = await _permissionRepository.FindUserPermissionsAsync(email);
            var hasPermission = permissions?.Any(p =>
                p.Value.Contains(permission) && p.Key == group) ?? false;
            return isAdmin || hasPermission;
        }
    }
}
