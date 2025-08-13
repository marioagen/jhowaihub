using System.Linq.Expressions;

namespace WoopiAiHub.UnitTests.Helpers
{
    /// <summary>
    /// An in-memory asynchronous enumerable that wraps a standard IEnumerable and provides 
    /// support for async iteration in unit tests. This is commonly used to simulate Entity 
    /// Framework Core's IAsyncEnumerable behavior when mocking DbSet or IQueryable sources.
    /// </summary>
    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable) { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider =>
            new TestAsyncQueryProvider<T>(this);
    }
}
