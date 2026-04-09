using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IQuestionServices
    {
        bool CreateUniqueQuestion(QuestionCreateDto questionCreateDto,
                                  HeadersDto headersDto);
        ICollection<QuestionDto> FindAll();
        QuestionDto? FindByDescriptionAndEmail(string desc,
                                              string emailCreator);
        bool DeleteByIds(List<int> ids);
        bool Update(QuestionUpdateDto updatequestionDto);
        QuestionDto? FindById(int id);
        QuestionPagedResultDto FindAllPaged(QuestionPagedDataDto questionPagedDataDto);
    }
}
