using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;
using DocAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Interfaces.Repository
{
    public interface IQuestionnaireRepository 
    {
        bool CreateUniqueQuestionnaire(Questionnaire questionnaire);
        ICollection<QuestionnaireDto> FindAll();
        QuestionnaireDto FindById(int id);
        bool DeleteByIds(List<int> ids);
        bool DeleteById(int id);
        List<int> FindByQuestionIds(List<int> ids);
        bool Update(Questionnaire questionnaire);
        IQueryable<QuestionnaireDto> FindAllPaged(QuestionnairePagedDataDto questionnairePagedDataDto);
        List<Questionnaire> FindByIds(List<int> ids);
    }
}
