using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface ITypeDocRepository
    {
        ResponseCreateTypeDto CreateUniqueTypeDoc(TypeDoc typeDoc);
        ICollection<TypeDoc> FindAll();
        TypeDoc FindByName(string name);
        public bool DeleteByIds(List<int> ids);
        bool Update(TypeDocUpdateDto updateTypeDocDto);
        IQueryable<TypeDocDto> FindAllPaged(TypeDocPagedDataDto typedocPagedDataDto);
    }
}
