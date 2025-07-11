using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(QuestionCollection))]
    public class QuestionServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly QuestionFixture _fixture;
        private readonly QuestionServices _questionServices;

        public QuestionServicesTests(QuestionFixture questionFixture)
        {
            this._fixture = questionFixture;
            _mocker = new AutoMocker();
            _questionServices = _mocker.CreateInstance<QuestionServices>();
        }

        [Fact(DisplayName = "Create question Sucess")]
        [Trait("Create", "Success")]
        public void CreateQuestion_Success()
        {
            // Arrange
            var questionCreateDto = QuestionFixture.FindValidQuestionCreateDto();
            var headersDto = QuestionFixture.FindValidHeadersDto();
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            questionRepository.Setup(a => a.CreateUniqueQuestion(It.IsAny<Question>())).Returns(true);

            // Act
            var result = _questionServices.CreateUniqueQuestion(questionCreateDto,
                                                                headersDto);

            // Assert
            Assert.True(result);
            questionRepository.Verify(a => a.CreateUniqueQuestion(It.IsAny<Question>()), Times.Once);
        }

        [Fact(DisplayName = "Create question Duplicate")]
        [Trait("Create", "Duplicate")]
        public void CreateQuestion_Duplicate()
        {
            // Arrange
            var questionCreateDto = QuestionFixture.FindValidQuestionCreateDto();
            var headersDto = QuestionFixture.FindValidHeadersDto();
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            questionRepository.Setup(a => a.CreateUniqueQuestion(It.IsAny<Question>())).Returns(false);

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _questionServices.CreateUniqueQuestion(questionCreateDto,
                                                                                          headersDto));
            questionRepository.Verify(a => a.CreateUniqueQuestion(It.IsAny<Question>()), Times.Once);
        }

        [Fact(DisplayName = "Find all questions")]
        [Trait("FindAll", "Success")]
        public void FindAll_Success()
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var questionDto = _fixture.FindValidQuestionListDto();
            questionRepository.Setup(a => a.FindAll()).Returns(questionDto);

            // Act
            var result = _questionServices.FindAll();

            // Assert
            Assert.NotNull(result);
            questionRepository.Verify(a => a.FindAll(), Times.Once);
        }

        [Fact(DisplayName = "Find all questions")]
        [Trait("FindAll", "Fail")]
        public void FindAll_Fail()
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var questionDto = _fixture.FindValidQuestionListDto();
            questionRepository.Setup(a => a.FindAll()).Returns((ICollection<QuestionDto>)null);

            // Act
            var result = _questionServices.FindAll();

            // Assert
            Assert.Null(result);
            questionRepository.Verify(a => a.FindAll(), Times.Once);
        }

        [Fact(DisplayName = "Find questions by email and description")]
        [Trait("FindByDescriptionAndEmail", "Success")]
        public void FindByDescriptionAndEmail_Success()
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var questionDto = _fixture.FindValidQuestionDto();
            questionRepository.Setup(a => a.FindByDescriptionAndEmail("desc", "email")).Returns(questionDto);

            // Act
            var result = _questionServices.FindByDescriptionAndEmail("desc", "email");

            // Assert
            Assert.NotNull(result);
            questionRepository.Verify(a => a.FindByDescriptionAndEmail("desc", "email"), Times.Once);
        }

        [Fact(DisplayName = "Find questions by email and description")]
        [Trait("FindByDescriptionAndEmail", "Fail")]
        public void FindByDescriptionAndEmail_Fail()
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var questionDto = _fixture.FindValidQuestionDto();
            questionRepository.Setup(a => a.FindByDescriptionAndEmail("desc", "email")).Returns<QuestionDto>(null);

            // Act
            var result = _questionServices.FindByDescriptionAndEmail("desc", "email");

            // Assert
            Assert.Null(result);
            questionRepository.Verify(a => a.FindByDescriptionAndEmail("desc", "email"), Times.Once);
        }

        [Fact(DisplayName = "Find questions by id")]
        [Trait("FindById", "Success")]
        public void FindById_Success()
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var questionDto = _fixture.FindValidQuestionDto();
            questionRepository.Setup(a => a.FindById(1)).Returns(questionDto);

            // Act
            var result = _questionServices.FindById(1);

            // Assert
            Assert.NotNull(result);
            questionRepository.Verify(a => a.FindById(1), Times.Once);
        }

        [Fact(DisplayName = "Find questions by id ")]
        [Trait("FindById", "Fail")]
        public void FindById_Fail()
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            questionRepository.Setup(a => a.FindById(1)).Returns<QuestionDto>(null);

            // Act
            var result = _questionServices.FindById(1);

            // Assert
            Assert.Null(result);
            questionRepository.Verify(a => a.FindById(1), Times.Once);
        }

        [Fact(DisplayName = "Delete question by id")]
        [Trait("Delete", "Success")]
        public void Delete_Success()
        {
            // Arrange
            List<int> ids = new List<int> { 1, 2, 3 };
            List<int> questionnaireIds = new List<int> { 1, 2, 3 };
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            var questionnaireList = _fixture.FindValidQuestionnaireList();

            questionRepository.Setup(a => a.DeleteByIds(ids)).Returns(true);
            questionnaireRepository.Setup(a => a.FindByIds(ids)).Returns(questionnaireList);
            questionnaireRepository.Setup(a => a.FindByQuestionIds(ids)).Returns(questionnaireIds);
            questionnaireRepository.Setup(a => a.DeleteById(It.IsAny<int>())).Returns(true);

            // Act
            var result = _questionServices.DeleteByIds(ids);

            // Assert
            Assert.True(result);
            questionRepository.Verify(a => a.DeleteByIds(ids), Times.Once);
            questionnaireRepository.Verify(a => a.FindByIds(ids), Times.Once);
            questionnaireRepository.Verify(a => a.FindByQuestionIds(ids), Times.Once);
            questionnaireRepository.Verify(a => a.DeleteById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Delete question by id")]
        [Trait("Delete", "Fail")]
        public void Delete_Fail()
        {
            // Arrange
            List<int> ids = new List<int> { 1, 2, 3 };
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            questionRepository.Setup(a => a.DeleteByIds(ids)).Returns(false);

            // Act
            var result = _questionServices.DeleteByIds(ids);

            // Assert
            Assert.False(result);
            questionRepository.Verify(a => a.DeleteByIds(ids), Times.Once);
        }

        [Fact(DisplayName = "Update questions")]
        [Trait("Update", "Success")]
        public void Update_Success()
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var updateQuestionDto = QuestionFixture.FindValidUpdateQuestionDto();
            questionRepository.Setup(a => a.Update(updateQuestionDto)).Returns(true);

            // Act
            var result = _questionServices.Update(updateQuestionDto);

            // Assert
            Assert.True(result);
            questionRepository.Verify(a => a.Update(updateQuestionDto), Times.Once);
        }

        [Fact(DisplayName = "Update questions duplicate")]
        [Trait("Update", "Duplicate")]
        public void Update_Duplicate()
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var updateQuestionDto = QuestionFixture.FindValidUpdateQuestionDto();
            updateQuestionDto.Description = "Title";

            // Act /Assert
            Assert.Throws<ArgumentException>(() => _questionServices.Update(updateQuestionDto));
        }

        [Theory(DisplayName = "FindAllPaged questions")]
        [Trait("FindAllPaged", "Success")]
        [InlineData("", 0)]
        [InlineData("Search", 1)]
        public void FindAllPaged_Success(string search, int pageSize)
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var questionDto = _fixture.FindValidQuestionListDto();
            var questionPagedDataDto = _fixture.FindValidQuestionPagedDataDto();
            questionPagedDataDto.PageSize = pageSize;
            questionPagedDataDto.Search = search;
            var questionPagedResultDto = _fixture.FindValidQuestionPagedResultDto();
            questionRepository.Setup(a => a.FindAllPaged(questionPagedDataDto)).Returns(questionDto.AsQueryable());

            // Act
            var result = _questionServices.FindAllPaged(questionPagedDataDto);

            // Assert
            Assert.NotNull(result);
            questionRepository.Verify(a => a.FindAllPaged(questionPagedDataDto), Times.Once);
        }

        [Fact(DisplayName = "FindAllPaged questions")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_Fail()
        {
            // Arrange
            var questionRepository = _mocker.GetMock<IQuestionRepository>();
            var questionDto = _fixture.FindValidQuestionListDto();
            var questionPagedDataDto = _fixture.FindInvalidQuestionPagedDataDto();
            var questionPagedResultDto = _fixture.FindValidQuestionPagedResultDto();
            questionRepository.Setup(a => a.FindAllPaged(questionPagedDataDto)).Returns(questionDto.AsQueryable());

            // Act /Assert
            Assert.Throws<ArgumentException>(() => _questionServices.FindAllPaged(questionPagedDataDto));
        }
    }
}
