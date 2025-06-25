using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;
using DocAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Interfaces.Services
{
    public interface IQuestionServices
    {
        bool CreateUniqueQuestion(QuestionCreateDto questionCreateDto,
                                  HeadersDto headersDto);
        ICollection<QuestionDto> FindAll();
        QuestionDto FindByDescriptionAndEmail(string desc,
                                              string emailCreator);
        bool DeleteByIds(List<int> ids);
        bool Update(QuestionUpdateDto updatequestionDto);
        QuestionDto FindById(int id);
        QuestionPagedResultDto FindAllPaged(QuestionPagedDataDto questionPagedDataDto);
    }
}
