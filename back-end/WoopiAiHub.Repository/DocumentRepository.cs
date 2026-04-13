using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
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
        public IQueryable<DocumentListItemDto> FindAllOrdered(DocumentPagedDataDto documentPagedDataDto,
            string email)
        {
            var search = documentPagedDataDto.Search?.ToLower();
            var login = documentPagedDataDto.Login?.ToLower();
            var query = _context.Documents
                .Include(t => t.Workflows)
                .Where(d => d.Enable)
                .AsNoTracking();
            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                    EF.Functions.Like(i.Name, $"%{search}%") ||
                    EF.Functions.Like(i.Description, $"%{search}%") ||
                    i.Id.ToString().Contains(search));
            }

            if (documentPagedDataDto.WorkflowIds.Count() > 0)
            {
                var workflowIds = documentPagedDataDto.WorkflowIds;

                query = query.Where(d => d.Workflows.Any(w => workflowIds.Contains(w.Id)));
            }

            if (documentPagedDataDto.StatusId.HasValue && documentPagedDataDto.StatusId.Value > 0)
            {
                var statusId = documentPagedDataDto.StatusId.Value;
                const int StatusIdDone = 5;
                if (statusId == StatusIdDone)
                {
                    query = query.Where(d => d.Cards.Any(c =>
                        c.Step != null &&
                        c.Step.Order == _context.Steps
                            .Where(s => s.WorkflowId == c.Step.WorkflowId)
                            .Max(s => s.Order)));
                }
                else
                {
                    query = query.Where(d => d.Cards.Any(c => c.StatusId == statusId));
                }
            }

            if (!documentPagedDataDto.IsAllUsers)
            {
                query = query.Where(d => d.Cards.Any(c =>
                    c.AssignedUser != null &&
                    EF.Functions.Like(c.AssignedUser.Email, login)
                ));
            }

            if(documentPagedDataDto.DocumentType is not null && documentPagedDataDto.DocumentType != DocumentFilter.All)
            {
                query = query.Where(w => documentPagedDataDto.DocumentType == DocumentFilter.Singles ? !w.HasBatch : w.HasBatch);
            }

            query = documentPagedDataDto.IsAscending
                ? query.OrderByDynamic(documentPagedDataDto.ColType.ToString())
                : query.OrderByDynamic(documentPagedDataDto.ColType + " descending");

            return query.Select(d => new DocumentListItemDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                ReferenceFile = d.ReferenceFile,
                Status = d.Status,
                Created = d.Created,
                HasBatch = d.HasBatch,
                WorkflowProgress = d.Workflows
                    .Where(w => w.Enable
                        && w.Steps.Count > 0
                        && (d.Status == DocumentStatus.Analyzed
                            || d.Cards.Any(c => c.Enable && c.Step!.WorkflowId == w.Id)))
                    .Select(w => new DocumentWorkflowProgressDto
                    {
                        WorkflowName = w.Name,
                        TotalSteps = w.Steps.Count(),
                        CurrentStep = d.Status == DocumentStatus.Analyzed
                            ? w.Steps.Count()
                            : d.Cards
                                .Where(c => c.Enable && c.Step!.WorkflowId == w.Id)
                                .OrderByDescending(c => c.Created)
                                .Select(c => c.Step!.Order)
                                .FirstOrDefault()
                    }).ToList()
            });
        }

        /// <summary>
        /// Search the database for document list and count
        /// </summary>
        /// <returns></returns>
        public int FindDocumentCount()
        {
            return _context.Documents.Count();
        }

        /// <summary>
        /// Search the database for an document by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Document? FindById(int id)
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
            return _context.Documents.Where(a => ids.Contains(a.Id))
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
        /// Logically deletes documents by setting Enable to false (soft delete).
        /// Documents are excluded from default queries via the global query filter.
        /// Call card soft-delete (e.g. DeleteByDocumentIds) before this so related cards are disabled.
        /// </summary>
        /// <param name="ids">Ids of documents to logically delete.</param>
        /// <returns>True if any document was updated; otherwise false.</returns>
        public bool Delete(List<int> ids)
        {
            var documents = _context.Documents.Where(a => ids.Contains(a.Id)).ToList();

            if (documents.Count > 0)
            {
                foreach (var document in documents)
                    document.Disable();
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Search the database for an document by id and change the status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ChangeStatus(int id,
            DocumentStatus documentStatus)
        {
            var documents = _context.Documents.Where(a => a.Id.Equals(id));
            if (documents.Any())
            {
                documents.ExecuteUpdate(b => b.SetProperty(u => u.Status, documentStatus));
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

        /// <summary>
        /// Search the database for an document id by referenceFile
        /// returns int
        /// </summary>
        /// <param name="referenceFile"></param>
        /// <returns></returns>
        public int FindDocumentIdByReferenceFile(string referenceFile)
        {
            var documentId = _context.Documents.Where(a => a.ReferenceFile.Equals(referenceFile))
                .Select(a => a.Id)
                .FirstOrDefault();

            return documentId;
        }

        public Document? FindByReferenceFile(string referenceFile)
        {
            return _context.Documents.Where(a => a.ReferenceFile.Equals(referenceFile))
                .AsNoTracking()
                .FirstOrDefault();
        }

        /// <summary>
        /// Clears the relationship between documents and workflows by removing entries from the WorkflowDocuments join table
        /// </summary>
        /// <param name="documentIds"></param>
        /// <returns></returns>
        public bool ClearWorkflowRelationships(List<int> documentIds)
        {
            if (documentIds == null || documentIds.Count == 0)
            {
                return false;
            }

            var WorkflowDocuments = _context.Set<Dictionary<string, object>>("WorkflowDocuments");
            var relationships = WorkflowDocuments
                .Where(workflowDocuments => documentIds.Contains((int)workflowDocuments["DocumentId"]))
                .ToList();

            if (relationships.Count > 0)
            {
                WorkflowDocuments.RemoveRange(relationships);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns IDs of enabled documents that are linked exclusively to the specified workflow
        /// (i.e. have exactly one entry in WorkflowDocuments, pointing to <paramref name="workflowId"/>).
        /// When <paramref name="candidateDocumentIds"/> is provided, only those documents are evaluated.
        /// </summary>
        public async Task<List<int>> FindOrphanDocumentIdsByWorkflowAsync(int workflowId, List<int>? candidateDocumentIds = null)
        {
            return await _context.Documents
                .Where(d =>
                    d.Workflows.Count == 1 &&
                    d.Workflows.Any(w => w.Id == workflowId) &&
                    (candidateDocumentIds == null || candidateDocumentIds.Contains(d.Id)))
                .Select(d => d.Id)
                .ToListAsync();
        }
    }
}
