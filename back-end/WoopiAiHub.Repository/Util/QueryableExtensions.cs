using System.Linq.Dynamic.Core;

namespace WoopiAiHub.Repository.Util
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string orderBy)
        {
            // Usa o método OrderBy do pacote Dynamic.Core, que aceita uma string como argumento
            return query.OrderBy(orderBy);
        }
    }
}
