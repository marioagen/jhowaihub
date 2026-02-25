using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace WoopiAiHub.Application.Services
{
    public class TypeDocServices : ITypeDocServices
    {
        private readonly ITypeDocRepository _typeDocRepository;

        public TypeDocServices(ITypeDocRepository typeDocRepository)
        {
            this._typeDocRepository = typeDocRepository;
        }

        /// <summary>
        /// Create a new type of document
        /// </summary>
        /// <param name="typeDocCreateDto"></param>
        /// <param name="typeDocHeaderDto"></param>
        /// <returns></returns>
        public ResponseCreateTypeDto CreateUniqueTypeDoc(
            TypeDocCreateDto typeDocCreateDto,
            HeadersDto typeDocHeaderDto
        )
        {
            TypeDoc typedoc = new TypeDoc
            (
                typeDocCreateDto.Name,
                typeDocHeaderDto.EmailCreator,
                0,
                DateTime.Now
            );

            var typeDocResult = _typeDocRepository.CreateUniqueTypeDoc(typedoc);

            if (typeDocResult.Duplicated)
            {
                throw new ArgumentException("Duplicated TypeDoc");
            }

            return typeDocResult;
        }

        /// <summary>
        /// Find all types of documents
        /// </summary>
        /// <returns></returns>
        public ICollection<TypeDoc> FindAll()
        {
            return _typeDocRepository.FindAll();
        }

        /// <summary>
        /// Find a type of document by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TypeDoc FindByName(string name)
        {
            return _typeDocRepository.FindByName(name);
        }

        /// <summary>
        /// Delete a list of type of document by id
        /// </summary>
        /// <param name="deleteTypeDocDto"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            return _typeDocRepository.DeleteByIds(ids);
        }

        /// <summary>
        /// Update a type of document
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Update(TypeDocUpdateDto updateTypeDocDto)
        {
            var typeDocResult = _typeDocRepository.Update(updateTypeDocDto);

            if (!typeDocResult)
            {
                throw new ArgumentException("Duplicated TypeDoc");
            }

            return typeDocResult;
        }

        /// <summary>
        /// Get all TypeDoc paged
        /// </summary>
        /// <param name="typeDocPagedDataDto"></param>
        /// <returns></returns>
        public TypeDocPagedResultDto FindAllPaged(TypeDocPagedDataDto typeDocPagedDataDto)
        {
            if (typeDocPagedDataDto.Page > 0)
            {
                var totalList = _typeDocRepository.FindAllPaged(typeDocPagedDataDto);

                totalList = typeDocPagedDataDto.IsAscending ?
                totalList.OrderBy(typeDocPagedDataDto.ColType.ToString()) :
                totalList.OrderBy(typeDocPagedDataDto.ColType.ToString() + " descending");

                var result = this.TypeDocPagination(totalList, typeDocPagedDataDto);
                return result;
            }
            else
            {
                var ex = new ArgumentException("The number of pages must be greater than 0");
                throw ex;
            }
        }

        /// <summary>
        /// Ordenates the list of TypeDocs and returns a paged result
        /// </summary>
        /// <param name="totalList"></param>
        /// <param name="typeDocPagedDataDto"></param>
        /// <returns></returns>
        private TypeDocPagedResultDto TypeDocPagination(IQueryable<TypeDocDto> totalList, TypeDocPagedDataDto typeDocPagedDataDto)
        {
            int pageCount, currentPage = 0;

            if (string.IsNullOrEmpty(typeDocPagedDataDto.Search) is false)
            {
                totalList = totalList.Where(i => i.Name.ToLower()
                                     .Contains(typeDocPagedDataDto.Search.ToLower()) ||
                                               i.Id.ToString().Contains(typeDocPagedDataDto.Search));
            }

            var totalListCount = totalList.Count();

            if (typeDocPagedDataDto.PageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
                typeDocPagedDataDto.PageSize = totalListCount;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)totalListCount / typeDocPagedDataDto.PageSize);
                currentPage = typeDocPagedDataDto.Page <= pageCount ? typeDocPagedDataDto.Page : 1;
                totalList = totalList.Skip((currentPage - 1) * typeDocPagedDataDto.PageSize)
                                     .Take(typeDocPagedDataDto.PageSize);
            }

            return new TypeDocPagedResultDto()
            {
                Content = totalList,
                CurrentPage = currentPage,
                PageCount = pageCount,
                RowCount = totalListCount
            };
        }
    }
}
