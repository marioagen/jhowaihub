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

        public static CardAnalysisDto FindValidCardAnalysisDto(int cardId = 1, int stepId = 1, int documentId = 1)
        {
            var document = DocumentFixture.FindValidDocument();
            
            return new CardAnalysisDto
            {
                Id = cardId,
                Created = DateTime.Now,
                StepId = stepId,
                DocumentId = documentId,
                Name = "Card Test",
                StatusId = 1,
                Document = new DocumentDto
                {
                    Id = documentId,
                    Name = document.Name,
                    Description = document.Description,
                    ReferenceFile = document.ReferenceFile
                },
                Step = new StepDto
                {
                    Id = stepId,
                    Name = "Step Test",
                    Order = 1,
                    WorkflowId = 1
                },
                Outputs = []
            };
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithOutput(int cardId = 1, int stepId = 1, int stepToolId = 1, string outputValue = "{\"Campo1\": \"Valor1\", \"Campo2\": \"Valor2\"}", string toolTypeName = "Prompt", int toolTypeId = 2, int toolId = 1)
        {
            var cardAnalysisDto = FindValidCardAnalysisDto(cardId, stepId);

            cardAnalysisDto.Outputs =
            [
                new StepToolOutputAnalysesDto
                {
                    Id = 1,
                    StepToolId = stepToolId,
                    Value = outputValue,
                    StepTool = new StepToolDto
                    {
                        Id = stepToolId,
                        StepId = stepId,
                        ToolId = toolId,
                        Tool = new ToolDto
                        {
                            Id = toolId,
                            Name = "Test Tool",
                            ToolTypeId = toolTypeId,
                            ToolType = toolTypeName
                        }
                    }
                }
            ];

            return cardAnalysisDto;
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithMultipleOutputs(int cardId = 1)
        {
            var document = DocumentFixture.FindValidDocument();
            
            return new CardAnalysisDto
            {
                Id = cardId,
                Created = DateTime.Now,
                StepId = 2,
                DocumentId = document.Id,
                Name = "Card Test",
                StatusId = 1,
                Document = new DocumentDto
                {
                    Id = document.Id,
                    Name = document.Name,
                    Description = document.Description,
                    ReferenceFile = document.ReferenceFile
                },
                Step = new StepDto
                {
                    Id = 1,
                    Name = "Step Test",
                    Order = 1,
                    WorkflowId = 1
                },
                Outputs =
                [
                    new StepToolOutputAnalysesDto
                    {
                        Id = 1,
                        StepToolId = 1,
                        Value = "{\"Field1\": \"Value1\"}",
                        StepTool = new StepToolDto
                        {
                            Id = 1,
                            StepId = 1,
                            ToolId = 1,
                            Tool = new ToolDto
                            {
                                Id = 1,
                                Name = "Test Tool",
                                ToolTypeId = 2,
                                ToolType = "Prompt"
                            }
                        }
                    },
                    new StepToolOutputAnalysesDto
                    {
                        Id = 2,
                        StepToolId = 2,
                        Value = "{\"Field2\": \"Value2\"}",
                        StepTool = new StepToolDto
                        {
                            Id = 2,
                            StepId = 2,
                            ToolId = 1,
                            Tool = new ToolDto
                            {
                                Id = 1,
                                Name = "Test Tool",
                                ToolTypeId = 2,
                                ToolType = "Prompt"
                            }
                        }
                    }
                ]
            };
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithOCROutput(int cardId = 1)
        {
            return FindCardAnalysisDtoWithOutput(cardId, 1, 1, "{\"text\": \"OCR Result\"}", "OCR", 1, 1);
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithEmbeddingsOutput(int cardId = 1)
        {
            return FindCardAnalysisDtoWithOutput(cardId, 1, 1, "{\"embedding\": \"[0.1, 0.2, 0.3]\"}", "Embeddings", 3, 1);
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithPlainTextOutput(int cardId = 1)
        {
            return FindCardAnalysisDtoWithOutput(cardId, 1, 1, "This is a plain text response without JSON structure", "Prompt", 2, 1);
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithInvalidJsonOutput(int cardId = 1)
        {
            return FindCardAnalysisDtoWithOutput(cardId, 1, 1, "{\"field\": \"value\", invalid json", "Prompt", 2, 1);
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithJsonOutput(int cardId = 1)
        {
            return FindCardAnalysisDtoWithOutput(cardId, 1, 1, "{\"Nome\": \"João Silva\", \"Email\": \"joao@example.com\"}", "Prompt", 2, 1);
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
