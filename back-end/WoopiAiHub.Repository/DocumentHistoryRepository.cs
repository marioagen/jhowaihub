using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using System.Linq.Dynamic.Core;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Repository
{
    public class DocumentHistoryRepository : IDocumentHistoryRepository
    {
        private readonly Context.ApplicationDbContext _context;

        public DocumentHistoryRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a document history in the database
        /// </summary>
        /// <param name="documentHistory"></param>
        /// <returns></returns>
        public bool Create(DocumentHistory documentHistory)
        {
            _context.Add(documentHistory);
            _context.SaveChanges();

            return true;
        }

        /// <summary>
        /// Find the list of DocumentHistory by the id of the document 
        /// </summary>
        /// <param name="idDocument"></param>
        /// <returns></returns>
        public IEnumerable<DocumentHistory> FindById(int idDocument)
        {
            return _context.DocumentHistories.Where(a => a.IdDocument.Equals(idDocument)).AsNoTracking();
        }

        /// <summary>
        /// Find the first N DocumentHistory entries by the id of the document (cumulative load: first 10, then 20, then 30...).
        /// Optional filter by search (Input or Output), order and orderBy (e.g. orderBy=created, order=desc).
        /// </summary>
        /// <param name="idDocument"></param>
        /// <param name="take"></param>
        /// <param name="search">Optional. Filter by text in Input or Output.</param>
        /// <param name="order">Optional. "asc" or "desc". Default desc.</param>
        /// <param name="orderBy">Optional. "created". Default created.</param>
        /// <returns></returns>
        public IEnumerable<DocumentHistory> FindByIdWithTake(int idDocument, int take, string? search = null, string? order = null, string? orderBy = null)
        {
            var query = _context.DocumentHistories
                .Include(h => h.User)
                .Where(a => a.IdDocument == idDocument);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(a => (a.Input != null && a.Input.Contains(term)) || (a.Output != null && a.Output.Contains(term)));
            }

            var isDesc = string.IsNullOrWhiteSpace(order) || order.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);
            query = isDesc ? query.OrderByDescending(a => a.Created) : query.OrderBy(a => a.Created);

            return query.Take(take).AsNoTracking();
        }

        /// <summary>
        /// Delete a DocumentHistory by the id of the document 
        /// </summary>
        /// <param name="idDocument"></param>
        /// <returns></returns>
        public bool Delete(int idDocument)
        {
            var document = _context.DocumentHistories.Where(a => a.IdDocument.Equals(idDocument));
            if (document != null)
            {
                _context.DocumentHistories.RemoveRange(document);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Find the related document and update the output of history
        /// </summary>
        /// <param name="updateHistoryDto"></param>
        /// <returns></returns>
        public bool UpdateHistory(UpdateHistoryDto updateHistoryDto)
        {
            var document = _context.DocumentHistories.Where(a => a.IdDocument.Equals(updateHistoryDto.IdDocument) &&
                                                            a.Output == updateHistoryDto.OldOutput)
                                                     .AsNoTracking()
                                                     .FirstOrDefault();
            if (document != null)
            {
                document.UpdateOutput(updateHistoryDto.UpdatedOutput);
                _context.DocumentHistories.Update(document);
                _context.SaveChanges();
                return true;
            }
            return false;

        }
    }
}
