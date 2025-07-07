using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUserRepository
    {
        public bool Create(User user);

        public List<User> FindByIds(List<Guid> ids);

        public bool DeactivateRange(List<Guid> ids);

        public bool Update(User user);

        public IQueryable<UserDtoPaged> FindAllPaged(PagedDataDto pagedDataDto);

    }
}
