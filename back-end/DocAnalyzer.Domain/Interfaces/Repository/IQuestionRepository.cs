using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Interfaces.Repository
{
    public interface IQuestionRepository
    {
        bool CreateUniqueQuestion(Question question);
        ICollection<QuestionDto> FindAll();
        QuestionDto FindByDescriptionAndEmail(string desc,
                                              string email);
        bool DeleteByIds(List<int> ids);
        bool Update(QuestionUpdateDto updateQuestionDto);
        QuestionDto FindById(int id);
        List<Question> FindByIds(List<int> ids);
        IQueryable<QuestionDto> FindAllPaged(QuestionPagedDataDto questionPagedDataDto);
    }
}
