using Bogus;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class MessagingFixture
    {
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
    }

    [CollectionDefinition(nameof(MessagingCollection))]
    public class MessagingCollection : ICollectionFixture<MessagingFixture>
    {

    }
}
