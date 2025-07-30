using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class ProfileFixture
    {
    }

    [CollectionDefinition(nameof(ProfileCollection))]
    public class ProfileCollection : ICollectionFixture<ProfileFixture>
    {
    }
}
