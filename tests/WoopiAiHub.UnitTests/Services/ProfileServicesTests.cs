using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(ProfileCollection))]
    public class ProfileServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly ProfileFixture _profileFixture;
        private readonly ProfileServices _profileServices;

        public ProfileServicesTests(ProfileFixture profileFixture)
        {
            _profileFixture = profileFixture;
            _mocker = new AutoMocker();
            _profileServices = _mocker.CreateInstance<ProfileServices>();
        }

        [Fact(DisplayName = "FindAllPaged should return first page of profiles")]
        [Trait("FindAllPaged", "Success")]
        public void FindAllPaged_ShouldReturnPagedResult_WhenPageIsGreaterThanZero()
        {
            // Arrange
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 10, IsAscending = true };
            var profiles = new List<ProfileDto>
            {
                new ProfileDto { Id = 1, Name = "Admin"}
            }.AsQueryable();

            var _profileRepositoryMock = _mocker.GetMock<IProfileRepository>();
            _profileRepositoryMock.Setup(r => r.FindAllPaged(pagedDataDto)).Returns(profiles);

            // Act
            var result = _profileServices.FindAllPaged(pagedDataDto);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Content);
        }

        [Fact(DisplayName = "FindAllPaged should return all profiles")]
        [Trait("FindAllPaged", "Success")]
        public void FindAllPaged_ShouldReturnPagedResult_WhenPageSizeIsZero()
        {
            // Arrange
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 0, IsAscending = true, Search = "Admin" };
            var profiles = new List<ProfileDto>
            {
                new ProfileDto { Id = 1, Name = "Admin"}
            }.AsQueryable();

            var _profileRepositoryMock = _mocker.GetMock<IProfileRepository>();
            _profileRepositoryMock.Setup(r => r.FindAllPaged(pagedDataDto)).Returns(profiles);

            // Act
            var result = _profileServices.FindAllPaged(pagedDataDto);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Content);
        }

        [Fact(DisplayName = "FindAllPaged should throw exceptio when page size is zero or less")]
        [Trait("FindPaged", "Fail")]
        public void FindAllPaged_ShouldThrowArgumentException_WhenPageIsZeroOrLess()
        {
            // Arrange
            var pagedDataDto = new PagedDataDto { Page = 0, PageSize = 10 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _profileServices.FindAllPaged(pagedDataDto));
        }

    }
}
