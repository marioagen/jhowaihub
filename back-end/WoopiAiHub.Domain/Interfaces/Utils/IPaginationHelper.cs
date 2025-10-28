using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface IPaginationHelper
    {
        public interface IPaginationHelper
        {
            PaginatedListDto<T> Paginate<T>(
                IQueryable<T> query,
                int page,
                int pageSize) where T : class;
        }
    }
}