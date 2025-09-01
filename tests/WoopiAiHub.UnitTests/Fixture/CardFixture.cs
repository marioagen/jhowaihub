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

        public static Card FindValidCard()
        {
            return new Card(1, DateTime.Now, 1, 1, "Card", 1, true);
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
