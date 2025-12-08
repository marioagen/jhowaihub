using AutoMapper;
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

        /// <summary>
        /// It creates the relationship between Profile Step and Permission
        /// </summary>
        /// <param name="WorkflowPermissionDto"></param>
        /// <returns></returns>
        public async Task<bool> Create(int ProfileId, List<WorkflowPermissionDto> PermissionsWorkflow)
        {
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

        /// <summary>
        /// It removes the relationship between Profile Step and Permission
        /// using a list of ProfileIds
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteListAsyncByIds(List<int> ProfileIds)
        {
            var list = await _context.StepProfilePermissions
                    .Where(x => ProfileIds.Contains(x.ProfileId))
                    .ToListAsync();

            if (list.Count() < 0)
                return false;

            _context.StepProfilePermissions.RemoveRange(list);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// It removes the relationship between Profile Step and Permission
        /// using a ProfileId
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int ProfileId)
        {
            var list = await _context.StepProfilePermissions
                .Where(x => x.ProfileId == ProfileId)
                .ToListAsync();

            if (list.Count() < 0)
                return false;

            _context.StepProfilePermissions.RemoveRange(list);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// It removes the relationship between Profile Step and Permission
        /// using a ProfileId, StepId and PermissionId
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRowAsync(int ProfileId, int StepId, int PermissionId)
        {
            var entity = await _context.StepProfilePermissions
                .FirstOrDefaultAsync(x =>
                    x.ProfileId == ProfileId &&
                    x.StepId == StepId &&
                    x.PermissionId == PermissionId);

            if (entity == null)
                return false;

            _context.StepProfilePermissions.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}