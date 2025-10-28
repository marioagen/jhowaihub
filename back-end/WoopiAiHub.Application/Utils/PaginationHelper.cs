using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Utils;

namespace WoopiAiHub.Application.Utils
{
    public class PaginationHelper : IPaginationHelper
    {
        /// <summary>
        /// Paginates the list of given model
        /// </summary>
        /// <param name="PaginatedListDto"></param>
        /// <returns></returns>
        public static PaginatedListDto<T> Paginate<T>(
            IQueryable<T> query,
            int page,
            int pageSize = 10)
        {
            var totalCount = query.Count();
            int currentPage, pageCount;

            if (pageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
                pageSize = totalCount;
            }
            else
            {
                pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);
                currentPage = page <= pageCount && page > 0 ? page : 1;

                query = query
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);
            }

            return new PaginatedListDto<T>
            {
                Content = query.ToList(),
                CurrentPage = currentPage,
                PageCount = pageCount,
                RowCount = totalCount
            };
        }
    }
}
