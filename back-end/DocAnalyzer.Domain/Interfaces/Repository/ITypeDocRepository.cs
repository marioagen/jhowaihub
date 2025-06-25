using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.Models;

namespace DocAnalyzer.Domain.Interfaces.Repository
{
    public interface ITypeDocRepository
    {
        bool CreateUniqueTypeDoc(TypeDoc typeDoc);
        ICollection<TypeDoc> FindAll();
        TypeDoc FindByName(string name);
        public bool DeleteByIds(List<int> ids);
        bool Update(TypeDocUpdateDto updateTypeDocDto);
        IQueryable<TypeDocDto> FindAllPaged(TypeDocPagedDataDto typedocPagedDataDto);
    }
}
