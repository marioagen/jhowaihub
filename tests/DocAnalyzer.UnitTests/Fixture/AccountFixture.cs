using DocAnalyzer.Domain.DTOs;
using Bogus;
using Xunit;
using Refit;
using DocAnalyzer.Domain.DTOs.Refit;
using DocAnalyzer.Domain.DTOs.Request;

namespace DocAnalyzer.UnitTests.Fixtures
{
    public class AccountFixture
    {
        public AuthenticateDto FindValidAuthenticate()
        {
            AuthenticateDto authenticateTests = new Faker<AuthenticateDto>("pt_BR")
            .CustomInstantiator(f => new AuthenticateDto
            {
                Login = f.Person.Email,
            });
            return authenticateTests;
        }

        public ApiResponse<UserGraphApiResponse> FindValidUserGraphApiResponse()
        {
            ApiResponse<UserGraphApiResponse> response = new Faker<ApiResponse<UserGraphApiResponse>>("pt_BR")
            .CustomInstantiator(f => new ApiResponse<UserGraphApiResponse>(
                new HttpResponseMessage(System.Net.HttpStatusCode.OK),
                new UserGraphApiResponse
                {
                    Mail = f.Person.Email,
                    UserPrincipalName = f.Person.Email,
                },
                new RefitSettings()
            ));
            return response;
        }

        public static AuthenticateHeaderDto FindValidAuthenticateHeaderDto()
        {
            return new AuthenticateHeaderDto { Authorization = Guid.NewGuid().ToString() };
        }

        public static AuthenticateDto FindValidAuthenticateDto()
        {
            return new Faker<AuthenticateDto>("pt_BR")
            .CustomInstantiator(f => new AuthenticateDto { Login = f.Person.FullName });
        }

        public ResponseCheckAccessDto FindValidResponseCheckAccessDto()
        {
            ResponseCheckAccessDto response = new Faker<ResponseCheckAccessDto>("pt_BR")
            .CustomInstantiator(f => new ResponseCheckAccessDto
            {
                HasAccess = true,
                Tenant = $"{f.Company.CompanyName()}_{f.Internet.Email()}"
            });
            return response;
        }
    }

    [CollectionDefinition(nameof(AccountCollection))]
    public class AccountCollection : ICollectionFixture<AccountFixture>
    {
    }
}
