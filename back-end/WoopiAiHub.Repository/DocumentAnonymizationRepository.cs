using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository
{
    public class DocumentAnonymizationRepository(Context.ApplicationDbContext context) : IDocumentAnonymizationRepository
    {
        private readonly Context.ApplicationDbContext _context = context;

        /// <summary>
        /// Asynchronously adds a new document anonymization record to the data store.
        /// </summary>
        /// <param name="documentAnonymization">The document anonymization entity to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the record
        /// was successfully created; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> CreateAsync(DocumentAnonymization documentAnonymization)
        {
            await _context.DocumentAnonymizations.AddAsync(documentAnonymization);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Asynchronously retrieves all anonymized documents associated with the specified creator email address.
        /// </summary>
        /// <param name="email">The email address of the document creator to search for. Cannot be null.</param>
        /// <returns>A collection of anonymized document data transfer objects associated with the specified email address. The
        /// collection is empty if no matching documents are found.</returns>
        public async Task<ICollection<DocumentAnonymizationDto>> FindAnonymizedDocumentsByEmail(string email)
        {
            return await _context.DocumentAnonymizations
                .Where(da => da.Document != null && da.Document.EmailCreator == email)
                .Select(da => new DocumentAnonymizationDto
                {
                    Id = da.Id,
                    DocumentId = da.DocumentId,
                    DocumentUrl = da.DocumentUrl,
                    DocumentName = da.Document != null ? da.Document.Name : string.Empty,
                    Created = da.Created
                }).ToListAsync();
        }
    }
}
