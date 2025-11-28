using WoopiAiHub.Domain.DTOs;
using Bogus;
using Xunit;
using Refit;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;
using Azure.Storage.Blobs.Models;
using WoopiAiHub.Domain.DTOs.Request.Account;

namespace WoopiAiHub.UnitTests.Fixtures
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

        public static User FindValidUser()
        {
            return new Faker<User>("pt_BR")
            .CustomInstantiator(f => new User
            (
                Guid.NewGuid(),
                f.Person.FullName,
                f.Person.Email,
                true,
                DateTime.Now
            ));
        }

        public static LoginDto FindValidLoginDto()
        {
            return new Faker<LoginDto>("pt_BR")
            .CustomInstantiator(f => new LoginDto
            {
                Email = f.Person.Email,
                Password = f.Internet.Password(8, true)
            });
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
                Tenants = [$"{f.Company.CompanyName()}_{f.Internet.Email()}"]
            });
            return response;
        }
    }

    [CollectionDefinition(nameof(AccountCollection))]
    public class AccountCollection : ICollectionFixture<AccountFixture>
    {
    }
}
