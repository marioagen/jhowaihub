using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;

namespace DocAnalyzer.Domain.Interfaces.Services
{
    public interface IQuestionnaireServices
    {
        bool CreateUniqueQuestionnaire(CreateQuestionnaireDto createQuestionnaireDto,
                                       string email);
        ICollection<QuestionnaireDto> FindAll();
        QuestionnaireDto FindById(int id);
        bool DeleteByIds(List<int> ids);
        bool Update(UpdateQuestionnaireDto updateQuestionnaireDto);
        QuestionnairePagedResultDto FindAllPaged(QuestionnairePagedDataDto questionnairePagedDataDto);
    }
}
