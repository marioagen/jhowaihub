using Bogus;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.UnitTests.Fixtures
{
    public class AnonymizationFixture
    {
        /// <summary>
        /// Creates a valid FindDocumentDto with random test data.
        /// </summary>
        /// <returns>A FindDocumentDto instance with valid test data.</returns>
        public static FindDocumentDto FindValidFindDocumentDto()
        {
            var faker = new Faker<FindDocumentDto>("pt_BR");
            return faker.CustomInstantiator(f => new FindDocumentDto
            {
                BytesDocument = f.Random.Bytes(1000),
                ReferenceFile = f.Random.String(20),
                DocumentName = f.System.FileName()
            }).Generate();
        }

        /// <summary>
        /// Creates a valid ProcessAnonymizationRequestDto with random test data.
        /// </summary>
        /// <returns>A ProcessAnonymizationRequestDto instance with valid test data.</returns>
        public static ProcessAnonymizationRequestDto FindValidProcessAnonymizationRequestDto()
        {
            return new Faker<ProcessAnonymizationRequestDto>("pt_BR")
                .CustomInstantiator(f => new ProcessAnonymizationRequestDto
                {
                    DocumentId = f.Random.Int(1, 10000),
                    CardId = f.Random.Int(1, 10000),
                    WorkflowId = f.Random.Int(1, 10000),
                    AnonymizationType = AnonymizationType.PartialMasking,
                    PromptId = f.Random.Int(1, 1000)
                })
                .Generate();
        }

        /// <summary>
        /// Creates a valid HeadersDto with random test data.
        /// </summary>
        /// <returns>A HeadersDto instance with valid test data.</returns>
        public static HeadersDto FindValidHeadersDto()
        {
            return new Faker<HeadersDto>("pt_BR")
                .CustomInstantiator(f => new HeadersDto
                {
                    Tenant = f.Random.String(10),
                    EmailCreator = f.Person.Email
                })
                .Generate();
        }

        /// <summary>
        /// Creates a valid AnonymizationResultDto with random test data.
        /// </summary>
        /// <returns>An AnonymizationResultDto instance with valid test data.</returns>
        public static AnonymizationResultDto FindValidAnonymizationResultDto()
        {
            return new Faker<AnonymizationResultDto>("pt_BR")
                .CustomInstantiator(f => new AnonymizationResultDto
                {
                    WoopiAiEmail = f.Person.Email,
                    WoopiAiDocumentId = f.Random.Int(1, 10000),
                    DocumentUrl = f.Internet.Url(),
                    WoopiAiTenant = f.Random.String(10)
                })
                .Generate();
        }

        /// <summary>
        /// Creates a valid AnonymizationResponseDto with random test data.
        /// </summary>
        /// <returns>An AnonymizationResponseDto instance with valid test data.</returns>
        public static AnonymizationResponseDto FindValidAnonymizationResponseDto()
        {
            return new Faker<AnonymizationResponseDto>("pt_BR")
                .CustomInstantiator(f => new AnonymizationResponseDto
                {
                    Document = new AnonymizationDocumentResponseDto
                    {
                        Download = f.Internet.Url()
                    }
                })
                .Generate();
        }

        /// <summary>
        /// Creates an AnonymizationResponseDto without a download URL for testing error scenarios.
        /// </summary>
        /// <returns>An AnonymizationResponseDto instance with an empty Download URL.</returns>
        public static AnonymizationResponseDto FindAnonymizationResponseDtoWithoutDownloadUrl()
        {
            return new AnonymizationResponseDto
            {
                Document = new AnonymizationDocumentResponseDto
                {
                    Download = string.Empty
                }
            };
        }

        /// <summary>
        /// Creates a valid DocumentAnonymizationDto with random test data.
        /// </summary>
        /// <returns>A DocumentAnonymizationDto instance with valid test data.</returns>
        public static DocumentAnonymizationDto FindValidDocumentAnonymizationDto()
        {
            return new Faker<DocumentAnonymizationDto>("pt_BR")
                .CustomInstantiator(f => new DocumentAnonymizationDto
                {
                    Id = f.Random.Int(1, 10000),
                    Created = f.Date.Past(),
                    DocumentId = f.Random.Int(1, 10000),
                    DocumentUrl = f.Internet.Url(),
                    DocumentName = f.System.FileName()
                })
                .Generate();
        }

        /// <summary>
        /// Creates a collection of valid DocumentAnonymizationDto with random test data.
        /// </summary>
        /// <param name="documentId">The document ID to associate with all generated anonymized documents.</param>
        /// <param name="count">The number of DocumentAnonymizationDto instances to generate. Defaults to 2.</param>
        /// <returns>A list of DocumentAnonymizationDto instances with the specified document ID.</returns>
        public static ICollection<DocumentAnonymizationDto> FindValidDocumentAnonymizationDtoCollection(int documentId, int count = 2)
        {
            return new Faker<DocumentAnonymizationDto>("pt_BR")
                .CustomInstantiator(f => new DocumentAnonymizationDto
                {
                    Id = f.Random.Int(1, 10000),
                    Created = f.Date.Past(),
                    DocumentId = documentId,
                    DocumentUrl = f.Internet.Url(),
                    DocumentName = f.System.FileName()
                })
                .Generate(count);
        }

        /// <summary>
        /// Creates a valid Document with random test data.
        /// </summary>
        /// <returns>A Document instance with valid test data.</returns>
        public static Document FindValidDocument()
        {
            var faker = new Faker("pt_BR");
            return new Document(
                faker.System.FileName(),
                faker.Lorem.Sentence(3),
                faker.Random.String(20),
                DocumentStatus.Analyzed,
                faker.Person.Email,
                faker.Random.Int(1, 1000),
                new List<Workflow>(),
                faker.Date.Past(),
                false);
        }

        /// <summary>
        /// Creates an AnonymizationResultDto with a specific tenant value for testing.
        /// </summary>
        /// <param name="tenant">The tenant identifier to associate with the anonymization result.</param>
        /// <returns>An AnonymizationResultDto instance with the specified tenant.</returns>
        public static AnonymizationResultDto FindAnonymizationResultDtoWithTenant(string tenant)
        {
            return new Faker<AnonymizationResultDto>("pt_BR")
                .CustomInstantiator(f => new AnonymizationResultDto
                {
                    WoopiAiEmail = f.Person.Email,
                    WoopiAiDocumentId = f.Random.Int(1, 10000),
                    DocumentUrl = f.Internet.Url(),
                    WoopiAiTenant = tenant
                })
                .Generate();
        }
    }
}
