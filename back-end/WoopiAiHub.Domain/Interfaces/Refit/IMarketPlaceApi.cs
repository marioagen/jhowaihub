using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using Refit;
using WoopiAiHub.Domain.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface IMarketPlaceApi
    {
        [Put("/Consumption/Questions")]
        Task<bool> ManageConsumptionQuestions([Header("KeyAccess")] string KeyAccess,
                                              ConsumptionQuestionsDto consumptionQuestions);

        [Put("/Consumption/Pages")]
        Task<bool> ManageConsumptionPages([Header("KeyAccess")] string KeyAccess,
                                          ConsumptionPagesDto consumptionPages);

        [Get("/user/CheckAccessByHub")]
        Task<ResponseCheckAccessDto> CheckAccessByHub([Header("KeyAccess")] string KeyAccess,
                                                      [Query] string email);

        [Get("/Consumption/CheckExceededPages")]
        Task<bool> CheckExceededPages([Header("KeyAccess")] string KeyAccess,
                                      [Query] string email);

        [Get("/user/TenantsByUserEmail")]
        Task<IEnumerable<string>> FindTenantsByUserEmail([Header("KeyAccess")] string KeyAccess,
                                                         [Query] string email);

        [Get("/user/CheckIsAdmin")]
        Task<bool> CheckIsAdmin([Header("KeyAccess")] string KeyAccess,
                                RequestCheckIsAdminDto requestCheckIsAdmin);

        [Get("/user/CheckAccessKey")]
        Task<ResponseCheckAccessDto> CheckAccessKey([Header("KeyAccess")] string KeyAccess,
                                                    RequestCheckAcessKeyDto request);

        [Get("/api/Tenant/{tenantName}")]
        Task<TenantInfoDto> FindTenantByName([Header("KeyAccess")] string KeyAccess,
                                             [AliasAs("tenantName")] string tenantName);

        [Put("/api/Tenant/{tenantName}/UpdateDatabaseStatus")]
        Task<bool> SendDatabaseCreatedNotification([Header("KeyAccess")] string KeyAccess,
                                                   [AliasAs("tenantName")] string tenantName);

        [Post("/user/AssignByHub")]
        Task<Guid> AssignLicensesByHub([Header("KeyAccess")] string KeyAccess,
                                       RequestAssignLicensesByHub requestAssignLicensesByHub);

        [Delete("/user/DeactivateUsers")]
        Task<bool> DeactivateUsersEnabledByReference([Header("KeyAccess")] string KeyAccess,
                                                    [FromBody] DeactivateUsersDto deactivateUsersDto);

        [Post("/api/Tenant/Subcription/ProcessPeriodConsumption")]
        Task<bool> ProcessSubcriptionPeriodConsumption([Header("KeyAccess")] string KeyAccess,
                                                       TenantConsumptionDto request);

        [Get("/api/Tenant/all/{module}")]
        Task<List<TenantListDto>> FindAllTenantsByModuleAsync([Header("KeyAccess")] string KeyAccess,
                                                              [AliasAs("module")] ColTypeModule module);
    }
}
