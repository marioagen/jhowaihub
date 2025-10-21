using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class StepProfilePermissionsRepository : IStepProfilePermissionsRepository
    {
        private readonly ApplicationDbContext _context;
        public StepProfilePermissionsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Create(int ProfileId, List<WorkflowPermissionDto> PermissionsWorkflow)
        {
            //var entity = new StepProfilePermission(StepId, ProfileId, PermissionId);

            //_context.StepProfilePermissions.Add(entity);
            //return await _context.SaveChangesAsync() > 0;

            foreach (var permission in PermissionsWorkflow)
            {
                var stepProfilePermission = new StepProfilePermission(
                    stepId: permission.StepId,
                    profileId: ProfileId,
                    permissionId: permission.PermissionId
                );

                _context.StepProfilePermissions.Add(stepProfilePermission);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int ProfileId)
        {
            var list = await _context.StepProfilePermissions
                .Where(x => x.ProfileId == ProfileId)
                .ToListAsync();

            if (!list.Any())
                return false;

            _context.StepProfilePermissions.RemoveRange(list);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}