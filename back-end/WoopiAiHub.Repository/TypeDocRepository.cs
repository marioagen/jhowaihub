using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace WoopiAiHub.Repository
{
    public class TypeDocRepository : ITypeDocRepository

    {
        private readonly Context.ApplicationDbContext _context;
        public TypeDocRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a document type in the database
        /// </summary>
        /// <param name="typeDoc"></param>
        /// <returns></returns>
        public ResponseCreateTypeDto CreateUniqueTypeDoc(TypeDoc typeDoc)
        {
            var existTypeDoc = _context.TypeDoc.Any(p => p.Name == typeDoc.Name);
            if (!existTypeDoc)
            {
                _context.TypeDoc.Add(typeDoc);
                _context.SaveChanges();

                return new ResponseCreateTypeDto
                {
                    Id = typeDoc.Id,
                    Name = typeDoc.Name,
                    Created = typeDoc.Created,
                    EmailCreator = typeDoc.EmailCreator,
                    Duplicated = false
                };
            }
            return new ResponseCreateTypeDto
                {
                    Id = typeDoc.Id,
                    Name = typeDoc.Name,
                    Created = typeDoc.Created,
                    EmailCreator = typeDoc.EmailCreator,
                    Duplicated = true
                };
        }

        /// <summary>
        /// Find all documents type in the database
        /// </summary>
        /// <returns></returns>
        public ICollection<TypeDoc> FindAll()
        {
            return _context.TypeDoc
                           .AsNoTracking()
                           .ToList();
        }

        /// <summary>
        /// Find a document type by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TypeDoc FindByName(string name)
        {
            return _context.TypeDoc.Where(a => a.Name.Equals(name))
                                     .FirstOrDefault();
        }

        /// <summary>
        /// Delete documents types by id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            var types = _context.TypeDoc.Where(a => ids.Contains(a.Id));

            if (types.Count() > 0)
            {
                _context.TypeDoc.RemoveRange(types);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Update a document type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Update(TypeDocUpdateDto updateTypeDocDto)
        {
            var existTypeDoc = _context.TypeDoc.Any(p => p.Name == updateTypeDocDto.Name && p.Id != updateTypeDocDto.Id);
            if (!existTypeDoc)
            {
                _context.TypeDoc.Where(a => a.Id.Equals(updateTypeDocDto.Id))
                                .ExecuteUpdate(b => b
                                .SetProperty(u => u.Name, updateTypeDocDto.Name));

                _context.SaveChanges();

                return true;
            }
            return false;
        }

        /// <summary>
        ///  Get all documents type in the database paged
        /// </summary>
        /// <param name="typedocPagedDataDto"></param>
        /// <returns></returns>

        public IQueryable<TypeDocDto> FindAllPaged(TypeDocPagedDataDto typedocPagedDataDto)
        {
            var query = _context.TypeDoc
                .Select(q => new TypeDocDto
                {
                    Id = q.Id,
                    Name = q.Name,
                    Created = q.Created,
                    EmailCreator = q.EmailCreator
                })
                .AsQueryable()
                .AsNoTracking();

            return query;
        }
    }
}
