using Bogus;
using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.Enum;
using DocAnalyzer.Domain.Models;
using Xunit;

namespace DocAnalyzer.UnitTests.Fixture
{
    public class TypeDocFixture
    {
        public ICollection<TypeDoc> FindValidTypeDocList()
        {
            var TypeDoc = new Faker<TypeDoc>("pt_BR")
            .CustomInstantiator(f => new TypeDoc
            (
                name: "Name",
                id: f.IndexFaker,
                emailCreator: f.Person.Email,
                created: f.Date.Past()
            ));
            return TypeDoc.Generate(1);
        }

        public static TypeDocPagedDataDto FindValidTypeDocPagedDataDto(int pageSize)
        {
            TypeDocPagedDataDto typeDocPagedDataDto = new Faker<TypeDocPagedDataDto>("pt_BR")
            .RuleFor(a => a.Page, f => 1)
            .RuleFor(a => a.PageSize, f => pageSize)
            .RuleFor(a => a.Search, f => f.Lorem.Text())
            .RuleFor(a => a.IsAscending, f => f.Random.Bool())
            .RuleFor(a => a.ColType, f => f.PickRandom<ColTypeDoc>());

            return typeDocPagedDataDto;
        }

        public TypeDocPagedDataDto FindInvalidTypeDocPagedDataDto()
        {
            TypeDocPagedDataDto typeDocPagedDataDto = new Faker<TypeDocPagedDataDto>("pt_BR")
            .RuleFor(a => a.Page, f => 0)
            .RuleFor(a => a.PageSize, f => f.IndexFaker)
            .RuleFor(a => a.Search, f => f.Lorem.Text())
            .RuleFor(a => a.IsAscending, f => f.Random.Bool())
            .RuleFor(a => a.ColType, f => f.PickRandom<ColTypeDoc>());

            return typeDocPagedDataDto;
        }

        public static TypeDocUpdateDto FindValidUpdateTypeDocDto()
        {
            TypeDocUpdateDto updateTypeDocDto = new Faker<TypeDocUpdateDto>("pt_BR")
            .RuleFor(a => a.Id, f => f.IndexFaker)
            .RuleFor(a => a.Name, f => "name");

            return updateTypeDocDto;
        }

        public static IEnumerable<TypeDocDto> FindValidTypeDocDto(int pageSize)
        {
            var typeDocDto = new Faker<TypeDocDto>("pt_BR")
            .RuleFor(a => a.Id, f => f.IndexFaker)
            .RuleFor(a => a.Name, f => f.Person.FirstName)
            .RuleFor(a => a.Created, f => f.Date.Past())
            .RuleFor(a => a.EmailCreator, f => f.Person.Email);

            return typeDocDto.Generate(pageSize); 
        }

        public static TypeDocCreateDto FindValidTypeDocCreateDto()
        {
            TypeDocCreateDto typeDocCreateDto = new Faker<TypeDocCreateDto>("pt_BR")
            .RuleFor(a => a.Name, f => f.Person.FirstName);
            return typeDocCreateDto;
        }

        public static HeadersDto FindValidTypeDocHeaderDto()
        {
            HeadersDto typeDocHeaderDto = new Faker<HeadersDto>("pt_BR")
            .RuleFor(a => a.EmailCreator, f => f.Person.FirstName);
            return typeDocHeaderDto;
        }
    }

    [CollectionDefinition(nameof(TypeDocCollection))]
    public class TypeDocCollection : ICollectionFixture<TypeDocFixture>
    {
    }
}
