using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class DocumentAnalysisRejectionRepository : IDocumentAnalysisRejectionRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentAnalysisRejectionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new document analysis rejection record in the database.
        /// </summary>
        /// <param name="rejection"></param>
        /// <returns></returns>
        public async Task<bool> CreateAsync(DocumentAnalysisRejection rejection)
        {
            await _context.DocumentAnalysisRejections.AddAsync(rejection);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Asynchronously adds a collection of document analysis rejections to the database and saves the changes.
        /// </summary>
        /// <remarks>This method performs a bulk insert operation and commits the changes in a single
        /// transaction. Ensure that the provided list contains valid entities to avoid exceptions.</remarks>
        /// <param name="rejections">A list of <see cref="DocumentAnalysisRejection"/> objects to add to the database. This parameter cannot be
        /// null or empty.</param>
        /// <returns>true if one or more changes were successfully saved to the database; otherwise, false.</returns>
        public async Task<bool> CreateRangeAsync(List<DocumentAnalysisRejection> rejections)
        {
            await _context.DocumentAnalysisRejections.AddRangeAsync(rejections);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Find all document analysis rejections for a specific card, including the user information of who made the rejection, ordered by creation date in descending order.
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public async Task<List<DocumentAnalysisRejectionDto>> FindByCardIdAsync(int cardId)
        {
            return await _context.DocumentAnalysisRejections
                .AsNoTracking()
                .Where(r => r.CardId == cardId)
                .Include(r => r.User)
                .OrderByDescending(r => r.Created)
                .Select(r => new DocumentAnalysisRejectionDto
                {
                    Id = r.Id,
                    Justification = r.Justification,
                    CardId = r.CardId,
                    StepId = r.StepId,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.Name : string.Empty,
                    Date = r.Created
                })
                .ToListAsync();
        }
    }
}
