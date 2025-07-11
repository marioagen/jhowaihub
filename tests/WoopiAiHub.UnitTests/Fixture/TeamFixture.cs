using Bogus;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class TeamFixture
    {
        private readonly Faker _faker = new("pt_BR");

        public Team CreateValidTeam()
        {
            return new Team(_faker.Company.CompanyName(), _faker.Random.Int(1, 1000), _faker.Date.Past())
            {
                Users = new List<User>
            {
                new User(Guid.NewGuid(), _faker.Person.FullName, _faker.Internet.Email(), true, _faker.Date.Past())
            }
            };
        }

        public TeamCreateDto CreateValidTeamCreateDto()
        {
            return new TeamCreateDto
            {
                Name = _faker.Company.CompanyName(),
                UserIds = new List<string> { "test" }
            };
        }

        public TeamUpdateDto CreateValidTeamUpdateDto(int id)
        {
            return new TeamUpdateDto
            {
                Id = id,
                Name = _faker.Company.CompanyName(),
                UserIds = new List<Guid> { Guid.NewGuid() }
            };
        }

        public TeamDto CreateValidTeamDto()
        {
            return new TeamDto
            {
                Id = _faker.Random.Int(1, 1000),
                Name = _faker.Company.CompanyName(),
                Created = _faker.Date.Past(),
                Users = new List<UserDto>
            {
                new UserDto
                {
                    Id = Guid.NewGuid(),
                    Name = _faker.Person.FullName,
                    Email = _faker.Internet.Email(),
                    IsActive = true
                }
            }
            };
        }
    }

    [CollectionDefinition(nameof(TeamCollection))]
    public class TeamCollection : ICollectionFixture<TeamFixture>
    {
    }
}
