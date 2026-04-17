using Bogus;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.IntegrationHub;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Utils;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class RagInvocationRouterFixture
    {
        public static TenantInfoDto FindValidTenantWithIndexer()
        {
            return new Faker<TenantInfoDto>("pt_BR")
                .CustomInstantiator(f => new TenantInfoDto
                {
                    Name = f.Company.CompanyName(),
                    Email = f.Person.Email,
                    DatabaseName = f.Database.Column(),
                    Template = "template",
                    RefineTemplate = "refine_template",
                    MaxTokens = 2000,
                    KValue = 5,
                    Model = "gpt-4",
                    EmbeddingModelName = "text-embedding-ada-002",
                    ChunkSize = 1000,
                    SearchMode = "any",
                    RagProvider = string.Empty, // Indexer route
                    OcrModel = "ocr_model",
                    Plan = "premium",
                    WtcsIncluded = 100,
                    AiGatewayKey = Guid.NewGuid().ToString(),
                    BillingId = Guid.NewGuid().ToString(),
                    AiGatewayApplicationId = Guid.NewGuid()
                }).Generate();
        }

        public static TenantInfoDto FindValidTenantWithAzureAiSearch()
        {
            return new Faker<TenantInfoDto>("pt_BR")
                .CustomInstantiator(f => new TenantInfoDto
                {
                    Name = f.Company.CompanyName(),
                    Email = f.Person.Email,
                    DatabaseName = f.Database.Column(),
                    Template = "template",
                    RefineTemplate = "refine_template",
                    MaxTokens = 2000,
                    KValue = 5,
                    Model = "gpt-4",
                    EmbeddingModelName = "text-embedding-ada-002",
                    ChunkSize = 1000,
                    SearchMode = "any",
                    RagProvider = RagProviderNames.AzureAiSearch,
                    OcrModel = "ocr_model",
                    Plan = "premium",
                    WtcsIncluded = 100,
                    AiGatewayKey = Guid.NewGuid().ToString(),
                    BillingId = Guid.NewGuid().ToString(),
                    AiGatewayApplicationId = Guid.NewGuid()
                }).Generate();
        }

        public static TenantInfoDto FindTenantMissingAiGatewayInfo()
        {
            return new Faker<TenantInfoDto>("pt_BR")
                .CustomInstantiator(f => new TenantInfoDto
                {
                    Name = f.Company.CompanyName(),
                    Email = f.Person.Email,
                    DatabaseName = f.Database.Column(),
                    Template = "template",
                    RefineTemplate = "refine_template",
                    MaxTokens = 2000,
                    KValue = 5,
                    Model = "gpt-4",
                    EmbeddingModelName = "text-embedding-ada-002",
                    ChunkSize = 1000,
                    SearchMode = "any",
                    RagProvider = string.Empty,
                    OcrModel = "ocr_model",
                    Plan = "premium",
                    WtcsIncluded = 100,
                    AiGatewayKey = string.Empty, // Missing key
                    BillingId = Guid.NewGuid().ToString(),
                    AiGatewayApplicationId = null // Missing ApplicationId
                }).Generate();
        }

        public static CustomQueryRequestRefitDto FindValidCustomQueryRequest()
        {
            return new Faker<CustomQueryRequestRefitDto>("pt_BR")
                .CustomInstantiator(f => new CustomQueryRequestRefitDto
                {
                    Question = f.Lorem.Sentence(),
                    kValue = 5,
                    Model = "gpt-4",
                    Template = "template",
                    Temperature = 1,
                    Refine_template = "refine_template",
                    Max_tokens = 2000,
                    SearchMode = "any"
                }).Generate();
        }

        public static QueryResponseModelRefitDto FindValidQueryResponseModel()
        {
            return new Faker<QueryResponseModelRefitDto>("pt_BR")
                .CustomInstantiator(f => new QueryResponseModelRefitDto
                {
                    response = f.Lorem.Paragraphs(3),
                    Usage = [
                        new QueryUsageDto
                        {
                            Model = "gpt-4",
                            Usage_unity = "tokens",
                            Prompt_usage = 100,
                            Completion_usage = 50,
                            Total_usage = 150
                        }
                    ]
                }).Generate();
        }

        public static ChatCompletionDto FindValidChatCompletionRequest()
        {
            return new Faker<ChatCompletionDto>("pt_BR")
                .CustomInstantiator(f => new ChatCompletionDto
                {
                    Messages =
                    [
                        new ChatMessageDto { Role = "user", Content = f.Lorem.Sentence() }
                    ]
                }).Generate();
        }

        public static ChatCompletionResponseDto FindValidChatCompletionResponse()
        {
            return new Faker<ChatCompletionResponseDto>("pt_BR")
                .CustomInstantiator(f => new ChatCompletionResponseDto
                {
                    Choices =
                    [
                        new ChatChoiceDto
                        {
                            Message = new ChatMessageResponseDto
                            {
                                Role = "assistant",
                                Content = f.Lorem.Paragraph()
                            }
                        }
                    ],
                    Usage = new ChatUsageDto
                    {
                        PromptTokens = 100,
                        CompletionTokens = 50,
                        TotalTokens = 150
                    }
                }).Generate();
        }

        public static IntegrationHubDocumentEmbeddingsQueryResponse FindValidIntegrationQueryResponse()
        {
            return new Faker<IntegrationHubDocumentEmbeddingsQueryResponse>("pt_BR")
                .CustomInstantiator(f => new IntegrationHubDocumentEmbeddingsQueryResponse
                {
                    QuestionsAnswers =
                    [
                        new IntegrationHubQuestionAnswerDto
                        {
                            Answer = f.Lorem.Paragraphs(3),
                            Usage =
                            [
                                new IntegrationHubQueryUsageDto
                                {
                                    Model = "gpt-4",
                                    Total_usage = 150
                                }
                            ]
                        }
                    ]
                }).Generate();
        }

        public static HttpResponseMessage FindValidHttpResponseMessage(string content = "{}")
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };
            return response;
        }

        public static HttpResponseMessage FindNotFoundHttpResponseMessage()
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not Found")
            };
        }

        public static HttpResponseMessage FindErrorHttpResponseMessage(string content = "Internal Server Error")
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(content)
            };
        }
    }

    [CollectionDefinition(nameof(RagInvocationRouterCollection))]
    public class RagInvocationRouterCollection : ICollectionFixture<RagInvocationRouterFixture>
    {
    }
}
