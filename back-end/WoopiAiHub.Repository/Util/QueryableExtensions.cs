using System.Linq.Dynamic.Core;

namespace WoopiAiHub.Repository.Util
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Orders the elements of a sequence based on a specified property or field name.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="query">The sequence of elements to order.</param>
        /// <param name="orderBy">The name of the property or field to order by. This must match a valid property or field name of
        /// <typeparamref name="T"/>.</param>
        /// <returns>An <see cref="IQueryable{T}"/> whose elements are sorted according to the specified property or field.</returns>
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string orderBy)
        {
            return query.OrderBy(orderBy);
        }
    }
}
