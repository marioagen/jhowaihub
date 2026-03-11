using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class DocumentBatchRepository(ApplicationDbContext context) : IDocumentBatchRepository
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Creates a new document batch and saves it to the database asynchronously.
        /// </summary>
        /// <remarks>This method performs an asynchronous operation to add the document batch to the
        /// context and save changes. Ensure that the provided batch is valid and meets any necessary constraints before
        /// calling this method.</remarks>
        /// <param name="batch">The document batch to be created. This parameter cannot be null.</param>
        /// <returns>true if the document batch was successfully created and saved; otherwise, false.</returns>
        public async Task<DocumentBatch> CreateAsync(DocumentBatch batch)
        {
            await _context.DocumentBatchs.AddAsync(batch);
            await _context.SaveChangesAsync();
            return batch;
        }
    }
}
