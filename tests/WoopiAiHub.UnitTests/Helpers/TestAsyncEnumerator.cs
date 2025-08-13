namespace WoopiAiHub.UnitTests.Helpers
{

    /// <summary>
    /// A test implementation of IAsyncEnumerator that wraps a regular IEnumerator, enabling 
    /// asynchronous iteration in unit tests. It is typically used together with TestAsyncEnumerable 
    /// to simulate EF Core's async query execution without hitting a real database.
    /// </summary>
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask DisposeAsync() { 
            _inner.Dispose(); 
            GC.SuppressFinalize(this); 
            return ValueTask.CompletedTask; 
        }
        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
    }
}
