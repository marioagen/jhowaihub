using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;


namespace WoopiAiHub.Repository
{
    public class DocumentNormalizedRepository : IDocumentNormalizedRepository
    {
        private readonly Context.ApplicationDbContext _context;

        public DocumentNormalizedRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a document history in the database
        /// </summary>
        /// <param name="DocumentNormalized"></param>
        /// <returns></returns>
        public bool Create(DocumentNormalized documentNormalized)
        {
            _context.Add(documentNormalized);
            _context.SaveChanges();

            return true;
        }

        /// <summary>
        /// update a normalized document in the database
        /// </summary>
        /// <param name="DocumentNormalized"></param>
        /// <returns></returns>
        public bool Update(DocumentNormalized documentNormalized)
        {
            _context.DocumentNormalized.Update(documentNormalized);
            _context.SaveChanges();
            return true;

        }

        /// <summary>
        /// Find the list of DocumentNormalized by the id of the document
        /// </summary>
        /// <param name="idDocument"></param>
        /// <returns></returns>
        public DocumentNormalized FindById(int idDocument)
        {
            return _context.DocumentNormalized.Where(a => a.IdDocument.Equals(idDocument))
                                       .AsNoTracking()
                                       .FirstOrDefault();
        }

        /// <summary>
        /// Find the count of DocumentNormalized
        /// </summary>
        /// <returns></returns>
        public int FindDocumentNormalizedCount()
        {
            return _context.DocumentNormalized.Count();
        }
    }
}
