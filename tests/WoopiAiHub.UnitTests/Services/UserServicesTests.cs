using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(UserCollection))]
    public class UserServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMarketPlaceApi> _marketPlaceApiMock;
        private readonly UserServices _userServices;
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly Mock<IProfileRepository> _profileRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly UserFixture _fixture;

        public UserServicesTests()
        {
            _fixture = new UserFixture();
            _mocker = new AutoMocker();
            _userRepositoryMock = _mocker.GetMock<IUserRepository>();
            _marketPlaceApiMock = _mocker.GetMock<IMarketPlaceApi>();
            _teamRepositoryMock = _mocker.GetMock<ITeamRepository>();
            _profileRepositoryMock = _mocker.GetMock<IProfileRepository>();
            _passwordHasherMock = _mocker.GetMock<IPasswordHasher>();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(config => config[It.Is<string>(s => s == "keyAccess")]).Returns("mockKeyAccess");
            configMock.Setup(x => x.GetSection("KeyAccess").Value).Returns(Guid.NewGuid().ToString());

            _mocker.Use(configMock);

            _userServices = _mocker.CreateInstance<UserServices>();
        }

        [Fact(DisplayName = "Create should return true when user is created")]
        [Trait("CreateUser", "Success")]
        public async Task Create_ShouldReturnTrue_WhenUserIsCreated()
        {
            // Arrange
            var userCreateDto = new UserCreateDto { Name = "Test", Email = "test@email.com", Password = "Password123", TeamIds = [1] };
            var headersDto = new HeadersDto { Tenant = "tenant" };
            var requestDto = _fixture.FindValidRequestAssignLicensesByHub();
            var userId = Guid.NewGuid();
            var user = new User(userId, userCreateDto.Name, userCreateDto.Email, true, DateTime.Now);

            _teamRepositoryMock
                .Setup(repo => repo.FindByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(new List<Team>());

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(userId);

            _userRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            _profileRepositoryMock
                .Setup(repo => repo.FindByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(new List<Profile>());


            // Act
            var result = await _userServices.Create(userCreateDto, headersDto);

            // Assert
            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact(DisplayName = "Create should return false when user is disabled")]
        [Trait("CreateUser", "Fail")]
        public async Task Create_ShouldReturnFalse_WhenUserNotEnabled()
        {
            // Arrange
            var userCreateDto = new UserCreateDto { Name = "Test", Email = "test@email.com", Password = "Password123", TeamIds = [] };
            var headersDto = new HeadersDto { Tenant = "tenant" };

            // Act
            var result = await _userServices.Create(userCreateDto, headersDto);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "DeactivateMultipleUsers")]
        [Trait("Deactivate", "Success")]
        public async Task DeactivateRange_ShouldReturnTrue_WhenAllUsersExistAndMarketplaceReturnsTrue()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var users = ids.Select(id => new User(id, "Name", "email@email.com", true, DateTime.Now)).ToList();

            _userRepositoryMock.Setup(r => r.FindByIdsAsync(ids)).ReturnsAsync(users);
            _marketPlaceApiMock
                .Setup(api => api.DeactivateUsersEnabledByReference(It.IsAny<string>(), It.IsAny<DeactivateUsersDto>()))
                .ReturnsAsync(true);
            _userRepositoryMock.Setup(r => r.DeactivateRange(ids)).Returns(true);

            // Act
            var result = await _userServices.DeactivateRange(ids);

            // Assert
            Assert.True(result);
            _userRepositoryMock.Verify(r => r.DeactivateRange(ids), Times.Once);
        }


        [Fact(DisplayName = "DeactivateMultipleUsers")]
        [Trait("Deactivate", "Fail")]
        public async Task DeactivateRange_ShouldReturnFalse_WhenNotAllUsersExist()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var users = new List<User> { new User(ids[0], "Name", "email@email.com", true, DateTime.Now) };

            _userRepositoryMock.Setup(r => r.FindByIdsAsync(ids)).ReturnsAsync(users);

            // Act
            var result = await _userServices.DeactivateRange(ids);

            // Assert
            Assert.False(result);
            _userRepositoryMock.Verify(r => r.DeactivateRange(It.IsAny<List<Guid>>()), Times.Never);
        }

        [Fact(DisplayName = "DeactivateMultipleUsers")]
        [Trait("Deactivate", "Fail")]
        public async Task DeactivateRange_ShouldReturnFalse_WhenMarketplaceReturnsFalse()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid() };
            var users = ids.Select(id => new User(id, "Name", "email@email.com", true, DateTime.Now)).ToList();

            _userRepositoryMock.Setup(r => r.FindByIdsAsync(ids)).ReturnsAsync(users);
            _marketPlaceApiMock
                .Setup(api => api.DeactivateUsersEnabledByReference(It.IsAny<string>(), It.IsAny<DeactivateUsersDto>()))
                .ReturnsAsync(false);

            // Act
            var result = await _userServices.DeactivateRange(ids);

            // Assert
            Assert.False(result);
            _userRepositoryMock.Verify(r => r.DeactivateRange(It.IsAny<List<Guid>>()), Times.Never);
        }


        [Fact(DisplayName = "FindAllUsers")]
        [Trait("FindAll", "Success")]
        public void FindAllPaged_ShouldReturnPagedResult_WhenPageIsGreaterThanZero()
        {
            // Arrange
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 10, IsAscending = true };
            var users = new List<UserPagedDto>
        {
            new UserPagedDto { Id = Guid.NewGuid(), Name = "A", Email = "a@a.com", IsActive = true, Created = DateTime.Now }
        }.AsQueryable();

            _userRepositoryMock.Setup(r => r.FindAllPaged(pagedDataDto)).Returns(users);

            // Act
            var result = _userServices.FindAllPaged(pagedDataDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Content.Count());
        }

        [Fact(DisplayName = "FindAllUsers")]
        [Trait("FindAll", "Fail")]
        public void FindAllPaged_ShouldThrowArgumentException_WhenPageIsZeroOrLess()
        {
            // Arrange
            var pagedDataDto = new PagedDataDto { Page = 0, PageSize = 10 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _userServices.FindAllPaged(pagedDataDto));
        }

        [Fact(DisplayName = "FindAllAsync")]
        [Trait("FindAllAsync", "Success")]
        public async Task FindAllAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new List<UserDto>();
            _userRepositoryMock.Setup(r => r.FindAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _userServices.FindAll();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _userRepositoryMock.Verify(r => r.FindAllAsync(), Times.Once);
        }

        [Fact(DisplayName = "Update should return true when update is sucess")]
        [Trait("Update", "Sucess")]
        public async Task Update_ShouldReturnTrue_WhenUpdateSucceeds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userUpdateDto = new UserUpdateDto
            {
                Id = userId,
                Name = "Novo Nome",
                Email = "novo@email.com",
                Password = "NovaSenha123",
                TeamIds = new List<int> { 1, 2 },
                ProfileIds = new List<int> { 1, 2 },
            };
            var headersDto = new HeadersDto { Tenant = "tenant" };
            var teams = new List<Team>
            {
                new Team("Time 1", 1, DateTime.Now),
                new Team("Time 2", 2, DateTime.Now)
            };
      
            var user = new User(userId, "Antigo Nome", "antigo@email.com", false, DateTime.Now)
            {
                Teams = teams,
            };

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(userId);

            _userRepositoryMock
                .Setup(repo => repo.FindByEmailAsync(userUpdateDto.Email))
                .ReturnsAsync(user);

            _teamRepositoryMock
                .Setup(repo => repo.FindByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(teams);

            _userRepositoryMock
                .Setup(repo => repo.Update(user))
                .Returns(true);

            // Act
            var result = await _userServices.Update(userUpdateDto, headersDto);

            // Assert
            Assert.True(result);
            Assert.Equal("Novo Nome", user.Name);
            Assert.Equal("novo@email.com", user.Email);
        }

        [Fact(DisplayName = "Update should return false when user is not found")]
        [Trait("Update", "Fail")]
        public async Task Update_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userUpdateDto = new UserUpdateDto
            {
                Id = userId,
                Name = "Nome",
                Email = "email@email.com"
            };
            var headersDto = new HeadersDto { Tenant = "tenant" };

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(userId);

            _userRepositoryMock
                .Setup(repo => repo.FindByEmailAsync(userUpdateDto.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userServices.Update(userUpdateDto, headersDto);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Update should return false when mkt fails")]
        [Trait("Update", "Fail")]
        public async Task Update_ShouldReturnFalse_WhenMarketplaceFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userUpdateDto = new UserUpdateDto
            {
                Id = userId,
                Name = "Nome",
                Email = "email@email.com"
            };
            var headersDto = new HeadersDto { Tenant = "tenant" };

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(Guid.Empty);

            // Act
            var result = await _userServices.Update(userUpdateDto, headersDto);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Update should throw exception when duplicated")]
        [Trait("Update", "Fail")]
        public async Task Update_ShouldThrowArgumentException_WhenDuplicatedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userIdDuplicated = Guid.NewGuid();
            var userUpdateDto = new UserUpdateDto
            {
                Id = userId,
                Name = "Nome",
                Email = "email@email.com"
            };
            var headersDto = new HeadersDto { Tenant = "tenant" };
            var user = new User(userIdDuplicated, "Nome", "email@email.com", true, DateTime.Now);

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(userId);

            _userRepositoryMock
                .Setup(repo => repo.FindByEmailAsync(userUpdateDto.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _userServices.Update(userUpdateDto, headersDto));
            Assert.Equal("Duplicated user", ex.Message);
        }

        [Theory(DisplayName = "Create should throw when required fields are missing")]
        [Trait("CreateUser", "Validation")]
        [InlineData("", "valid@email.com")]
        [InlineData("Valid Name", "")]
        [InlineData("", "")]
        public async Task Create_ShouldThrowArgumentException_WhenNameOrEmailIsEmpty(string name, string email)
        {
            // Arrange
            var dto = new UserCreateDto { Name = name, Email = email, Password = "Password123" };
            var headers = new HeadersDto { Tenant = "tenant" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userServices.Create(dto, headers));

            Assert.Equal("Data cannot be empty", exception.Message);
        }

        [Fact(DisplayName = "Create should reactivate existing user")]
        [Trait("CreateUser", "Reactivation")]
        public async Task Create_ShouldReactivateUser_WhenUserAlreadyExists()
        {
            // Arrange
            var dto = new UserCreateDto { Name = "Reactivated User", Email = "reactivate@test.com", Password = "Password123", TeamIds = [] };
            var headers = new HeadersDto { Tenant = "tenant" };
            var userId = Guid.NewGuid();

            var existingUser = new User(userId, "Old Name", "old@email.com", false, DateTime.Now);

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(userId);

            _userRepositoryMock
                .Setup(repo => repo.FindByEmailAsync(dto.Email))
                .ReturnsAsync(existingUser);

            _userRepositoryMock
                .Setup(repo => repo.Update(It.IsAny<User>()))
                .Returns(true);

            // Act
            var result = await _userServices.Create(dto, headers);

            // Assert
            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.Update(It.Is<User>(u =>
                u.Email == dto.Email && u.Name == dto.Name)), Times.Once);
        }

        [Fact(DisplayName = "Create should throw exception when user is duplicated")]
        [Trait("CreateUser", "Fail")]
        public async Task Create_ShouldThrowAppException_WhenUserIsDuplicated()
        {
            // Arrange
            var dto = new UserCreateDto { Name = "name", Email = "email", Password = "Password123" };
            var headers = new HeadersDto { Tenant = "tenant" };
            var userId = Guid.NewGuid();
            var existingUser = new User(userId, "Old Name", "old@email.com", true, DateTime.Now);

            _userRepositoryMock
                .Setup(repo => repo.FindByEmailAsync(dto.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _userServices.Create(dto, headers));

            Assert.Equal("Duplicated user", exception.Message);
        }

        [Fact(DisplayName = "Create should assign teams when TeamIds are present")]
        [Trait("CreateUser", "Teams")]
        public async Task Create_ShouldAssignTeams_WhenTeamIdsArePresent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UserCreateDto
            {
                Name = "Test User",
                Email = "test@email.com",
                Password = "password123",
                TeamIds = new List<int> { 1, 2 }
            };

            var headers = new HeadersDto { Tenant = "tenant" };

            var teams = new List<Team>
            {
                new Team("Team 1", 1, DateTime.Now),
                new Team("Team 2", 2, DateTime.Now)
            };

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(userId);

            _userRepositoryMock
                .Setup(repo => repo.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            _teamRepositoryMock
                .Setup(repo => repo.FindByIds(dto.TeamIds))
                .Returns(teams);

            _userRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userServices.Create(dto, headers);

            // Assert
            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.CreateAsync(It.Is<User>(u =>
                u.Teams.Count == 2)), Times.Once);
        }

        [Fact(DisplayName = "Create should assign profiles when ProfilesIds are present")]
        [Trait("CreateUser", "Profiles")]
        public async Task Create_ShouldAssignProfiles_WhenProfilesIdsArePresent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UserCreateDto
            {
                Name = "Test User",
                Email = "test@email.com",
                Password = "password123",
                TeamIds = new List<int>(),
            };

            var headers = new HeadersDto { Tenant = "tenant" };

            var teams = new List<Team>();

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(userId);

            _userRepositoryMock
                .Setup(repo => repo.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            _teamRepositoryMock
                .Setup(repo => repo.FindByIds(dto.TeamIds))
                .Returns(teams);

            _userRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userServices.Create(dto, headers);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Create should return false when marketplace returns Guid.Empty")]
        [Trait("CreateUser", "Marketplace Failure")]
        public async Task Create_ShouldReturnFalse_WhenMarketplaceReturnsEmptyGuid()
        {
            // Arrange
            var dto = new UserCreateDto { Name = "Test", Email = "fail@test.com", Password = "Password123", TeamIds = [] };
            var headers = new HeadersDto { Tenant = "tenant" };

            _marketPlaceApiMock
                .Setup(api => api.AssignLicensesByHub(It.IsAny<string>(), It.IsAny<RequestAssignLicensesByHub>()))
                .ReturnsAsync(Guid.Empty);

            // Act
            var result = await _userServices.Create(dto, headers);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "IsEmailInUseAsync should return true when email exists in repository")]
        [Trait("IsEmailInUseAsync", "Success")]
        public async Task IsEmailInUseAsync_ShouldReturnTrue_WhenEmailExistsInRepository()
        {
            // Arrange
            var userEmailDto = new UserEmailDto
            {
                Email = "existing@example.com",
                UserId = Guid.NewGuid()
            };

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(userEmailDto.Email, userEmailDto.UserId))
                .ReturnsAsync(true);

            // Act
            var result = await _userServices.IsEmailInUseAsync(userEmailDto);

            // Assert
            Assert.True(result);
            _userRepositoryMock.Verify(
                x => x.EmailExistsAsync(userEmailDto.Email, userEmailDto.UserId),
                Times.Once);
        }

        [Fact(DisplayName = "IsEmailInUseAsync should throw ArgumentException when email is empty")]
        [Trait("IsEmailInUseAsync", "Exception")]
        public async Task IsEmailInUseAsync_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            var userEmailDto = new UserEmailDto
            {
                Email = string.Empty,
                UserId = Guid.NewGuid()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _userServices.IsEmailInUseAsync(userEmailDto));

            Assert.Equal("Null or empty email", exception.Message);
            _userRepositoryMock.Verify(
                x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>()),
                Times.Never);
        }

        [Fact(DisplayName = "FindByTeamIds")]
        [Trait("FindByTeamIds", "Success")]
        public async Task FindByTeamIds_ShouldReturnUsers_WhenTeamIdsAreValid()
        {
            // Arrange
            var teamIds = new[] { 1, 2, 3 };
            var expectedUsers = new List<UserDto>
            {
                new UserDto { Id = Guid.NewGuid(), Name = "User 1", Email = "user1@a.com" },
                new UserDto { Id = Guid.NewGuid(), Name = "User 2", Email = "user2@a.com" }
            };

            _userRepositoryMock
                .Setup(r => r.FindByTeamIdsAsync(teamIds))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _userServices.FindByTeamIds(teamIds);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUsers.Count, result.Count);
            Assert.Equal(expectedUsers.First().Name, result.First().Name);
            _userRepositoryMock.Verify(r => r.FindByTeamIdsAsync(teamIds), Times.Once);
        }

        [Fact(DisplayName = "FindByTeamIds - Invalid Parameters")]
        [Trait("FindByTeamIds", "Exception")]
        public async Task FindByTeamIds_ShouldThrowArgumentException_WhenTeamIdsIsNullOrEmpty()
        {
            // Arrange
            int[]? nullIds = null;
            var emptyIds = Array.Empty<int>();

            // Act & Assert - caso nulo
            await Assert.ThrowsAsync<ArgumentException>(() => _userServices.FindByTeamIds(nullIds!));

            // Act & Assert - caso vazio
            await Assert.ThrowsAsync<ArgumentException>(() => _userServices.FindByTeamIds(emptyIds));
        }

    }
}

