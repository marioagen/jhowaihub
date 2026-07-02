using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class ModelEmbeddingRepository : IModelEmbeddingRepository
    {
        private readonly ApplicationDbContext _context;

        public ModelEmbeddingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Find model embedding by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<ModelEmbedding?> FindByNameAsync(string name)
        {
            return await _context.ModelEmbeddings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == name);
        }

        /// <summary>
        /// Find all model embeddings by a list of names
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ModelEmbedding>> FindAllByNamesListAsync(List<string> names)
        {
            return await _context.ModelEmbeddings.AsNoTracking().Where(x => names.Contains(x.Name)).ToListAsync();
        }

        public async Task<IReadOnlyList<ModelEmbedding>> FindAllAsync()
        {
            return await _context.ModelEmbeddings.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        }
    }
}
