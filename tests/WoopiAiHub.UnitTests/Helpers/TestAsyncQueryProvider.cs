using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace WoopiAiHub.UnitTests.Helpers
{
    /// <summary>
    /// A custom IAsyncQueryProvider implementation used for mocking asynchronous LINQ queries
    /// in unit tests. This allows methods like ToListAsync(), FirstOrDefaultAsync(), etc. to 
    /// work with in-memory IQueryable sources without relying on a real database.
    /// </summary
    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression) =>
            new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression) =>
            _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) =>
            _inner.Execute<TResult>(expression);

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) =>
            new TestAsyncEnumerable<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) =>
            Execute<TResult>(expression);
    }
}
