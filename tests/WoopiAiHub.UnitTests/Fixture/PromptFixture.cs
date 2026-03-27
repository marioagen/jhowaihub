using Bogus;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;
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

        public static PromptDto FindValidPromptDto()
        {
            var _faker = new Faker("pt_BR");
            return new PromptDto
            {
                Id = _faker.Random.Int(1, 1000),
                Name = _faker.Name.FullName(),
                Description = _faker.Name.FullName(),
                Text = _faker.Name.FullName(),
                IdUser = Guid.NewGuid(),
                IsOwner = true,
                Created = _faker.Date.Past(),
                OwnerName = _faker.Name.FullName(),
                OwnerEmail = _faker.Internet.Email(),
            };
        }
    }
    [CollectionDefinition(nameof(PromptCollection))]
    public class PromptCollection : ICollectionFixture<PromptFixture>
    {
    }
}
