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
        /// Retrieves all anonymized document records associated with the specified document identifier.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document for which to retrieve anonymized versions.</param>
        /// <returns>A collection of <see cref="DocumentAnonymizationDto"/> objects representing the anonymized versions of the
        /// specified document. The collection is empty if no anonymized documents are found.</returns>
        public async Task<ICollection<DocumentAnonymizationDto>> FindAnonymizedDocumentsByDocument(int documentId)
        {
            return await _context.DocumentAnonymizations
                .Where(da => da.DocumentId == documentId)
                .Select(da => new DocumentAnonymizationDto
                {
                    Id = da.Id,
                    DocumentId = da.DocumentId,
                    DocumentUrl = da.DocumentUrl,
                    DocumentName = da.Document != null ? da.Document.Name : string.Empty,
                    Created = da.Created
                })
                .OrderByDescending(da => da.Created)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
