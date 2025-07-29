using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IProfileRepository
    {
        bool CreateUniqueProfile(Profile team);
        ICollection<Profile> FindAll();
        ProfileDto? FindById(int id);
        bool Update(Profile team);
        bool DeleteByIds(List<int> ids);
        IQueryable<ProfileDto> FindAllPaged(PagedDataDto pagedDataDto);
        List<Profile> FindByIds(IEnumerable<int> ids);
        Profile FindByIdReturnModel(int id);
    }
}
