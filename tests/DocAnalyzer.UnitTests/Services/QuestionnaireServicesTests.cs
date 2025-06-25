using DocAnalyzer.Application.Services;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;
using DocAnalyzer.Domain.Enum;
using DocAnalyzer.Domain.Interfaces.Repository;
using DocAnalyzer.Domain.Interfaces.Services;
using DocAnalyzer.Domain.Models;
using DocAnalyzer.UnitTests.Fixture;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace DocAnalyzer.UnitTests.Services
{
    [Collection(nameof(QuestionnaireCollection))]
    public class QuestionnaireServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly QuestionnaireFixture _fixture;
        private readonly QuestionnaireServices _questionnaireServices;

        public QuestionnaireServicesTests(QuestionnaireFixture questionnaireFixture)
        {
            this._fixture = questionnaireFixture;
            _mocker = new AutoMocker();
            _questionnaireServices = _mocker.CreateInstance<QuestionnaireServices>();
        }

        [Fact(DisplayName = "Create questionnaire success")]
        [Trait("Create", "Success")]
        public void CreateQuestionnaire_Success()
        {
            // Arrange
            var createQuestionnaireDto = _fixture.FindValidCreateQuestionnaireDto();
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            questionnaireRepository.Setup(a => a.CreateUniqueQuestionnaire(It.IsAny<Questionnaire>())).Returns(true);

            // Act
            var result = _questionnaireServices.CreateUniqueQuestionnaire(createQuestionnaireDto, "email");

            // Assert
            Assert.True(result);
            questionnaireRepository.Verify(a => a.CreateUniqueQuestionnaire(It.IsAny<Questionnaire>()), Times.Once);
        }

        [Fact(DisplayName = "Create questionnaire Fail")]
        [Trait("Create", "Fail")]
        public void CreateQuestionnaire_Fail()
        {
            // Arrange
            var createQuestionnaireDto = _fixture.FindInvalidCreateQuestionnaireDto();

            // Act / Assert
            Assert.Throws<NullReferenceException>(() => _questionnaireServices.CreateUniqueQuestionnaire(createQuestionnaireDto, "email" ));
        }

        [Fact(DisplayName = "Create questionnaire Duplicate")]
        [Trait("Create", "Duplicate")]
        public void CreateQuestionnaire_Duplicate()
        {
            // Arrange
            var createQuestionnaireDto = _fixture.FindValidCreateQuestionnaireDto();
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            questionnaireRepository.Setup(a => a.CreateUniqueQuestionnaire(It.IsAny<Questionnaire>())).Returns(false);

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _questionnaireServices.CreateUniqueQuestionnaire(createQuestionnaireDto, "email" ));
            questionnaireRepository.Verify(a => a.CreateUniqueQuestionnaire(It.IsAny<Questionnaire>()), Times.Once);
        }

        [Fact(DisplayName = "Find all questionnaire")]
        [Trait("FindAll", "Success")]
        public void FindAll_Success()
        {
            // Arrange
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            var questionnaireDto = _fixture.FindValidQuestionnaireDtoList();
            questionnaireRepository.Setup(a => a.FindAll()).Returns(questionnaireDto);

            // Act
            var result = _questionnaireServices.FindAll();

            // Assert
            Assert.NotNull(result);
            questionnaireRepository.Verify(a => a.FindAll(), Times.Once);
        }

        [Fact(DisplayName = "Find all questionnaire")]
        [Trait("FindAll", "Fail")]
        public void FindAllByEmail_Fail()
        {
            // Arrange
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            questionnaireRepository.Setup(a => a.FindAll()).Returns((ICollection<QuestionnaireDto>)null);

            // Act
            var result = _questionnaireServices.FindAll();

            // Assert
            Assert.Null(result);
            questionnaireRepository.Verify(a => a.FindAll(), Times.Once);
        }

        [Fact(DisplayName = "Find questionnaire by id")]
        [Trait("FindById", "Success")]
        public void FindById_Success()
        {
            // Arrange
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            var questionnaireDto = _fixture.FindValidQuestionnaireDto();
            questionnaireRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(questionnaireDto);

            // Act
            var result = _questionnaireServices.FindById(It.IsAny<int>());

            // Assert
            Assert.NotNull(result);
            questionnaireRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Find questionnaire by id ")]
        [Trait("FindById", "Fail")]
        public void FindById_Fail()
        {
            // Arrange
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            questionnaireRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns<QuestionnaireDto>(null);

            // Act
            var result = _questionnaireServices.FindById(It.IsAny<int>());

            // Assert
            Assert.Null(result);
            questionnaireRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Delete questionnaire by id")]
        [Trait("Delete", "Success")]
        public void Delete_Success()
        {
            // Arrange
            List<int> ids = new List<int> { 1, 2, 3 };
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            questionnaireRepository.Setup(a => a.DeleteByIds(ids)).Returns(true);

            // Act
            var result = _questionnaireServices.DeleteByIds(ids);

            // Assert
            Assert.True(result);
            questionnaireRepository.Verify(a => a.DeleteByIds(ids), Times.Once);
        }

        [Fact(DisplayName = "Delete questionnaire by id")]
        [Trait("Delete", "Fail")]
        public void Delete_Fail()
        {
            // Arrange
            List<int> ids = new List<int> { 1, 2, 3 };
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            questionnaireRepository.Setup(a => a.DeleteByIds(ids)).Returns(false);

            // Act
            var result = _questionnaireServices.DeleteByIds(ids);

            // Assert
            Assert.False(result);
            questionnaireRepository.Verify(a => a.DeleteByIds(ids), Times.Once);
        }

        [Theory(DisplayName = "Update questionnaire")]
        [Trait("Update", "Success")]
        [InlineData(1)]
        [InlineData(2)]
        public void Update_Success(int questionId)
        {
            // Arrange
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            var questionQuestionnaireRepository = _mocker.GetMock<IQuestionQuestionnaireRepository>();
            var questionServices = _mocker.GetMock<IQuestionServices>();
            var updateQuestionnaireDto = QuestionnaireFixture.FindValidUpdateQuestionnaireDto(questionId);
            var questionnaireDto = _fixture.FindValidQuestionnaireDto();
            var questionnaireDtoList = _fixture.FindValidQuestionnaireDtoList();
            var questionDto = QuestionnaireFixture.FindInvalidQuestionDto();
            questionnaireRepository.Setup(a => a.Update(It.IsAny<Questionnaire>())).Returns(true);
            questionnaireRepository.Setup(a => a.FindById(updateQuestionnaireDto.Id)).Returns(questionnaireDto);
            questionServices.Setup(a => a.FindById(questionId)).Returns(questionDto);
            questionQuestionnaireRepository.Setup(a => a.Delete(It.IsAny<List<Question>>())).Returns(true);

            // Act
            var result = _questionnaireServices.Update(updateQuestionnaireDto);

            // Assert
            Assert.True(result);
            questionnaireRepository.Verify(a => a.FindById(updateQuestionnaireDto.Id), Times.Once);
            questionnaireRepository.Verify(a => a.Update(It.IsAny<Questionnaire>()), Times.Once);
        }

        [Fact(DisplayName = "Update questionnaire")]
        [Trait("Update", "Fail")]
        public void Update_Fail()
        {
            // Arrange
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            var updateQuestionnaireDto = QuestionnaireFixture.FindValidUpdateQuestionnaireDto(1);
            var questionnaireDto = _fixture.FindValidQuestionnaireDto();
            var questionnaireDtoList = _fixture.FindValidQuestionnaireDtoList();
            questionnaireRepository.Setup(a => a.FindById(updateQuestionnaireDto.Id)).Returns(questionnaireDto);
            questionnaireRepository.Setup(a => a.FindAll()).Returns(questionnaireDtoList);
            questionnaireRepository.Setup(a => a.Update(It.IsAny<Questionnaire>())).Returns(false);

            // Act/Assert
            Assert.Throws<ArgumentException>(() => _questionnaireServices.Update(updateQuestionnaireDto));
            questionnaireRepository.Verify(a => a.FindById(updateQuestionnaireDto.Id), Times.Once);
            questionnaireRepository.Verify(a => a.Update(It.IsAny<Questionnaire>()), Times.Once);
        }

        [Fact(DisplayName = "Update questionnaire duplicate")]
        [Trait("Update", "Duplicate")]
        public void Update_Duplicate()
        {
            // Arrange
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            var questionnaireDto = _fixture.FindValidQuestionnaireDto();
            var updateQuestionnaireDto = QuestionnaireFixture.FindValidUpdateQuestionnaireDto(1);
            updateQuestionnaireDto.Title = "Title";
            questionnaireRepository.Setup(a => a.FindById(updateQuestionnaireDto.Id)).Returns(questionnaireDto);
            var questionnaireDtoList = _fixture.FindValidQuestionnaireDtoList();
            questionnaireRepository.Setup(a => a.FindAll()).Returns(questionnaireDtoList);

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _questionnaireServices.Update(updateQuestionnaireDto));
        }

        [Theory(DisplayName = "FindAllPaged questionnaire")]
        [Trait("FindAllPaged", "Fail")]
        [InlineData(ColTypeQuestionnaire.TypeDoc, 0)]
        [InlineData(ColTypeQuestionnaire.Title, 1)]
        public void FindAllPaged_Success(ColTypeQuestionnaire colTypeQuestionnaire, int pageSize)
        {
            // Arrange
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            var questionnairePagedDataDto = QuestionnaireFixture.FindValidQuestionnairePagedData(colTypeQuestionnaire, pageSize);            
            var questionnaireDto = _fixture.FindValidQuestionnaireDtoList();
            questionnaireRepository.Setup(a => a.FindAllPaged(questionnairePagedDataDto)).Returns(questionnaireDto.AsQueryable());

            // Act 
            var result = _questionnaireServices.FindAllPaged(questionnairePagedDataDto);

            //Assert
            questionnaireRepository.Verify(a => a.FindAllPaged(questionnairePagedDataDto), Times.Once);
        }

        [Fact(DisplayName = "FindAllPaged questionnaire")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_Fail()
        {
            // Arrange
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            var questionnairePagedDataDto = _fixture.FindInvalidQuestionnairePagedData();
            var questionnaireDto = _fixture.FindValidQuestionnaireDtoList();
            questionnaireRepository.Setup(a => a.FindAllPaged(questionnairePagedDataDto)).Returns(questionnaireDto.AsQueryable());

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _questionnaireServices.FindAllPaged(questionnairePagedDataDto));
        }
    }
}
