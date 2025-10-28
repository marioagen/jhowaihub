using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;
using Xunit;

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
                true, 
                null
            );
            card.Document = new Document(
                "Doc", 
                "Ref", 
                "Link", 
                Domain.Enum.DocumentStatus.ReadyForAnalysis, 
                true,
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
    }

    [CollectionDefinition(nameof(CardCollection))]
    public class CardCollection : ICollectionFixture<CardFixture>
    {
    }
}
