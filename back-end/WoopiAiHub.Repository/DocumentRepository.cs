using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Application.Dto;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using System.Linq.Dynamic.Core;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Repository.Util;
namespace WoopiAiHub.Repository
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly Context.ApplicationDbContext _context;

        public DocumentRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Search the database for all documents and filter by page and
        /// search text ( if the user searched for a document)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<Document> FindAllOrdered(DocumentPagedDataDto documentPagedDataDto, 
                                                   string email)
        {
            var search = documentPagedDataDto.Search?.ToLower();

            var query = _context.Documents
                                .Include(t => t.Teams)
                                .AsNoTracking()
                                .Where(i => i.Enable);

            if (documentPagedDataDto.TeamIds != null &&
                documentPagedDataDto.TeamIds.Any())
            {
                query = query.Where(d => d.Teams.Any(t => documentPagedDataDto.TeamIds.Contains(t.Id)));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                             EF.Functions.Like(i.Name, $"%{search}%") ||
                             i.Id.ToString().Contains(search) ||
                             i.Teams.Any(t => EF.Functions.Like(t.Name, $"%{search}%")));
            }

            query = documentPagedDataDto.IsAscending ? 
                    query.OrderByDynamic(documentPagedDataDto.ColType.ToString()) : 
                    query.OrderByDynamic(documentPagedDataDto.ColType + " descending");

            return query;
        }

        /// <summary>
        /// Search the database for document list and count
        /// </summary>
        /// <returns></returns>
        public int FindDocumentCount()
        {
            return _context.Documents.Where(a => a.Enable.Equals(true)).Count();
        }

        /// <summary>
        /// Search the database for an document by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Document FindById(int id)
        {
            return _context.Documents.Where(a => a.Id.Equals(id))
                                     .AsNoTracking()
                                     .FirstOrDefault();
        }

        /// <summary>
        /// Search the database for an document by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<string> FindHashById(List<int> ids)
        {
            return _context.Documents.Where(a => ids.Contains(a.Id) && a.Enable.Equals(true))
                                     .Select(b => b.ReferenceFile);
        }

        /// <summary>
        /// Create an document in the database
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public bool Create(Document document)
        {
            _context.Add(document);
            _context.SaveChanges();

            return true;
        }

        /// <summary>
        /// Search the database for an document by id and change the enable
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(List<int> ids)
        {
            var documents = _context.Documents.Where(a => ids.Contains(a.Id) && a.Enable.Equals(true));

            if (documents.Any())
            {
                documents.ExecuteUpdate(b => b
                .SetProperty(u => u.Enable, false));
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Search the database for an document by id and change the status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ChangeStatus(int id,
                                 DocumentStatus status)
        {
            var documents = _context.Documents.Where(a => a.Id.Equals(id));
            if (documents.Any())
            {
                documents.ExecuteUpdate(b => b
                .SetProperty(u => u.Status, status));
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Search the database for an document history by id and 
        /// return in JsonResult format
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult FindInqueryHistory(int id)
        {
            var inqueryHistory = _context.DocumentHistories.Where(a => a.Id.Equals(id))
                                                           .AsNoTracking()
                                                           .FirstOrDefault();

            return new JsonResult(inqueryHistory);
        }
    }
}
