using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IProfileServices
    {
        public ProfilePagedResultDto FindAllPaged(PagedDataDto pagedDataDto);
    }
}
