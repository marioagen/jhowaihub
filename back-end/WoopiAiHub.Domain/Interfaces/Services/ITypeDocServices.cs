using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ITypeDocServices
    {
        ResponseCreateTypeDto CreateUniqueTypeDoc(TypeDocCreateDto typeDocCreateDto,
                                 HeadersDto typeDocHeaderDto);
        ICollection<TypeDoc> FindAll();
        TypeDoc FindByName(string name);
        bool DeleteByIds(List<int> ids);
        bool Update(TypeDocUpdateDto updateTypeDocDto);
        TypeDocPagedResultDto FindAllPaged(TypeDocPagedDataDto typeDocPagedDataDto);
    }
}
