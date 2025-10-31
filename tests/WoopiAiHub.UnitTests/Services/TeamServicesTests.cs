using Moq;
using Moq.AutoMock;
using System.Collections.Generic;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using WoopiAiHub.UnitTests.Helpers;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(TeamCollection))]
    public class TeamServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly TeamFixture _fixture;
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly TeamServices _service;

        public TeamServicesTests()
        {
            _fixture = new TeamFixture();
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<TeamServices>();
            _teamRepositoryMock = _mocker.GetMock<ITeamRepository>();
            _userRepositoryMock = _mocker.GetMock<IUserRepository>();
        }

        [Fact(DisplayName = "FindById should return TeamDto when ID exists")]
        [Trait("FindById", "Success")]
        public void FindById_ExistingId_ReturnsTeamDto()
        {
            // Arrange
            var teamDto = _fixture.CreateValidTeamDto();
            _teamRepositoryMock.Setup(r => r.FindById(teamDto.Id)).Returns(teamDto);

            // Act
            var result = _service.FindById(teamDto.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teamDto.Id, result.Id);
        }

        [Fact(DisplayName = "FindById should throw ArgumentException when ID does not exist")]
        [Trait("FindById", "Failure/Exception")]
        public void FindById_NonExistingId_ThrowsArgumentException()
        {
            // Arrange
            TeamDto? teamDto = null;
            _teamRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(teamDto);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.FindById(999));
        }

        [Fact(DisplayName = "CreateUniqueTeam should retunr true when success")]
        [Trait("CreateUniqueTeam", "Sucess")]
        public async Task CreateUniqueTeam_ShouldReturnTrue_WhenSuccess()
        {
            // Arrange
            var teamCreateDto = new TeamCreateDto
            {
                Name = "Equipe Teste",
                UserIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            };

            var users = new List<User>
            {
                new User(teamCreateDto.UserIds[0], "User1", "user1@email.com", true, DateTime.Now),
                new User(teamCreateDto.UserIds[1], "User2", "user2@email.com", true, DateTime.Now)
            };

            _userRepositoryMock
                .Setup(repo => repo.FindByIdsAsync(teamCreateDto.UserIds))
                .ReturnsAsync(users);

            _teamRepositoryMock
                .Setup(repo => repo.CreateUniqueTeam(It.IsAny<Team>()))
                .Returns(true);

            // Act
            var result = await _service.CreateUniqueTeam(teamCreateDto);

            // Assert
            Assert.True(result);
            _teamRepositoryMock.Verify(repo => repo.CreateUniqueTeam(It.IsAny<Team>()), Times.Once);
        }

        [Fact(DisplayName = "CreateUniqueTeam should throw argument exception when name is empty")]
        [Trait("CreateUniqueTeam", "Failure/Exception")]
        public async Task CreateUniqueTeam_ShouldThrowArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            var teamCreateDto = new TeamCreateDto
            {
                Name = "",
                UserIds = new List<Guid> { Guid.NewGuid() }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateUniqueTeam(teamCreateDto));
            Assert.Equal("Team name cannot be empty", ex.Message);
        }
        [Fact(DisplayName = "CreateUniqueTeam should throw argument exception when name is duplicated")]
        [Trait("CreateUniqueTeam", "Failure/Exception")]
        public async Task CreateUniqueTeam_ShouldThrowArgumentException_WhenDuplicatedName()
        {
            // Arrange
            var teamCreateDto = new TeamCreateDto
            {
                Name = "Equipe Duplicada",
                UserIds = new List<Guid> { Guid.NewGuid() }
            };

            var users = new List<User>
            {
                new User(teamCreateDto.UserIds[0], "User1", "user1@email.com", true, DateTime.Now)
            };

            _userRepositoryMock
                .Setup(repo => repo.FindByIdsAsync(teamCreateDto.UserIds))
                .ReturnsAsync(users);

            _teamRepositoryMock
                .Setup(repo => repo.CreateUniqueTeam(It.IsAny<Team>()))
                .Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateUniqueTeam(teamCreateDto));
            Assert.Equal("Duplicated Team Name", ex.Message);
        }

        [Fact(DisplayName = "Update should return true when update succeeds")]
        [Trait("Update", "Success")]
        public async Task Update_ShouldReturnTrue_WhenUpdateSucceeds()
        {
            // Arrange
            var teamId = 1;
            var teamUpdateDto = new TeamUpdateDto
            {
                Id = teamId,
                Name = "Novo Nome",
                UserIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            var team = new Team("Antigo Nome", teamId, DateTime.Now)
            {
                Users = new List<User>()
            };

            var users = new List<User>
            {
                new User(teamUpdateDto.UserIds[0], "User1", "user1@email.com", true, DateTime.Now),
                new User(teamUpdateDto.UserIds[1], "User2", "user2@email.com", true, DateTime.Now)
            };

            _teamRepositoryMock.Setup(r => r.FindByIdReturnModel(teamId)).Returns(team);
            _userRepositoryMock.Setup(r => r.FindByIdsAsync(teamUpdateDto.UserIds)).ReturnsAsync(users);
            _teamRepositoryMock.Setup(r => r.Update(team)).Returns(true);

            // Act
            var result = await _service.Update(teamUpdateDto);

            // Assert
            Assert.True(result);
            Assert.Equal("Novo Nome", team.Name);
            Assert.Equal(2, team.Users.Count);
        }

        [Fact(DisplayName = "Update should return fakse when team is not found")]
        [Trait("Update", "Fail")]
        public async Task Update_ShouldReturnFalse_WhenTeamNotFound()
        {
            // Arrange
            var teamUpdateDto = new TeamUpdateDto
            {
                Id = 99,
                Name = "Nome",
                UserIds = new List<Guid> { Guid.NewGuid() }
            };

            _teamRepositoryMock.Setup(r => r.FindByIdReturnModel(teamUpdateDto.Id)).Returns((Team)null);

            // Act
            var result = await _service.Update(teamUpdateDto);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Update should throw exception when team is duplicated")]
        [Trait("Update", "Fail")]
        public async Task Update_ShouldThrowArgumentException_WhenDuplicatedTeam()
        {
            // Arrange
            var teamId = 1;
            var teamUpdateDto = new TeamUpdateDto
            {
                Id = teamId,
                Name = "Duplicado",
                UserIds = new List<Guid> { Guid.NewGuid() }
            };

            var team = new Team("Antigo Nome", teamId, DateTime.Now)
            {
                Users = new List<User>()
            };

            var users = new List<User>
        {
            new User(teamUpdateDto.UserIds[0], "User1", "user1@email.com", true, DateTime.Now)
        };

            _teamRepositoryMock.Setup(r => r.FindByIdReturnModel(teamId)).Returns(team);
            _userRepositoryMock.Setup(r => r.FindByIdsAsync(teamUpdateDto.UserIds)).ReturnsAsync(users);
            _teamRepositoryMock.Setup(r => r.Update(team)).Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.Update(teamUpdateDto));
            Assert.Equal("Duplicated Team Name", ex.Message);
            Assert.Equal(ErrorCode.Duplicated, ex.ErrorCode);
        }

        [Fact(DisplayName = "DeleteByIds should return true when IDs are valid")]
        [Trait("DeleteByIds", "Success")]
        public void DeleteByIds_ValidIds_ReturnsTrue()
        {
            // Arrange
            var ids = new List<int> { 1, 2, 3 };
            var teams = new List<Team>();
            _teamRepositoryMock.Setup(r => r.FindByIds(ids)).Returns(teams);
            _teamRepositoryMock.Setup(r => r.DeleteByIds(ids)).Returns(true);

            // Act
            var result = _service.DeleteByIds(ids);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "DeleteByIds should return false when IDs do not exist")]
        [Trait("DeleteByIds", "Failure")]
        public void DeleteByIds_InvalidIds_ReturnsFalse()
        {
            // Arrange
            var ids = new List<int> { 999 };
            var teams = new List<Team>();
            _teamRepositoryMock.Setup(r => r.FindByIds(ids)).Returns(teams);
            _teamRepositoryMock.Setup(r => r.DeleteByIds(ids)).Returns(false);

            // Act
            var result = _service.DeleteByIds(ids);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "FindAllPaged should return paged result correctly (first page, ascending)")]
        [Trait("FindAllPaged", "Success - Page 1, ascending")]
        public void FindAllPaged_FirstPageAscending_ReturnsPagedResult()
        {
            // Arrange
            var pagedData = new PagedDataDto { Page = 1, PageSize = 2, IsAscending = true };
            var teamDtos = new List<TeamDto>
            {
                _fixture.CreateValidTeamDto(),
                _fixture.CreateValidTeamDto(),
                _fixture.CreateValidTeamDto()
            }.AsQueryable();

            _teamRepositoryMock.Setup(r => r.FindAllPaged(pagedData)).Returns(teamDtos);

            // Act
            var result = _service.FindAllPaged(pagedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.PageCount);
            Assert.Equal(3, result.RowCount);
            Assert.Equal(2, result.Content.Count());
        }

        [Fact(DisplayName = "FindAllPaged should return paged result correctly (last page, descending)")]
        [Trait("FindAllPaged", "Success - Last page, descending")]
        public void FindAllPaged_LastPageDescending_ReturnsPagedResult()
        {
            // Arrange
            var pagedData = new PagedDataDto { Page = 2, PageSize = 2, IsAscending = false };
            var teams = new List<TeamDto>
            {
                _fixture.CreateValidTeamDto(),
                _fixture.CreateValidTeamDto(),
                _fixture.CreateValidTeamDto()
            }.AsQueryable();

            _teamRepositoryMock.Setup(r => r.FindAllPaged(pagedData)).Returns(teams);

            // Act
            var result = _service.FindAllPaged(pagedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(2, result.PageCount);
            Assert.Equal(3, result.RowCount);
            Assert.Single(result.Content);
        }

        [Fact(DisplayName = "FindAllPaged should filter by search when Search is filled")]
        [Trait("FindAllPaged", "Search filter")]
        public void FindAllPaged_WithSearch_FiltersResults()
        {
            // Arrange
            var team1 = _fixture.CreateValidTeamDto();
            var team2 = _fixture.CreateValidTeamDto();
            team2.Name = "Special Team";
            var pagedData = new PagedDataDto { Page = 1, PageSize = 10, IsAscending = true, Search = "special" };
            var teamDtos = new List<TeamDto> { team1, team2 }.AsQueryable();

            _teamRepositoryMock.Setup(r => r.FindAllPaged(pagedData)).Returns(teamDtos);

            // Act
            var result = _service.FindAllPaged(pagedData);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Content);
            Assert.Contains(result.Content, t => t.Name.Contains("Special"));
        }

        [Fact(DisplayName = "FindAllPaged should throw ArgumentException when Page <= 0")]
        [Trait("FindAllPaged", "Failure/Exception")]
        public void FindAllPaged_PageZeroOrLess_ThrowsArgumentException()
        {
            // Arrange
            var pagedData = new PagedDataDto { Page = 0, PageSize = 10 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.FindAllPaged(pagedData));
        }

        [Fact(DisplayName = "FindAllPaged should return all items when PageSize = 0")]
        [Trait("FindAllPaged", "PageSize zero returns all")]
        public void FindAllPaged_PageSizeZero_ReturnsAllItems()
        {
            // Arrange
            var pagedData = new PagedDataDto { Page = 1, PageSize = 0, IsAscending = true };
            var teams = new List<TeamDto>
            {
                _fixture.CreateValidTeamDto(),
                _fixture.CreateValidTeamDto(),
                _fixture.CreateValidTeamDto()
            }.AsQueryable();

            _teamRepositoryMock.Setup(r => r.FindAllPaged(pagedData)).Returns(teams);

            // Act
            var result = _service.FindAllPaged(pagedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(1, result.PageCount);
            Assert.Equal(3, result.RowCount);
            Assert.Equal(3, result.Content.Count());
        }

        [Fact(DisplayName = "FindByIdsAndUser should return teams when all teams are found")]
        [Trait("FindByIdsAndUser", "Valid case")]
        public void FindByIdsAndUser_ShouldReturnTeams_WhenAllTeamsAreFound()
        {
            // Arrange
            var email = "user@example.com";
            var teams = new List<Team>
            {
                _fixture.CreateValidTeam(),
                _fixture.CreateValidTeam(),
                _fixture.CreateValidTeam()
            }.ToList();

            var ids = teams.Select(t => t.Id).ToList();

            _teamRepositoryMock.Setup(r => r.FindByIdsAndUser(ids, email))
                               .Returns(teams);

            // Act
            var result = _service.FindByIdsAndUser(ids, email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teams.Count, result.Count);
            Assert.All(result, team => Assert.Contains(team.Id, ids));
        }

        [Fact(DisplayName = "FindByIdsAndUser should throw when no teams are found")]
        [Trait("FindByIdsAndUser", "Validation")]
        public void FindByIdsAndUser_ShouldThrow_WhenNoTeamsAreFound()
        {
            // Arrange
            var email = "user@example.com";
            var teams = new List<Team>
            {
                _fixture.CreateValidTeam(),
                _fixture.CreateValidTeam(),
            }.ToList();

            var ids = teams.Select(t => t.Id).ToList();

            _teamRepositoryMock.Setup(r => r.FindByIdsAndUser(ids, email))
                               .Returns(new List<Team>());

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.FindByIdsAndUser(ids, email));
            Assert.Equal("No teams were found", ex.Message);
        }

        [Fact(DisplayName = "FindByIdsAndUser should throw when some teams are missing")]
        [Trait("FindByIdsAndUser", "Validation")]
        public void FindByIdsAndUser_ShouldThrow_WhenSomeTeamsAreMissing()
        {
            // Arrange
            var email = "user@example.com";
            var requestedIds = new List<int> { 1, 2, 3 };

            var foundTeams = new List<Team>
            {
                new Team("Team 1", 1, DateTime.Now),
               new Team("Team 2", 2, DateTime.Now),
            };

            _teamRepositoryMock
                .Setup(r => r.FindByIdsAndUser(requestedIds, email))
                .Returns(foundTeams);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _service.FindByIdsAndUser(requestedIds, email));

            Assert.Equal("Some teams were not found", ex.Message);
        }

        [Fact(DisplayName = "Tests FindByUser and returns a list os teams")]
        [Trait("FindByUser", "Success")]
        public async Task FindByUser_ShouldReturnTeamsForUser()
        {
            // Arrange
            var emailUser = "user@example.com";
            var teams = new List<TeamDto>
            {
                _fixture.CreateValidTeamDto(),
                _fixture.CreateValidTeamDto(),
            }.AsQueryable();

            var asyncTeams = new TestAsyncEnumerable<TeamDto>(teams);

            _teamRepositoryMock.Setup(repo => repo.FindAllByUser(It.IsAny<string>())).Returns(asyncTeams);

            // Act
            var result = await _service.FindByUser(emailUser);

            // Assert
            Assert.Equal(teams.ToList(), result);
            _teamRepositoryMock.Verify(repo => repo.FindAllByUser(It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "Tests FindByUser and returns an empty list os teams")]
        [Trait("FindByUser", "Success")]
        public async Task FindByUser_ShouldReturnEmptyList_WhenNoTeamsExistForUser()
        {
            // Arrange
            var emailUser = "user@example.com";
            var teams = new List<TeamDto>().AsQueryable();

            var asyncTeams = new TestAsyncEnumerable<TeamDto>(teams);

            _teamRepositoryMock.Setup(repo => repo.FindAllByUser(It.IsAny<string>())).Returns(asyncTeams);

            // Act
            var result = await _service.FindByUser(emailUser);

            // Assert
            Assert.Empty(result);
            _teamRepositoryMock.Verify(repo => repo.FindAllByUser(It.IsAny<string>()), Times.Once);
        }
    }
}