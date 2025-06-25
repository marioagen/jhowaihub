using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.DTOs.Refit;
using DocAnalyzer.Domain.Enum;
using Refit;

namespace DocAnalyzer.Domain.Interfaces.Refit
{
    public interface IMarketPlaceApi
    {
        [Put("/Consumption/Questions")]
        Task<bool> ManageConsumptionQuestions([Header("KeyAccess")] string KeyAccess,
                                              ConsumptionQuestionsDto consumptionQuestions);

        [Put("/Consumption/Pages")]
        Task<bool> ManageConsumptionPages([Header("KeyAccess")] string KeyAccess,
                                          ConsumptionPagesDto consumptionPages);

        [Get("/user/CheckAccess")]
        Task<ResponseCheckAccessDto> CheckAccess([Header("KeyAccess")] string KeyAccess,
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

        [Get("/api/Tenant/{tenantName}/{module}")]
        Task<TenantInfoDto> FindTenantByNameAndModule([Header("KeyAccess")] string KeyAccess,
                                                      [AliasAs("tenantName")] string tenantName,
                                                      [AliasAs("module")] ColTypeModule module);
    }
}