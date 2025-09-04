using Bogus;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Models;
using System.Net;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class QuestionnaireFixture
    {
        public CreateQuestionnaireDto FindValidCreateQuestionnaireDto()
        {
            var createQuestionnaireDto = new Faker<CreateQuestionnaireDto>("pt_BR")
            .RuleFor(a => a.Title, f => f.Person.FirstName)
            .RuleFor(a => a.TypeDocId, f => f.IndexFaker)
            .RuleFor(a => a.QuestionsId, f => new List<int>());

            return createQuestionnaireDto;
        }

        public CreateQuestionnaireDto FindInvalidCreateQuestionnaireDto()
        {
            List<int> ids = new List<int>();
            ids.Add(0);
            var createQuestionnaireDto = new Faker<CreateQuestionnaireDto>("pt_BR")
            .RuleFor(a => a.Title, f => f.Person.FirstName)
            .RuleFor(a => a.TypeDocId, f => f.IndexFaker)
            .RuleFor(a => a.QuestionsId, ids);

            return createQuestionnaireDto;
        }

        public static UpdateQuestionnaireDto FindValidUpdateQuestionnaireDto(int questionId)
        {
            var updateQuestionnaireDto = new Faker<UpdateQuestionnaireDto>("pt_BR")
            .RuleFor(a => a.Title, f => f.Person.FirstName)
            .RuleFor(a => a.TypeDocId, f => f.IndexFaker)
            .RuleFor(a => a.QuestionsId, f => new List<int> { questionId })
            .RuleFor(a => a.Id, f => 1);

            return updateQuestionnaireDto;
        }

        public ICollection<QuestionnaireDto> FindValidQuestionnaireDtoList()
        {
            var questionnaireDto = new Faker<QuestionnaireDto>("pt_BR")
            .RuleFor(a => a.Id, 2)
            .RuleFor(a => a.Title, "Title")
            .RuleFor(a => a.TypeDocId, f => f.IndexFaker)
            .RuleFor(a => a.EmailCreator, f => f.Person.Email)
            .RuleFor(a => a.TypeDoc, f => new TypeDoc("test", "email", 1, DateTime.Now))
            .RuleFor(a => a.TypeDocName, f => f.Person.FirstName)
            .RuleFor(a => a.Questions, f => new Question[] { new Question("test", "email", 1, DateTime.Now) })
            .RuleFor(a => a.Created, f => f.Date.Past());

            return questionnaireDto.Generate(1);
        }

        public QuestionnaireDto FindValidQuestionnaireDto()
        {
            var questionnaireDto = new Faker<QuestionnaireDto>("pt_BR")
            .RuleFor(a => a.Id, f => f.IndexFaker)
            .RuleFor(a => a.Title, f => f.Person.FirstName)
            .RuleFor(a => a.TypeDocId, f => f.IndexFaker)
            .RuleFor(a => a.EmailCreator, f => f.Person.Email)
            .RuleFor(a => a.TypeDoc, f => new TypeDoc("test", "email", 1, DateTime.Now))
            .RuleFor(a => a.TypeDocName, f => f.Person.FirstName)
            .RuleFor(a => a.Questions, f => new Question[] { new Question("test", "email", 1, DateTime.Now) })
            .RuleFor(a => a.Created, f => f.Date.Past());

            return questionnaireDto;
        }

        public static QuestionnairePagedDataDto FindValidQuestionnairePagedData(ColTypeQuestionnaire colTypeQuestionnaire, int pageSize)
        {
            var questionnairePagedDataDto = new Faker<QuestionnairePagedDataDto>("pt_BR")
            .RuleFor(a => a.Page,f => 1)
            .RuleFor(a => a.PageSize, pageSize)
            .RuleFor(a => a.Search, f => f.Person.FirstName)
            .RuleFor(a => a.IsAscending, true)
            .RuleFor(a => a.ColType, f => colTypeQuestionnaire);

            return questionnairePagedDataDto;
        }

        public QuestionnairePagedDataDto FindInvalidQuestionnairePagedData()
        {
            var questionnairePagedDataDto = new Faker<QuestionnairePagedDataDto>("pt_BR")
            .RuleFor(a => a.Page, f => f.IndexFaker)
            .RuleFor(a => a.PageSize, f => f.IndexFaker)
            .RuleFor(a => a.Search, f => f.Person.FirstName)
            .RuleFor(a => a.IsAscending, true)
            .RuleFor(a => a.ColType, f => f.PickRandom<ColTypeQuestionnaire>());

            return questionnairePagedDataDto;
        }

        public static QuestionDto FindInvalidQuestionDto()
        {
            var questionDto = new Faker<QuestionDto>("pt_BR")
            .RuleFor(a => a.Description, f => f.Person.FirstName)
            .RuleFor(a => a.EmailCreator, f => f.Person.Email)
            .RuleFor(a => a.Id, f => f.IndexFaker)
            .RuleFor(a => a.Created, f => f.Date.Past());

            return questionDto;
        }

        public static HeadersDto FindValidHeadersDto()
        {
            var questionnaireHeaderDto = new Faker<HeadersDto>("pt_BR")
            .RuleFor(a => a.EmailCreator, f => f.Person.Email);

            return questionnaireHeaderDto;
        }
    }

    [CollectionDefinition(nameof(QuestionnaireCollection))]
    public class QuestionnaireCollection : ICollectionFixture<QuestionnaireFixture>
    {
    }
}
