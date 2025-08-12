using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
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
        private readonly Mock<IProfileRepository> _profileRepoMock;
        private readonly Mock<IPermissionRepository> _permissionRepoMock;
        private readonly ProfileServices _service;

        public ProfileServicesTests(ProfileFixture profileFixture)
        {
            _profileFixture = profileFixture;
            _mocker = new AutoMocker();
            _profileServices = _mocker.CreateInstance<ProfileServices>();
            _profileRepoMock = new Mock<IProfileRepository>();
            _permissionRepoMock = new Mock<IPermissionRepository>();
            _service = new ProfileServices(_profileRepoMock.Object, _permissionRepoMock.Object);
        }

        [Fact(DisplayName = "Test Find by id and returns profile when exists")]
        [Trait("FindById", "Success")]
        public async Task FindById_ReturnsProfile_WhenExists()
        {
            var profileDto = new ProfileDto { Id = 1, Name = "Test" };
            _profileRepoMock.Setup(r => r.FindById(1)).ReturnsAsync(profileDto);

            var result = await _service.FindById(1);

            Assert.Equal(1, result.Id);
            Assert.Equal("Test", result.Name);
        }


        [Fact(DisplayName = "Test FindAllPaged and returns profile paged result")]
        [Trait("FindAll", "Success")]
        public void FindAllPaged_ReturnsPagedResult()
        {
            var pagedData = new PagedDataDto { Page = 1, PageSize = 10, IsAscending = true, Search = "A" };
            var profiles = new List<ProfileDto>
            {
                new ProfileDto { Id = 1, Name = "A" },
                new ProfileDto { Id = 2, Name = "B" }
            }.AsQueryable();

            _profileRepoMock.Setup(r => r.FindAllPaged(pagedData)).Returns(profiles);

            var result = _service.FindAllPaged(pagedData);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Content.Any());
        }

        [Fact(DisplayName = "Test CreateUniqueProfile and returns true when success")]
        [Trait("CreateUniqueProfile", "Success")]
        public async Task CreateUniqueProfile_ReturnsTrue_WhenSuccess()
        {
            var dto = new ProfileCreateDto { Name = "Novo", PermissionsIds = new List<int> { 1 } };
            var permission = new Permission("Perm", "permName", "screen", 1, DateTime.Now);

            _permissionRepoMock.Setup(r => r.FindByIdsAsync(dto.PermissionsIds)).ReturnsAsync(new List<Permission> { permission });
            _profileRepoMock.Setup(r => r.CreateUniqueProfile(It.IsAny<Profile>())).Returns(true);

            var result = await _service.CreateUniqueProfile(dto);

            Assert.True(result);
        }

        [Fact(DisplayName = "Test Update and returns true when success")]
        [Trait("Update", "Success")]
        public async Task Update_ReturnsTrue_WhenSuccess()
        {
            var dto = new ProfileUpdateDto { Id = 1, Name = "Editado", PermissionsIds = new List<int> { 1 } };
            var profile = new Profile("Antigo", 1, DateTime.Now) { Permissions = new List<Permission>() };
            var permission = new Permission("Perm", "permName", "screen", 1, DateTime.Now);

            _profileRepoMock.Setup(r => r.FindByIdReturnModel(dto.Id)).Returns(profile);
            _permissionRepoMock.Setup(r => r.FindByIdsAsync(dto.PermissionsIds)).ReturnsAsync(new List<Permission> { permission });
            _profileRepoMock.Setup(r => r.Update(profile)).Returns(true);

            var result = await _service.Update(dto);

            Assert.True(result);
        }

        [Fact(DisplayName = "Test DeleteByIds and returns true when success")]
        [Trait("DeleteByIds", "Success")]
        public void DeleteByIds_ReturnsTrue_WhenSuccess()
        {
            var ids = new List<int> { 1, 2 };
            _profileRepoMock.Setup(r => r.DeleteByIds(ids)).Returns(true);

            var result = _service.DeleteByIds(ids);

            Assert.True(result);
        }

        [Fact(DisplayName = "Test FindById and throw exception when not id is not found")]
        [Trait("FindById", "Fail")]
        public async Task FindById_Throws_WhenNotFound()
        {
            _profileRepoMock.Setup(r => r.FindById(99)).ReturnsAsync((ProfileDto?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.FindById(99));
        }

        [Fact(DisplayName = "Test FindAllPaged and throw exception when page is invalid")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_Throws_WhenPageInvalid()
        {
            var pagedData = new PagedDataDto { Page = 0, PageSize = 10 };

            Assert.Throws<ArgumentException>(() => _service.FindAllPaged(pagedData));
        }

        [Fact(DisplayName = "Test CreateUniqueProfile and throw exception when name is empty")]
        [Trait("CreateUniqueProfile", "Fail")]
        public async Task CreateUniqueProfile_Throws_WhenNameEmpty()
        {
            var dto = new ProfileCreateDto { Name = "", PermissionsIds = new List<int>() };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateUniqueProfile(dto));
        }

        [Fact(DisplayName = "Test CreateUniqueProfile and throw exception when name is duplicated")]
        [Trait("CreateUniqueProfile", "Fail")]
        public async Task CreateUniqueProfile_Throws_WhenDuplicated()
        {
            var dto = new ProfileCreateDto { Name = "Duplicado", PermissionsIds = new List<int>() };
            var permission = new Permission("Perm", "permName", "screen", 1, DateTime.Now);
            _permissionRepoMock.Setup(r => r.FindByIdsAsync(dto.PermissionsIds)).ReturnsAsync(new List<Permission> { permission });
            _profileRepoMock.Setup(r => r.CreateUniqueProfile(It.IsAny<Profile>())).Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateUniqueProfile(dto));
        }

        [Fact(DisplayName = "Test Update and return false when profile is not found")]
        [Trait("Update", "Fail")]
        public async Task Update_ReturnsFalse_WhenProfileNotFound()
        {
            var dto = new ProfileUpdateDto { Id = 99, Name = "X", PermissionsIds = new List<int>() };
            _profileRepoMock.Setup(r => r.FindByIdReturnModel(dto.Id)).Returns((Profile)null);

            var result = await _service.Update(dto);

            Assert.False(result);
        }

        [Fact(DisplayName = "Test Update and return false when profile is duplicated")]
        [Trait("Update", "Fail")]
        public async Task Update_Throws_WhenDuplicated()
        {
            var dto = new ProfileUpdateDto { Id = 1, Name = "Duplicado", PermissionsIds = new List<int>() };
            var profile = new Profile("Antigo", 1, DateTime.Now) { Permissions = new List<Permission>() };
            var permission = new Permission("Perm", "permName", "screen", 1, DateTime.Now);

            _permissionRepoMock.Setup(r => r.FindByIdsAsync(dto.PermissionsIds)).ReturnsAsync(new List<Permission> { permission });
            _profileRepoMock.Setup(r => r.FindByIdReturnModel(dto.Id)).Returns(profile);
            _profileRepoMock.Setup(r => r.Update(profile)).Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Update(dto));
        }

        [Fact(DisplayName = "Test FindAll and return all profiles")]
        [Trait("FindAll", "Sucess")]
        public async Task FindAll_ShouldReturnAllProfiles()
        {
            // Arrange
            var profiles = new List<ProfileDto>
            {
                new ProfileDto { Id = 1, Name = "Profile1" },
                new ProfileDto { Id = 2, Name = "Profile2" }
            };
            _profileRepoMock.Setup(repo => repo.FindAll()).ReturnsAsync(profiles);

            // Act
            var result = await _service.FindAll();

            // Assert
            Assert.Equal(profiles, result);
            _profileRepoMock.Verify(repo => repo.FindAll(), Times.Once);
        }
    }
}
