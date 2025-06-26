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
    public class QuestionFixture
    {
        public Question FindValidQuestion()
        {
            Question question = new Faker<Question>("pt_BR")
            .CustomInstantiator(f => new Question
            (
                description: f.Lorem.Paragraph(),
                created: f.Date.Past(),
                id: f.IndexFaker,
                emailCreator: f.Person.Email
            ));

            return question;
        }

        public ICollection<QuestionDto> FindValidQuestionListDto()
        {
            var questionDto = new Faker<QuestionDto>("pt_BR")
            .RuleFor(a => a.Id, f => f.IndexFaker)
            .RuleFor(a => a.Description, f => "Title")
            .RuleFor(a => a.EmailCreator, f => f.Person.Email)
            .RuleFor(a => a.Questionnaires, f => new Questionnaire[] { new Questionnaire("test",1,"email",1,DateTime.Now) })
            .RuleFor(a => a.Created, f => f.Date.Past());

            return questionDto.Generate(1);
        }

        public QuestionDto FindValidQuestionDto()
        {
            var questionDto = new Faker<QuestionDto>("pt_BR")
            .RuleFor(a => a.Id, f => f.IndexFaker)
            .RuleFor(a => a.Description, f => f.Lorem.Paragraph())
            .RuleFor(a => a.EmailCreator, f => f.Person.Email)
            .RuleFor(a => a.Questionnaires, f => new Questionnaire[] { new Questionnaire("test", 1, "email", 1, DateTime.Now) })
            .RuleFor(a => a.Created, f => f.Date.Past());

            return questionDto;
        }

        public static QuestionUpdateDto FindValidUpdateQuestionDto()
        {
            var updateValidQuestionDto = new Faker<QuestionUpdateDto>("pt_BR")
            .RuleFor(a => a.Id, f => f.IndexFaker)
            .RuleFor(a => a.Description, f => f.Lorem.Paragraph());

            return updateValidQuestionDto;
        }

        public QuestionPagedDataDto FindValidQuestionPagedDataDto()
        {
            var questionPagedDataDto = new Faker<QuestionPagedDataDto>("pt_BR")
            .RuleFor(a => a.Page, f => 1)
            .RuleFor(a => a.PageSize, f => f.IndexFaker)
            .RuleFor(a => a.Search, String.Empty)
            .RuleFor(a => a.IsAscending, true)
            .RuleFor(a => a.ColType, f => f.PickRandom<ColTypeQuestion>());

            return questionPagedDataDto;
        }

        public QuestionPagedDataDto FindInvalidQuestionPagedDataDto()
        {
            var questionPagedDataDto = new Faker<QuestionPagedDataDto>("pt_BR")
            .RuleFor(a => a.Page, f => 0)
            .RuleFor(a => a.PageSize, f => f.IndexFaker)
            .RuleFor(a => a.Search, String.Empty)
            .RuleFor(a => a.IsAscending, true)
            .RuleFor(a => a.ColType, f => f.PickRandom<ColTypeQuestion>());

            return questionPagedDataDto;
        }

        public QuestionPagedResultDto FindValidQuestionPagedResultDto()
        {
            var questionPagedResultDto = new Faker<QuestionPagedResultDto>("pt_BR")
            .RuleFor(a => a.Content, this.FindValidQuestionListDto())
            .RuleFor(a => a.CurrentPage, f => f.IndexFaker)
            .RuleFor(a => a.PageCount, 1)
            .RuleFor(a => a.RowCount, 1);

            return questionPagedResultDto;
        }

        public List<Questionnaire> FindValidQuestionnaireList()
        {
            List<Questionnaire> questionnaireList = new Faker<Questionnaire>("pt_BR")
            .CustomInstantiator(f => new Questionnaire
            (
                title: "Test",
                typeDocId: 1,
                created: f.Date.Past(),
                id: f.IndexFaker,
                emailCreator: f.Person.Email
            )).Generate(1);

            return questionnaireList;
        }

        public static QuestionCreateDto FindValidQuestionCreateDto()
        {
            var questionCreateDto = new Faker<QuestionCreateDto>("pt_BR")
            .RuleFor(a => a.Description, f => f.Lorem.Paragraph());

            return questionCreateDto;
        }

        public static HeadersDto FindValidHeadersDto()
        {
            var headersDto = new Faker<HeadersDto>("pt_BR")
            .RuleFor(a => a.EmailCreator, f => f.Person.Email);

            return headersDto;
        }
    }

    [CollectionDefinition(nameof(QuestionCollection))]
    public class QuestionCollection : ICollectionFixture<QuestionFixture>
    {
    }
}
