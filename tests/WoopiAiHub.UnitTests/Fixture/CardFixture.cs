using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;
using Xunit;
using Bogus;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class CardFixture
    {
        public static UpdateCardStepStatusDto FindValidUpdateCardStepStatusDto() {             
            return new UpdateCardStepStatusDto
            {
                CardId = 1,
                NextStepOrder = 1,
                WorkflowId = 1
            };
        }

        public static UpdateAssignedUserDto FindValidUpdateAssignedUserDto()
        {
            return new UpdateAssignedUserDto
            {
                CardId = 1,
                UserId = Guid.NewGuid(),
            };
        }

        public static Card FindValidCard()
        {
            var card = new Card(
                1, 
                DateTime.Now, 
                1, 
                1, 
                "Card", 
                1, 
                null
            );
            card.Document = new Document(
                "Doc", 
                "Ref", 
                "Link", 
                Domain.Enum.DocumentStatus.ReadyForAnalysis, 
                "email",
                1,
                new List<Workflow>(),
                DateTime.Now
               );
            return card;
        }

        public static Step FindValidStep()
        {
            return new Step(1, DateTime.Now, 1, "Step", 1, 1, 1);
        }

        public static Status FindValidStatus()
        {
            return new Status("Status", "#FFFFFF", 1, DateTime.Now);
        }

        public static UpdateCardStatusDto FindValidCardStatusDto()
        {
            return new UpdateCardStatusDto
            {
                CardId = 1,
                StatusId =1
            };
        }

        public static CreateDocumentAnalysisRejectionDto FindValidCreateDocumentAnalysisRejectionDto()
        {
            var faker = new Faker("pt_BR");
            return new CreateDocumentAnalysisRejectionDto(
                Justification: faker.Lorem.Paragraph(),
                CardId: 1,
                StepId: 1
            );
        }

        public static DocumentAnalysisRejectionDto FindValidDocumentAnalysisRejectionDto()
        {
            var faker = new Faker("pt_BR");
            return new DocumentAnalysisRejectionDto
            {
                Id = faker.IndexFaker,
                Justification = faker.Lorem.Paragraph(),
                CardId = 1,
                StepId = 1,
                UserId = Guid.NewGuid(),
                UserName = faker.Person.FirstName,
                Date = faker.Date.Past()
            };
        }

        public static List<DocumentAnalysisRejectionDto> FindValidDocumentAnalysisRejectionDtoList()
        {
            var faker = new Faker<DocumentAnalysisRejectionDto>("pt_BR")
                .RuleFor(x => x.Id, f => f.IndexFaker)
                .RuleFor(x => x.Justification, f => f.Lorem.Paragraph())
                .RuleFor(x => x.CardId, 1)
                .RuleFor(x => x.StepId, 1)
                .RuleFor(x => x.UserId, f => Guid.NewGuid())
                .RuleFor(x => x.UserName, f => f.Person.FirstName)
                .RuleFor(x => x.Date, f => f.Date.Past());

            return faker.Generate(3);
        }

        public static DocumentAnalysisRejection FindValidDocumentAnalysisRejection()
        {
            var faker = new Faker("pt_BR");
            var userId = Guid.NewGuid();
            return new DocumentAnalysisRejection(
                id: faker.IndexFaker,
                created: faker.Date.Past(),
                justification: faker.Lorem.Paragraph(),
                cardId: 1,
                stepId: 1,
                userId: userId
            );
        }
    }

    [CollectionDefinition(nameof(CardCollection))]
    public class CardCollection : ICollectionFixture<CardFixture>
    {
    }
}
