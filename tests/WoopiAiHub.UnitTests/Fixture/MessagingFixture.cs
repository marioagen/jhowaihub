using Azure.Storage.Blobs.Models;
using Bogus;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.DTOs.Response;
using Xunit;
using Newtonsoft.Json.Linq;

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
                  Choices = new[] {
                        new ChatChoiceDto {
                            Message = new ChatMessageResponseDto {
                                Role = "assistant",
                                Content = f.Lorem.Paragraph()
                            },
                        }
                    }.ToList(),
                  Usage = new ChatUsageDto
                  {
                      PromptTokens = f.Random.Int(1, 1000),
                      CompletionTokens = f.Random.Int(1, 1000),
                      TotalTokens = f.Random.Int(1, 2000)
                  },
                  Data  = mockJObject
              });
            return faker;
        }
    }

    [CollectionDefinition(nameof(MessagingCollection))]
    public class MessagingCollection : ICollectionFixture<MessagingFixture>
    {

    }
}
