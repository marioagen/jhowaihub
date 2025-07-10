using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public  class UserFixture
    {
        public RequestAssignLicensesByHub FindValidRequestAssignLicensesByHub()
        {
            var faker = new Faker<RequestAssignLicensesByHub>("pt_BR")
              .CustomInstantiator(f => new RequestAssignLicensesByHub
              {
                  UserEmail = f.Internet.Email(),
                  Tenant = "TenantTest",
              });

            return faker;
        }
    }

    [CollectionDefinition(nameof(UserCollection))]
    public class UserCollection : ICollectionFixture<UserFixture>
    {
    }
}

