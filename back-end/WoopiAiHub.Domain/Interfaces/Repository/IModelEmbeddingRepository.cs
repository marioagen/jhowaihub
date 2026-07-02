using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IModelEmbeddingRepository
    {
        Task<ModelEmbedding?> FindByNameAsync(string name);
        Task<IEnumerable<ModelEmbedding>> FindAllByNamesListAsync(List<string> names);
        Task<IReadOnlyList<ModelEmbedding>> FindAllAsync();
    }
}
