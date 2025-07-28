using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Request;


namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IProfileServices
    {
        ProfileDto FindById(int id);
        ProfilePagedResultDto FindAllPaged(PagedDataDto pagedDataDto);
        Task<bool> CreateUniqueProfile(ProfileCreateDto profileCreateDto);
        Task<bool> Update(ProfileUpdateDto profileUpdateDto);
        bool DeleteByIds(List<int> ids);
    }
}
