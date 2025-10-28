using AutoMapper;
using Google.Api;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class StepProfilePermissionsServices : IStepProfilePermissionsServices
    {
        private readonly IStepProfilePermissionsRepository _stepProfilePermissionsRepository;

        public StepProfilePermissionsServices(IStepProfilePermissionsRepository stepProfilePermissionsRepository)
        {
            _stepProfilePermissionsRepository = stepProfilePermissionsRepository;
        }

        /// <summary>
        /// It creates the relationship between Profile Step and Permission
        /// </summary>
        /// <param name="WorkflowPermissionDto"></param>
        /// <returns></returns>
        ///
        public async Task<bool> Create(int ProfileId, List<WorkflowPermissionDto> PermissionsWorkflow)
        {
            if (PermissionsWorkflow == null || PermissionsWorkflow.Count == 0)
                return false;

            await _stepProfilePermissionsRepository.Create(ProfileId, PermissionsWorkflow);
            return true;
        }

        /// <summary>
        /// It removes the relationship between Profile Step and Permission
        /// using a ProfileId
        /// </summary>
        /// <param name="ProfileId"></param>
        /// <returns></returns>
        ///
        public async Task<bool> Delete(int ProfileId)
        {
            return await _stepProfilePermissionsRepository.DeleteAsync(ProfileId);
        }
    }
}