using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;
using DocAnalyzer.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Interfaces.Services
{
    public interface ITypeDocServices
    {
        bool CreateUniqueTypeDoc(TypeDocCreateDto typeDocCreateDto,
                                 HeadersDto typeDocHeaderDto);
        ICollection<TypeDoc> FindAll();
        TypeDoc FindByName(string name);
        bool DeleteByIds(List<int> ids);
        bool Update(TypeDocUpdateDto updateTypeDocDto);
        TypeDocPagedResultDto FindAllPaged(TypeDocPagedDataDto typeDocPagedDataDto);
    }
}
