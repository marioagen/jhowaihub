using Bogus;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;

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
                    DocumentUrl = f.Internet.Url()
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
    }
}
