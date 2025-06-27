using Xunit;
using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using WoopiAiHub.Domain.DTOs;
using Moq.AutoMock;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(TeamCollection))]
    public class TeamServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly TeamFixture _fixture;
        private readonly Mock<ITeamRepository> _teamRepositoryMock;
        private readonly TeamServices _service;

        public TeamServicesTests()
        {
            _fixture = new TeamFixture();            
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<TeamServices>();
            _teamRepositoryMock = _mocker.GetMock<ITeamRepository>();
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

        [Fact(DisplayName = "CreateUniqueTeam should return true when data is valid")]
        [Trait("CreateUniqueTeam", "Success")]
        public void CreateUniqueTeam_ValidData_ReturnsTrue()
        {
            // Arrange
            var dto = _fixture.CreateValidTeamCreateDto();
            _teamRepositoryMock.Setup(r => r.CreateUniqueTeam(It.IsAny<Team>())).Returns(true);

            // Act
            var result = _service.CreateUniqueTeam(dto);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "CreateUniqueTeam should throw ArgumentException when name already exists")]
        [Trait("CreateUniqueTeam", "Failure/Exception")]
        public void CreateUniqueTeam_DuplicateName_ThrowsArgumentException()
        {
            // Arrange
            var dto = _fixture.CreateValidTeamCreateDto();
            _teamRepositoryMock.Setup(r => r.CreateUniqueTeam(It.IsAny<Team>())).Returns(false);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.CreateUniqueTeam(dto));
        }

        [Fact(DisplayName = "CreateUniqueTeam should throw ArgumentException when name is empty")]
        [Trait("CreateUniqueTeam", "Failure/Exception")]
        public void CreateUniqueTeam_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var dto = _fixture.CreateValidTeamCreateDto();
            dto.Name = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.CreateUniqueTeam(dto));
        }

        [Fact(DisplayName = "Update should return true when data is valid")]
        [Trait("Update", "Success")]
        public void Update_ValidData_ReturnsTrue()
        {
            // Arrange
            var teamDto = _fixture.CreateValidTeamDto();
            var updateDto = _fixture.CreateValidTeamUpdateDto(teamDto.Id);

            _teamRepositoryMock.Setup(r => r.FindById(updateDto.Id)).Returns(teamDto);
            _teamRepositoryMock.Setup(r => r.Update(It.IsAny<Team>())).Returns(true);

            // Act
            var result = _service.Update(updateDto);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Update should throw ArgumentException when name already exists")]
        [Trait("Update", "Failure/Exception")]
        public void Update_DuplicateName_ThrowsArgumentException()
        {
            // Arrange
            var teamDto = _fixture.CreateValidTeamDto();
            var updateDto = _fixture.CreateValidTeamUpdateDto(teamDto.Id);

            _teamRepositoryMock.Setup(r => r.FindById(updateDto.Id)).Returns(teamDto);
            _teamRepositoryMock.Setup(r => r.Update(It.IsAny<Team>())).Returns(false);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.Update(updateDto));
        }

        [Fact(DisplayName = "Update should throw ArgumentException when team is not found")]
        [Trait("Update", "Failure/Exception")]
        public void Update_TeamNotFound_ThrowsArgumentException()
        {
            // Arrange
            TeamDto? teamDto = null;
            var updateDto = _fixture.CreateValidTeamUpdateDto(999);
            _teamRepositoryMock.Setup(r => r.FindById(updateDto.Id)).Returns(teamDto);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.Update(updateDto));
        }

        [Fact(DisplayName = "Update should throw ArgumentException when name is empty")]
        [Trait("Update", "Failure/Exception")]
        public void Update_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var teamDto = _fixture.CreateValidTeamDto();
            var updateDto = _fixture.CreateValidTeamUpdateDto(teamDto.Id);
            updateDto.Name = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.Update(updateDto));
        }

        [Fact(DisplayName = "DeleteByIds should return true when IDs are valid")]
        [Trait("DeleteByIds", "Success")]
        public void DeleteByIds_ValidIds_ReturnsTrue()
        {
            // Arrange
            var ids = new List<int> { 1, 2, 3 };
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

            var filtered = new List<TeamDto> { team2 }.AsQueryable();
            _teamRepositoryMock.Setup(r => r.FindAllPaged(pagedData)).Returns(filtered);

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
            Assert.Equal(1, result.PageCount);
            Assert.Equal(3, result.RowCount);
            Assert.Equal(3, result.Content.Count());
        }
    }
}