using AutoMapper;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IStepProfilePermissionsServices
    {
        Task<bool> Create(int ProfileId, List<WorkflowPermissionDto> PermissionsWorkflow);
        Task<bool> DeleteRow(List<StepProfilePermission> permissions);
        Task<bool> Delete(int ProfileId);
        Task DeleteByIds(List<int> ProfileIds);
    }
}
