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
