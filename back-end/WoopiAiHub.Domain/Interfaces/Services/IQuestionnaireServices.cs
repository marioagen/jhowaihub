using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IQuestionnaireServices
    {
        bool CreateUniqueQuestionnaire(CreateQuestionnaireDto createQuestionnaireDto,
                                       string email);
        ICollection<QuestionnaireDto> FindAll();
        QuestionnaireDto? FindById(int id);
        bool DeleteByIds(List<int> ids);
        bool Update(UpdateQuestionnaireDto updateQuestionnaireDto);
        QuestionnairePagedResultDto FindAllPaged(QuestionnairePagedDataDto questionnairePagedDataDto);
    }
}
