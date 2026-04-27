using Bogus;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;
using Xunit;
namespace WoopiAiHub.UnitTests.Fixture
{
    public class PromptFixture
    {
        public static Domain.Models.Prompt FindValidPrompt()
        {
            Domain.Models.Prompt prompt = new Faker<Domain.Models.Prompt>("pt_BR")
           .CustomInstantiator(f => new Domain.Models.Prompt
           (
               id: f.IndexFaker,
               created: f.Date.Past(),
               name: "name",
               description: f.Lorem.Paragraph(),
               text: f.Lorem.Paragraph(),
               idUser: Guid.NewGuid()
           ));

            return prompt;
        }

        public static Domain.Models.Prompt FindInvalidPrompt()
        {
            Domain.Models.Prompt prompt = new Faker<Domain.Models.Prompt>("pt_BR")
          .CustomInstantiator(f => new Domain.Models.Prompt
          (
              id: f.IndexFaker,
              created: f.Date.Past(),
              name: string.Empty,
              description: string.Empty,
              text: string.Empty,
              idUser: Guid.NewGuid()
          ));

            return prompt;
        }

        public static PromptDto FindValidPromptDto(bool enableAccessToMcp = true)
        {
            var _faker = new Faker("pt_BR");
            var promptId = _faker.Random.Int(1, 1000);
            return new PromptDto
            {
                Id = promptId,
                Name = _faker.Name.FullName(),
                Description = _faker.Name.FullName(),
                Text = _faker.Name.FullName(),
                IdUser = Guid.NewGuid(),
                IsOwner = true,
                Created = _faker.Date.Past(),
                OwnerName = _faker.Name.FullName(),
                OwnerEmail = _faker.Internet.Email(),
                EnableAccessToMcp = enableAccessToMcp,
                PromptApiTemplates = enableAccessToMcp ?
                    new List<PromptApiTemplateDto> {
                        new PromptApiTemplateDto{
                            Id = 1,
                            PromptId = promptId,
                            ApiTemplateId = 1
                        }
                    } :
                    new List<PromptApiTemplateDto>()
            };
        }
    }
    [CollectionDefinition(nameof(PromptCollection))]
    public class PromptCollection : ICollectionFixture<PromptFixture>
    {
    }
}
