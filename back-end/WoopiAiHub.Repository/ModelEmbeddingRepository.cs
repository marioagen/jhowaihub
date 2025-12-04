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
            return await _context.ModelEmbeddings.FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}
