using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
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

        public static Step FindValidStepWithWorkflow()
        {
            return new Step(1, DateTime.UtcNow, 1, "Step", 1, 1, 1)
            {
                Workflow = WorkflowFixture.FindValidWorkflow()
            };
        }

        public static Card FindCard(int id, int documentId, string name, int? documentBatchId = null, Guid? assignedUserId = null)
        {
            return new Card(id, DateTime.UtcNow, 1, documentId, name, 1, assignedUserId, documentBatchId);
        }

        public static Card FindCardWithWorkflowStep(int id, int documentId, string name, int? documentBatchId = null, Guid? assignedUserId = null)
        {
            var card = FindCard(id, documentId, name, documentBatchId, assignedUserId);
            card.Step = FindValidStepWithWorkflow();
            return card;
        }

        public static List<Card> FindDocumentBatchCardsWithAssignedUsers(int documentBatchId = 100)
        {
            return
            [
                FindCardWithWorkflowStep(1, 1, "Card Name", documentBatchId, Guid.NewGuid()),
                FindCardWithWorkflowStep(2, 2, "Card 2", documentBatchId, Guid.NewGuid()),
                FindCardWithWorkflowStep(3, 3, "Card 3", documentBatchId, Guid.NewGuid())
            ];
        }

        public static List<Card> FindDocumentBatchCardsWithoutAssignedUser(int documentBatchId = 100)
        {
            return
            [
                FindCardWithWorkflowStep(1, 1, "Card Name", documentBatchId),
                FindCardWithWorkflowStep(2, 2, "Card 2", documentBatchId),
                FindCardWithWorkflowStep(3, 3, "Card 3", documentBatchId)
            ];
        }

        public static Card FindSecondaryCardSharingDocumentAndStep(Card primary, int cardId, string name)
        {
            return new Card(cardId, DateTime.UtcNow, 1, 1, name, 1, null)
            {
                Document = primary.Document,
                Step = primary.Step
            };
        }

        public static Step FindWorkflowNextStep(int id = 2, string name = "Next", int order = 2, int workflowId = 1, int statusId = 2)
        {
            return new Step(id, DateTime.UtcNow, workflowId, name, order, 1, statusId);
        }

        public static CardHeaderDto FindValidCardHeaderDto(string cardName = "Test Card", string workflowName = "Test Workflow")
        {
            return new CardHeaderDto
            {
                CardName = cardName,
                WorkflowName = workflowName,
                WorkflowId = 1
            };
        }

        public static CardHeaderDto FindValidCardHeaderDtoWithBatchId(int documentBatchId = 50)
        {
            return new CardHeaderDto
            {
                CardName = "Test Card",
                WorkflowName = "Test Workflow",
                WorkflowId = 1,
                DocumentBatchId = documentBatchId
            };
        }

        public static Card FindCardWithStepForWorkflow(int id, int documentId, string name, int workflowId, int? documentBatchId = null)
        {
            var card = FindCard(id, documentId, name, documentBatchId);
            card.Step = new Step(1, DateTime.UtcNow, workflowId, "Step", 1, 1, 1);
            return card;
        }

        public static List<Card> FindBatchCardsForWorkflow(int documentBatchId, int workflowId, int count = 3)
        {
            return Enumerable.Range(1, count)
                .Select(i => FindCardWithStepForWorkflow(i, i * 10, $"Doc {i}", workflowId, documentBatchId))
                .ToList();
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

        public static CardAnalysisDto FindCardAnalysisDtoWithPromptOutputsUsingPromptParameter(
            int cardId = 1,
            int promptId = 99)
        {
            var cardAnalysisDto = FindValidCardAnalysisDto(cardId);
            cardAnalysisDto.Outputs =
            [
                new StepToolOutputAnalysesDto
                {
                    Id = 1,
                    StepToolId = 1,
                    Value = "v1",
                    StepTool = new StepToolDto
                    {
                        Id = 1,
                        StepId = 1,
                        ToolId = 1,
                        Parameters = [new StepToolParameterDto { Value = promptId.ToString() }],
                        Tool = new ToolDto
                        {
                            Id = 1,
                            Name = "FallbackTool",
                            ToolTypeId = 2,
                            ToolType = HandlersTypes.Prompt
                        }
                    }
                },
                new StepToolOutputAnalysesDto
                {
                    Id = 2,
                    StepToolId = 1,
                    Value = "v2",
                    StepTool = new StepToolDto
                    {
                        Id = 1,
                        StepId = 1,
                        ToolId = 1,
                        Parameters = [new StepToolParameterDto { Value = promptId.ToString() }],
                        Tool = new ToolDto
                        {
                            Id = 1,
                            Name = "FallbackTool",
                            ToolTypeId = 2,
                            ToolType = HandlersTypes.Prompt
                        }
                    }
                }
            ];

            return cardAnalysisDto;
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithQuizOutputUsingQuestionnaireParameter(
            int cardId = 1,
            int questionnaireId = 7,
            string toolName = "QuizTool")
        {
            var cardAnalysisDto = FindValidCardAnalysisDto(cardId);
            cardAnalysisDto.Outputs =
            [
                new StepToolOutputAnalysesDto
                {
                    Id = 1,
                    StepToolId = 1,
                    Value = "answer",
                    StepTool = new StepToolDto
                    {
                        Id = 1,
                        StepId = 1,
                        ToolId = 1,
                        Parameters = [new StepToolParameterDto { Value = questionnaireId.ToString() }],
                        Tool = new ToolDto
                        {
                            Id = 1,
                            Name = toolName,
                            ToolTypeId = 5,
                            ToolType = HandlersTypes.Quiz
                        }
                    }
                }
            ];

            return cardAnalysisDto;
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithApiToolOutput(
            int cardId = 1,
            string toolName = "Test Tool",
            int apiTemplateId = 7)
        {
            var cardAnalysisDto = FindCardAnalysisDtoWithPlainTextOutput(cardId);
            var output = cardAnalysisDto.Outputs?.FirstOrDefault();
            if (output?.StepTool?.Tool != null)
            {
                output.StepTool.Tool.ToolType = HandlersTypes.API;
                output.StepTool.Tool.Name = toolName;
                output.StepTool.Parameters =
                [
                    new StepToolParameterDto
                    {
                        Value = apiTemplateId.ToString()
                    }
                ];
            }

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

        public static CardAnalysisDto FindCardAnalysisDtoWithNullStep(int cardId = 1)
        {
            var dto = FindValidCardAnalysisDto(cardId);
            dto.Step = null;
            return dto;
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithEmptyJsonObjectOutput(int cardId = 1)
        {
            return FindCardAnalysisDtoWithOutput(cardId, 1, 1, "{}", "Prompt", 2, 1);
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithWhitespaceValueOutput(int cardId = 1)
        {
            return FindCardAnalysisDtoWithOutput(cardId, 1, 1, "   \t  ", "Prompt", 2, 1);
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithJsonThatThrowsOnParse(int cardId = 1)
        {
            return FindCardAnalysisDtoWithOutput(cardId, 1, 1, "{\"a\":}", "Prompt", 2, 1);
        }

        public static CardAnalysisDto FindCardAnalysisDtoWithNullToolOnOutput(int cardId = 1)
        {
            var cardAnalysisDto = FindValidCardAnalysisDto(cardId);
            cardAnalysisDto.Outputs =
            [
                new StepToolOutputAnalysesDto
                {
                    Id = 1,
                    StepToolId = 1,
                    Value = "{\"x\": \"y\"}",
                    StepTool = new StepToolDto
                    {
                        Id = 1,
                        StepId = 1,
                        ToolId = 1,
                        Tool = null
                    }
                }
            ];
            return cardAnalysisDto;
        }

        public static StepToolExecution CreateStepToolExecutionWithToolTypeName(
            int executionId,
            int cardId,
            int stepToolId,
            string toolTypeName,
            StatusExecution status = StatusExecution.Ready)
        {
            var toolType = new ToolType(executionId, DateTime.UtcNow, toolTypeName, string.Empty, true);
            var tool = new Tool(executionId, DateTime.UtcNow, "Tool", true, executionId, 1, 1, false, null, null);
            tool.ToolType = toolType;
            var stepTool = new StepTool(stepToolId, DateTime.UtcNow, 1, executionId, 1, 0, 0);
            stepTool.Tool = tool;
            var execution = new StepToolExecution(executionId, DateTime.UtcNow, stepToolId, status, cardId);
            execution.StepTool = stepTool;
            return execution;
        }

        public static Card FindRejectedCardWithDocument(int rejectedStatusId = 9)
        {
            var card = new Card(1, DateTime.UtcNow, 1, 1, "Card", rejectedStatusId, null);
            card.Status = new Status(StatusNames.Rejected, "#FFFFFF", rejectedStatusId, DateTime.UtcNow);
            card.Document = new Document(
                "Doc",
                "Ref",
                "Link",
                Domain.Enum.DocumentStatus.ReadyForAnalysis,
                "email",
                1,
                new List<Workflow>(),
                DateTime.Now);
            return card;
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

        public static CreateDocumentAnalysisRejectionRangeDto FindValidCreateDocumentAnalysisRejectionRangeDto(
            Guid? userId = null)
        {
            var faker = new Faker("pt_BR");
            return new CreateDocumentAnalysisRejectionRangeDto(
                Justification: faker.Lorem.Paragraph(),
                StepId: 1,
                CardIds: new List<int> { 1, 2 },
                UserId: userId
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
