using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(TypeDocCollection))]
    public class TypeDocServicesTests
    {
        private readonly AutoMocker _mocker;
        public readonly TypeDocServices _typeDocServices;
        private readonly TypeDocFixture _fixture;
        public TypeDocServicesTests(TypeDocFixture typeDocFixture)
        {
            this._fixture = typeDocFixture;
            _mocker = new AutoMocker();
            _typeDocServices = _mocker.CreateInstance<TypeDocServices>();
        }

        [Fact(DisplayName = "CreateTypeDoc")]
        [Trait("CreateTypeDoc", "Success")]
        public void CreateTypeDoc_Success()
        {
            // Arrange
            var typeDocHeaderDto = TypeDocFixture.FindValidTypeDocHeaderDto();
            var typeDocCreateDto = TypeDocFixture.FindValidTypeDocCreateDto();
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            typeDocRepository.Setup(a => a.CreateUniqueTypeDoc(It.IsAny<TypeDoc>())).Returns(new Domain.DTOs.ResponseCreateTypeDto
            {
                Duplicated = false
            });

            // Act
            var result = _typeDocServices.CreateUniqueTypeDoc(typeDocCreateDto,
                                                              typeDocHeaderDto);

            // Assert
            Assert.True(result.Duplicated == false);
            typeDocRepository.Verify(a => a.CreateUniqueTypeDoc(It.IsAny<TypeDoc>()), Times.Once);
        }

        [Fact(DisplayName = "CreateTypeDoc")]
        [Trait("CreateTypeDoc", "Duplicate")]
        public void CreateTypeDoc_Duplicate()
        {
            // Arrange
            var typeDocHeaderDto = TypeDocFixture.FindValidTypeDocHeaderDto();
            var typeDocCreateDto = TypeDocFixture.FindValidTypeDocCreateDto();
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            typeDocRepository.Setup(a => a.CreateUniqueTypeDoc(It.IsAny<TypeDoc>())).Returns(new Domain.DTOs.ResponseCreateTypeDto
            {
                Duplicated = true
            });

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _typeDocServices.CreateUniqueTypeDoc(typeDocCreateDto,
                                                                                        typeDocHeaderDto));
            typeDocRepository.Verify(a => a.CreateUniqueTypeDoc(It.IsAny<TypeDoc>()), Times.Once);
        }

        [Fact(DisplayName = "FindByName")]
        [Trait("FindByName", "Success")]
        public void FindByName_Success()
        {
            // Arrange
            var typeDoc = _fixture.FindValidTypeDocList().First();
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            typeDocRepository.Setup(a => a.FindByName(It.IsAny<string>())).Returns(typeDoc);

            // Act
            var result = _typeDocServices.FindByName("name");

            // Assert
            Assert.NotNull(result);
            typeDocRepository.Verify(a => a.FindByName(It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "FindAll")]
        [Trait("FindAll", "Success")]
        public void FindAll_Success()
        {
            // Arrange
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            var typeDoc = _fixture.FindValidTypeDocList();
            typeDocRepository.Setup(a => a.FindAll()).Returns(typeDoc);

            // Act
            var result = _typeDocServices.FindAll();

            // Assert
            Assert.NotNull(result);
            typeDocRepository.Verify(a => a.FindAll(), Times.Once);
        }

        [Fact(DisplayName = "FindAll")]
        [Trait("FindAll", "Fail")]
        public void FindAll_Fail()
        {
            // Arrange
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            var typeDoc = _fixture.FindValidTypeDocList();
            typeDocRepository.Setup(a => a.FindAll()).Returns((ICollection<TypeDoc>)null);

            // Act
            var result = _typeDocServices.FindAll();

            // Assert
            Assert.Null(result);
            typeDocRepository.Verify(a => a.FindAll(), Times.Once);
        }

        [Fact(DisplayName = "Delete")]
        [Trait("Delete", "Success")]
        public void DeleteByIds_Success()
        {
            // Arrange
            List<int> ids = new List<int> { 1, 2, 3 };
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            typeDocRepository.Setup(a => a.DeleteByIds(ids)).Returns(true);

            // Act
            var result = _typeDocServices.DeleteByIds(ids);

            // Assert
            Assert.True(result);
            typeDocRepository.Verify(a => a.DeleteByIds(ids), Times.Once);
        }

        [Fact(DisplayName = "Delete")]
        [Trait("DeleteByIds", "Fail")]
        public void DeleteByIds_Fail()
        {
            // Arrange
            List<int> ids = new List<int> { 1, 2, 3 };
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            typeDocRepository.Setup(a => a.DeleteByIds(ids)).Returns(false);

            // Act
            var result = _typeDocServices.DeleteByIds(ids);

            // Assert
            Assert.False(result);
            typeDocRepository.Verify(a => a.DeleteByIds(ids), Times.Once);
        }

        [Fact(DisplayName = "Update")]
        [Trait("Update", "Success")]
        public void Update_Success()
        {
            // Arrange
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            var updateTypeDocDto = TypeDocFixture.FindValidUpdateTypeDocDto();
            typeDocRepository.Setup(a => a.Update(updateTypeDocDto)).Returns(true);

            // Act
            var result = _typeDocServices.Update(updateTypeDocDto);

            // Assert
            Assert.True(result);
            typeDocRepository.Verify(a => a.Update(updateTypeDocDto), Times.Once);
        }

        [Fact(DisplayName = "Update duplicate")]
        [Trait("Update", "Duplicate")]
        public void Upload_Duplicate()
        {
            // Arrange
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            var updateTypeDocDto = TypeDocFixture.FindValidUpdateTypeDocDto();
            updateTypeDocDto.Name = "Name";

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _typeDocServices.Update(updateTypeDocDto));
        }

        [Theory(DisplayName = "FindAllPaged")]
        [Trait("FindAllPaged", "Success")]
        [InlineData(0)]
        [InlineData(1)]
        public void FindAllPaged_Success(int pageSize)
        {
            // Arrange
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            var typeDocPagedDataDto = TypeDocFixture.FindValidTypeDocPagedDataDto(pageSize);
            var typeDocDto = TypeDocFixture.FindValidTypeDocDto(pageSize);
            typeDocRepository.Setup(a => a.FindAllPaged(typeDocPagedDataDto)).Returns(typeDocDto.AsQueryable());

            // Act
            var result = _typeDocServices.FindAllPaged(typeDocPagedDataDto);

            // Assert
            Assert.NotNull(result);
            typeDocRepository.Verify(a => a.FindAllPaged(typeDocPagedDataDto), Times.Once);
        }

        [Fact(DisplayName = "FindAllPaged")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_Fail()
        {
            // Arrange
            var typeDocRepository = _mocker.GetMock<ITypeDocRepository>();
            var typeDocPagedDataDto = _fixture.FindInvalidTypeDocPagedDataDto();

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _typeDocServices.FindAllPaged(typeDocPagedDataDto));
        }
    }
}
