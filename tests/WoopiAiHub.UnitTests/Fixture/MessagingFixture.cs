using Bogus;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using Xunit;
using Newtonsoft.Json.Linq;
using WoopiAiHub.Domain.DTOs.Response.OpenAiResponses;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class MessagingFixture
    {
        public static TenantSubscriptionDto FindValidTenantSubscriptionDto()
        {
            var faker = new Faker<TenantSubscriptionDto>("pt_BR")
                .CustomInstantiator(f => new TenantSubscriptionDto
                {
                    Name = f.Random.String(),
                    Email = f.Random.String(),
                    MarketplaceId = f.Random.Guid(),
                    IsActive = f.Random.Bool(),
                    PlanName = f.Random.String(),
                    DataBaseName = f.Random.String(),
                    DateStart = f.Date.Past(),
                    DateEnd = f.Date.Past(),
                    DateRenew = f.Date.Future()
                });
            return faker;
        }

        public static ProcessOcrResultDto FindValidProcessOcrResultDto()
        {
            var faker = new Faker<ProcessOcrResultDto>("pt_BR")
                .CustomInstantiator(f => new ProcessOcrResultDto
                {
                    Model = f.Random.String(),
                    ReferenceFile = f.Random.Guid().ToString(),
                    Tenant = f.Random.String(),
                    AnalyzeResult = new Domain.Utils.AnalyzeResultAzure.AnalyzeResultCustomDto()
                });

            return faker;
        }

        public static TenantInfoDto FindValidTenantInfoDto()
        {
            var faker = new Faker<TenantInfoDto>("pt_BR")
                .CustomInstantiator(f => new TenantInfoDto
                {
                    ChunkSize = f.Random.Int(),
                    DatabaseName = f.Random.String(),
                    Email = f.Random.String(),
                    EmbeddingModelName = f.Random.String(),
                    KValue = f.Random.Int(),
                    MaxTokens = f.Random.Int(),
                    Model = f.Random.String(),
                    Name = f.Random.String(),
                    OcrModel = f.Random.String(),
                    RagProvider = f.PickRandom<RagProvider>(),
                    RefineTemplate = f.Random.String(),
                    SearchMode = f.Random.String(),
                    Template = f.Random.String(),
                });

            return faker;
        }

        public static IEnumerable<DocumentEmbeddingsAddDto> FindValidDocumentEmbeddingsAddDto()
        {
            var documentEmbeddingsAddDto = new Faker<DocumentEmbeddingsAddDto>("pt_BR")
                .RuleFor(a => a.Tenant, "test")
                .RuleFor(a => a.ReferenceFile, "test")
                .RuleFor(a => a.KeyMongoAccess, f => "test")
                .RuleFor(a => a.Text, "test")
                .RuleFor(a => a.EmbeddingModelName, "test")
                .RuleFor(a => a.ChunkSize, 1)
                .RuleFor(a => a.Email, "test");

            return documentEmbeddingsAddDto.Generate(2);
        }

        public static DocumentEmbeddingsDataDto FindValidDocumentEmbeddingsDataDto()
        {
            var faker = new Faker<DocumentEmbeddingsDataDto>("pt_BR")
                .CustomInstantiator(f => new DocumentEmbeddingsDataDto
                {
                    ReferenceFile = f.Random.Guid().ToString(),
                    ResponseQueue = f.Random.String(),
                    DocumentEmbeddings = [.. FindValidDocumentEmbeddingsAddDto()]
                });
            return faker;
        }

        public static DocumentEmbeddingsResultDto FindValidDocumentEmbeddingsResultDto()
        {
            var faker = new Faker<DocumentEmbeddingsResultDto>("pt_BR")
                .CustomInstantiator(f => new DocumentEmbeddingsResultDto
                {
                    ReferenceFile = f.Random.Guid().ToString(),
                    KeyMongoAccess = f.Random.String(),
                    Tenant = f.Random.String(),
                    Email = f.Random.String(),
                    TotalPages = f.Random.Int(1, 10)
                });
            return faker;
        }

        public static AutomationOutputDto FindValidAutomationOutputDto()
        {
            var faker = new Faker<AutomationOutputDto>("pt_BR")
                .CustomInstantiator(f => new AutomationOutputDto
                {
                    Tenant = f.Random.String(),
                    Email = f.Random.String(),
                    ExecutionId = f.Random.Int(1, 10),
                    Content = f.Random.ToString()
                });
            return faker;
        }

        public static ChatCompletionResponseDto FindValidChatCompletionResponseDto()
        {
            JObject mockJObject = new JObject();
            mockJObject.Add("CardId", 1);
            mockJObject.Add("StepToolId", 30);

            var faker = new Faker<ChatCompletionResponseDto>("pt_BR")
                .CustomInstantiator(f => new ChatCompletionResponseDto
                {
                    ReferenceFile = f.Random.Guid().ToString(),
                    Tenant = f.Random.String(),
                    Email = f.Random.String(),
                    Choices = new[]
                    {
                        new ChatChoiceDto
                        {
                            Message = new ChatMessageResponseDto
                            {
                                Role = "assistant", Content = f.Lorem.Paragraph()
                            },
                        }
                    }.ToList(),
                    Usage = new ChatUsageDto
                    {
                        PromptTokens = f.Random.Int(1, 1000),
                        CompletionTokens = f.Random.Int(1, 1000),
                        TotalTokens = f.Random.Int(1, 2000)
                    },
                    Data = mockJObject
                });
            return faker;
        }

        public static OpenAiResponseConsumerResponseDto FindValidOpenAiResponseConsumerResponseDto(bool emptyMessage = false)
        {
            JObject mockJObject = JObject.FromObject(FindValidMetaDataAutomationDto());

            var faker = new Faker<OpenAiResponseConsumerResponseDto>("pt_BR")
                .CustomInstantiator(f => new OpenAiResponseConsumerResponseDto
                {
                    ReferenceFile = f.Random.Guid().ToString(),
                    Tenant = f.Random.String(),
                    Email = f.Random.String(),
                    Response = new ResponseOpenAiResponseDto
                    {

                        Usage = new ResponseOpenAiResponseUsageDto
                        {
                            InputTokens = f.Random.Int(1, 1000),
                            OutputTokens = f.Random.Int(1, 1000),
                            TotalTokens = f.Random.Int(1, 2000)
                        },
                        Output = new List<ResponseOpenAiResponseOutputDto> {
                            new ResponseOpenAiResponseOutputDto {
                                Output = f.Lorem.Paragraph(),
                                Type = OpenAiResponsesTypes.Message,
                                Arguments = f.Lorem.Paragraph(),
                                Content = new List<ResponseOpenAiResponseOutputMessageContentDto> {
                                    new ResponseOpenAiResponseOutputMessageContentDto {
                                        Text  = emptyMessage ? string.Empty : f.Lorem.Paragraph(),
                                        Type = OpenAiResponseInputContentType.OutputText
                                    }
                                }
                            }
                        }
                    },
                    Data = mockJObject
                });
            return faker;
        }

        public static StepToolExecution FindValidStepToolExecution(MetaDataAutomationDto metadata)
        {
            var faker = new Faker<StepToolExecution>("pt_BR")
                .CustomInstantiator(f => new StepToolExecution(f.Random.Int(0), DateTime.Now, metadata.StepToolId, Domain.Enum.StatusExecution.Pending, metadata.CardId)
                {
                    Card = new Card(metadata.CardId, DateTime.Now, metadata.StepToolId, f.Random.Int(0), f.Random.String(100), 1, Guid.NewGuid())
                });
            return faker;
        }

        public static MetaDataAutomationDto FindValidMetaDataAutomationDto()
        {
            var faker = new Faker("pt_BR");
            return new MetaDataAutomationDto()
            {
                CardId = faker.Random.Int(0),
                StepToolId = faker.Random.Int(0)
            };
        }

        public static SubscriptionPeriodDto FindValidSubscriptionPeriodDto()
        {
            var faker = new Faker<SubscriptionPeriodDto>("pt_BR")
                .CustomInstantiator(f => new SubscriptionPeriodDto
                {
                    Tenant = f.Random.String(),
                    PeriodStart = f.Date.Past(),
                    PeriodEnd = f.Date.Future()
                });
            return faker;
        }

        public static ExternalFileUploadDto FindValidExternalFileUploadDto()
        {
            var faker = new Faker<ExternalFileUploadDto>("pt_BR")
                .CustomInstantiator(f => new ExternalFileUploadDto
                {
                    FileName = f.System.FileName(),
                    FileReference = f.Random.Guid().ToString(),
                    Tenant = f.Random.String(),
                    Email = f.Internet.Email(),
                    WorkflowId = f.Random.Int(1, 100)
                });
            return faker;
        }

        internal static DocumentEmbeddingsQueryResponseDto FindValidDocumentEmbeddingsQueryResponseDto()
        {
            JObject mockJObject = new JObject();
            mockJObject.Add("CardId", 1);
            mockJObject.Add("StepToolId", 30);

            var faker = new Faker<DocumentEmbeddingsQueryResponseDto>("pt_BR")
                .CustomInstantiator(f => new DocumentEmbeddingsQueryResponseDto
                {
                    ReferenceFile = f.Random.Guid().ToString(),
                    Tenant = f.Random.String(),
                    Email = f.Random.String(),
                    QuestionsAnswers = new List<QuestionAnswerDto>
                    {
                        new QuestionAnswerDto
                        {
                            Question = f.Lorem.Sentence(),
                            Answer = f.Lorem.Paragraph(),
                            Usage = new List<QueryUsageDto>
                            {
                                new QueryUsageDto
                                {
                                    Prompt_usage = f.Random.Int(1, 1000),
                                    Completion_usage = f.Random.Int(1, 1000),
                                    Total_usage = f.Random.Int(1, 2000)
                                }
                            }
                        }
                    },
                    Data = mockJObject
                });
            return faker;
        }

        public static ApiOutputDto FindValidApiOutputDto()
        {
            var faker = new Faker<ApiOutputDto>("pt_BR")
                .CustomInstantiator(f => new ApiOutputDto
                {
                    TemplateName = f.Random.String(),
                    Tenant = f.Random.String(),
                    Email = f.Internet.Email(),
                    ExecutionId = f.Random.Int(1, 10),
                    StatusCode = f.Random.Int(100, 599),
                    Content = "{}"
                });
            return faker;
        }
        public static List<ApiTemplateDto> FindValidListApiTemplateDto()
        {
            var faker = new Faker<List<ApiTemplateDto>>("pt_BR")
                .CustomInstantiator(f =>
                {
                    var id1 = f.Random.Int(0, 8);
                    var id2 = f.Random.Int(0, 8);

                    while (id2 == id1)
                        id2 = f.Random.Int(0, 8);

                    return new List<ApiTemplateDto> {
                        new ApiTemplateDto {
                            Id = id1,
                            Created = DateTime.Now,
                            Name = string.Format("Api {0}",id1),
                            Method = "GET",
                            Url = string.Format("http://localhost/api-{0}",id1),
                            Description = "",
                            EnableAccessFromMcp = true,
                            BodyTemplate = "{}"
                        },
                        new ApiTemplateDto {
                            Id = id2,
                            Created = DateTime.Now,
                            Name = string.Format("Api {0}",id2),
                            Method = "GET",
                            Url = string.Format("http://localhost/api-{0}",id2),
                            Description = "",
                            EnableAccessFromMcp = true,
                            BodyTemplate = "{}"
                        }
                    };
                });
            return faker;
        }

        public static PromptTemplatesResponse FindValidPromptTemplatesResponseSort()
        {
            var faker = new Faker<PromptTemplatesResponse>("pt_BR")
                .CustomInstantiator(f => new PromptTemplatesResponse()
                {
                    Prompts = new List<PromptTemplateDto> {
                        new PromptTemplateDto
                        {
                            Id = Guid.NewGuid(),
                            Name = "B",
                            Description = "Desc",
                            Text = "Text",
                            Created = new DateTime(2026, 1, 2)
                        },
                        new PromptTemplateDto
                        {
                            Id = Guid.NewGuid(),
                            Name = "A",
                            Description = "Desc",
                            Text = "Text",
                            Created = new DateTime(2026, 1, 3)
                        },
                        new PromptTemplateDto
                        {
                            Id = Guid.NewGuid(),
                            Name = "C",
                            Description = "Desc",
                            Text = "Text",
                            Created = new DateTime(2026, 1, 1)
                        }
                    }
                });
            return faker;
        }
        public static PromptTemplatesResponse FindValidPromptTemplatesResponse(Guid? id = null)
        {
            var faker = new Faker<PromptTemplatesResponse>("pt_BR")
                .CustomInstantiator(f => new PromptTemplatesResponse()
                {
                    Prompts = new List<PromptTemplateDto>
                    {
                        new PromptTemplateDto
                        {
                            Id = id ?? Guid.NewGuid(),
                            Name = "Template 1",
                            Description = "Desc 1",
                            Text = "Text 1",
                            Created = DateTime.Now
                        }
                    }
                });
            return faker;
        }

        public static List<PromptIntegrationDto> FindValidPromptInternalDtoList()
        {
            var faker = new Faker<List<PromptIntegrationDto>>("pt_BR")
                .CustomInstantiator(f => new List<PromptIntegrationDto>
                    {
                        new PromptIntegrationDto {
                            Id = f.Random.Int(1),
                            Name = string.Format("Prompt {0}", f.Random.Int(1)),
                            Description = string.Format("Description {0}", f.Random.Int(1))
                        },
                        new PromptIntegrationDto {
                            Id = f.Random.Int(1),
                            Name = string.Format("Prompt {0}", f.Random.Int(1)),
                            Description = string.Format("Description {0}", f.Random.Int(1))
                        },
                        new PromptIntegrationDto {
                            Id = f.Random.Int(1),
                            Name = string.Format("Prompt {0}", f.Random.Int(1)),
                            Description = string.Format("Description {0}", f.Random.Int(1))
                        }
                    }
                );
            return faker;
        }

        public static AutomationServicesDto FindValidAutomationServicesDto()
        {
            var faker = new Faker<AutomationServicesDto>("pt_BR")
                .CustomInstantiator(f => new AutomationServicesDto(
                    f.Random.Int(1, 100),
                    f.Random.Int(1, 100),
                    f.Random.String(),
                    f.Internet.Email(),
                    f.Random.String(),
                    f.Random.Int(1, 10)
                ));
            return faker;
        }

        public static PromptCreateDto FindValidPromptCreateDto()
        {
            var faker = new Faker<PromptCreateDto>("pt_BR")
                .CustomInstantiator(f => new PromptCreateDto
                {
                    Name = f.Name.JobArea(),
                    Description = f.Name.JobTitle(),
                    Text = f.Name.JobDescriptor()
                });
            return faker;
        }

        public static PromptIntegrationCreateDto FindValidPromptIntegrationCreateDto()
        {
            var faker = new Faker<PromptIntegrationCreateDto>("pt_BR")
                .CustomInstantiator(f => new PromptIntegrationCreateDto
                {
                    Name = f.Name.JobArea(),
                    Description = f.Name.JobTitle(),
                    Text = f.Name.JobDescriptor()
                });
            return faker;
        }

        public static (PromptUpdateDto, PromptDto) FindValidPromptUpdateDtoAndPromptDto()
        {
            var fakerPromptUpdateDto = new Faker<PromptUpdateDto>("pt_BR")
                .CustomInstantiator(f => new PromptUpdateDto
                {
                    Id = f.Random.Int(),
                    Name = f.Name.JobArea(),
                    Description = f.Name.JobTitle(),
                    Text = f.Name.JobDescriptor()
                });

                var obj = fakerPromptUpdateDto.Generate();

            var fakerPromptDto = new Faker<PromptDto>("pt_BR")
                .CustomInstantiator(f => new PromptDto
                {
                    Id = obj.Id,
                    Name = f.Name.JobArea(),
                    Description = f.Name.JobTitle(),
                    Text = f.Name.JobDescriptor(),
                    IdUser = Guid.NewGuid(),
                    Created = DateTime.Now
                });
            return (obj, fakerPromptDto.Generate());
        }
    }

    [CollectionDefinition(nameof(MessagingCollection))]
    public class MessagingCollection : ICollectionFixture<MessagingFixture>
    {
    }
}
