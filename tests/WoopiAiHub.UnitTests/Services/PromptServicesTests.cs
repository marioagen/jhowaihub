using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Application.Utils;
using Newtonsoft.Json;
using WoopiAiHub.UnitTests.Fixture;
using Moq.AutoMock;

namespace WoopiAiHub.UnitTests.Services
{
    public class PromptServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly PromptServices _promptServices;

        public PromptServicesTests ()
        {
            _mocker = new AutoMocker();
            _promptServices = _mocker.CreateInstance<PromptServices>();
        }

        [Fact(DisplayName = "Create unique prompt success")]
        [Trait("Create", "Success")]
        public void CreateUniquePrompt_Success()
        {
            //Arrange
            var dto = new PromptCreateDto { Name = "Teste", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var userServices = _mocker.GetMock<IUserServices>();
            var validatePrompt = _mocker.GetMock<IValidatePrompt>();
            var promptRepository = _mocker.GetMock<IPromptRepository>();

            userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>())).Returns(true);
            promptRepository.Setup(r => r.CreateUniquePrompt(It.IsAny<Prompt>())).Returns(true);

            //Act
            var result = _promptServices.CreateUniquePrompt(dto, email);

            //Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Create unique prompt should throw app exception when is duplicated")]
        [Trait("Create", "Fail")]
        public void CreateUniquePrompt_ShouldThrowAppException_Duplicated()
        {
            //Arrange
            var dto = new PromptCreateDto { Name = "Teste", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var userServices = _mocker.GetMock<IUserServices>();
            var validatePrompt = _mocker.GetMock<IValidatePrompt>();
            var promptRepository = _mocker.GetMock<IPromptRepository>();

            userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>())).Returns(true);
            promptRepository.Setup(r => r.CreateUniquePrompt(It.IsAny<Prompt>())).Returns(false);

            //Act/Assert
            Assert.Throws<AppException>(() => _promptServices.CreateUniquePrompt(dto, email));
        }

        [Fact(DisplayName = "Create unique prompt should returns false when data is empty")]
        [Trait("Create", "Fail")]
        public void CreateUniquePrompt_ShouldReturnFalse_Empty()
        {
            //Arrange
            var dto = new PromptCreateDto { Name = "Teste", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var _userServices = _mocker.GetMock<IUserServices>();
            var _validatePrompt = _mocker.GetMock<IValidatePrompt>();
            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>())).Returns(false);

            //Act
            var result = _promptServices.CreateUniquePrompt(dto, email);

            //Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Create prompt success")]
        [Trait("Update", "Success")]
        public void Update_Success()
        {
            //Arrange
            var dto = new PromptUpdateDto { Id = 1, Name = "Novo", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var promptDto = new PromptDto { Id = 1, Name = "Antigo", Description = "Desc", Text = "Texto", IdUser = Guid.NewGuid(), Created = DateTime.Now };
            var _validatePrompt = _mocker.GetMock<IValidatePrompt>();
            var _promptRepository = _mocker.GetMock<IPromptRepository>();
            var _unitOfWork = _mocker.GetMock<IUnitOfWork>();


            _validatePrompt.Setup(v => v.ValidateOwnership(1, email));
            _promptRepository.Setup(r => r.FindById(1)).Returns(promptDto);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>()));
            _promptRepository.Setup(r => r.Update(It.IsAny<Prompt>())).Returns(true);
            _unitOfWork.Setup(u => u.BeginTransaction());
            _unitOfWork.Setup(u => u.Commit());

            //Act
            var result = _promptServices.Update(dto, email);

            //Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Update prompt should throw argumentException when is not found")]
        [Trait("Update", "Fail")]
        public void Update_ShouldThrowArgumentException_PromptNotFound()
        {
            //Arrange
            var dto = new PromptUpdateDto { Id = 1, Name = "Novo", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var _validatePrompt = _mocker.GetMock<IValidatePrompt>();
            var _promptRepository = _mocker.GetMock<IPromptRepository>();

            _validatePrompt.Setup(v => v.ValidateOwnership(1, email));
            _promptRepository.Setup(r => r.FindById(1)).Returns((PromptDto)null);

            //Act/Assert
            Assert.Throws<ArgumentException>(() => _promptServices.Update(dto, email));
        }

        [Fact(DisplayName = "Delete prompts by ids success")]
        [Trait("Delete", "Success")]
        public void DeleteByIds_Success()
        {
            //Arrange
            var ids = new List<int> { 1, 2 };
            var _promptRepository = _mocker.GetMock<IPromptRepository>();
            _promptRepository.Setup(r => r.Delete(ids)).Returns(true);

            //Act
            var result = _promptServices.DeleteByIds(ids);

            //Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Delete prompts by should throw argumentException")]
        [Trait("Delete", "Success")]
        public void DeleteByIds_ShouldThrowArgumentException()
        {
            //Arrange
            var ids = new List<int> { 1, 2 };
            var _promptRepository = _mocker.GetMock<IPromptRepository>();
            _promptRepository.Setup(r => r.Delete(ids)).Returns(false);

            //Act/Assert
            Assert.Throws<ArgumentException>(() => _promptServices.DeleteByIds(ids));
        }

        [Fact(DisplayName = "Find prompts by id success")]
        [Trait("FindById", "Success")]
        public void FindById_Success()
        {
            //Arrange
            var promptDto = new PromptDto { Id = 1, Name = "Teste", Description = "Desc", Text = "Texto", IdUser = Guid.NewGuid(), Created = DateTime.Now };
            var _promptRepository = _mocker.GetMock<IPromptRepository>();
            _promptRepository.Setup(r => r.FindById(1)).Returns(promptDto);

            //Act
            var result = _promptServices.FindById(1);

            //Assert
            Assert.Equal(promptDto, result);
        }

        [Fact(DisplayName = "Find prompts by id should throw argument exception")]
        [Trait("FindById", "Fail")]
        public void FindById_ShouldThrowArgumentException()
        {
            //Arrange
            var _promptRepository = _mocker.GetMock<IPromptRepository>();
            _promptRepository.Setup(r => r.FindById(1)).Returns((PromptDto)null);

            //Act/Assert
            Assert.Throws<ArgumentException>(() => _promptServices.FindById(1));
        }

        [Fact(DisplayName = "Find all prompts success")]
        [Trait("FindAll", "Success")]
        public void FindAll_Success()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var queryable = new List<PromptDto> { new PromptDto { Id = 1, Name = "Teste", Description = "Desc", Text = "Texto", IdUser = idUser, Created = DateTime.Now } }.AsQueryable();
            var _userServices = _mocker.GetMock<IUserServices>();
            var _promptRepository = _mocker.GetMock<IPromptRepository>();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindAllWithOwnerStatus(idUser)).Returns(queryable);

            //Act
            var result = _promptServices.FindAll(email);

            //Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact(DisplayName = "Find all prompts should throw argumentException when prompt is not found")]
        [Trait("FindAll", "Fail")]
        public void FindAll_ShouldThrowArgumentException_PromptNotFound()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var _userServices = _mocker.GetMock<IUserServices>();
            var _promptRepository = _mocker.GetMock<IPromptRepository>();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindAllWithOwnerStatus(idUser)).Returns((IQueryable<PromptDto>)null);

            //Act/Assert
            Assert.Throws<ArgumentException>(() => _promptServices.FindAll(email));
        }

        [Fact(DisplayName = "Find all prompts should throw argumentException when user id is invalid")]
        [Trait("FindAll", "Fail")]
        public void FindAll_ShouldThrowArgumentException_InvalidUserId()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.Empty;
            var queryable = new List<PromptDto>().AsQueryable();
            var _promptRepository = new Mock<IPromptRepository>();
            var _userServices = new Mock<IUserServices>();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindAllWithOwnerStatus(idUser)).Returns(queryable);

            //Act/Assert
            Assert.Throws<ArgumentException>(() => _promptServices.FindAll(email));
        }

        [Fact(DisplayName = "Find all prompts paged should throw argumentException when user page is invalid")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_ShouldThrowArgumentException_InvalidPage()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var pagedDataDto = new PagedDataDto { Page = 0, PageSize = 10 };
            var _userServices = new Mock<IUserServices>();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);

            //Act/Assert
            Assert.Throws<ArgumentException>(() => _promptServices.FindAllPaged(pagedDataDto, email));
        }

        [Fact(DisplayName = "Find all prompts paged success")]
        [Trait("FindAllPaged", "Success")]
        public void FindAllPaged_Success()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 10 };
            var _userServices = new Mock<IUserServices>();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);

            //Act
             var result = _promptServices.FindAllPaged(pagedDataDto, email);

            //Assert
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Find prompts by idUser paged should throw argument exception when prompt is not found")]
        [Trait("FindByIdUserPaged", "Failure")]
        public void FindByIdUserPaged_ShouldThrowArgumentException_PromptNotFound()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 10 };
            var _userServices = new Mock<IUserServices>();
            var _promptRepository = new Mock<IPromptRepository>();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindByIdUser(idUser)).Returns((IQueryable<PromptDto>)null);

            //Act/Assert
            Assert.Throws<ArgumentException>(() => _promptServices.FindByIdUserPaged(pagedDataDto, email));
        }

        [Fact(DisplayName = "Find prompts by idUser paged should throw argument exception when userId is invalid")]
        [Trait("FindByIdUserPaged", "Failure")]
        public void FindByIdUserPaged_Failure_InvalidUserId()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.Empty;
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 10 };
            var queryable = new List<PromptDto>().AsQueryable();
            var _userServices = new Mock<IUserServices>();
            var _promptRepository = new Mock<IPromptRepository>();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindByIdUser(idUser)).Returns(queryable);

            //Act/Assert
            Assert.Throws<ArgumentException>(() => _promptServices.FindByIdUserPaged(pagedDataDto, email));
        }

        [Fact(DisplayName = "Find prompts by idUser paged success")]
        [Trait("FindByIdUserPaged", "Success")]
        public void FindByIdUserPaged_Success()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 10 };
            var queryable = new List<PromptDto>().AsQueryable();
            var _promptRepository = new Mock<IPromptRepository>();

            _mocker.GetMock<IUserServices>().
               Setup(s => s.FindIdByEmail(It.IsAny<string>()))
                      .Returns(Guid.NewGuid());

            _promptRepository.Setup(r => r.FindByIdUser(idUser)).Returns(queryable);

            //Act
            var result = _promptServices.FindByIdUserPaged(pagedDataDto, email);

            //Assert
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "ProcessChatCompletionResult success")]
        [Trait("ProcessChatCompletionResult", "Success")]
        public async Task ProcessChatCompletionResult_Success()
        {
            //Arrange
            var chatCompletionResponseDto = MessagingFixture.FindValidChatCompletionResponseDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var _stepToolExecutionRepository = new Mock<IStepToolExecutionRepository>();
            var _documentHistoryRepository = new Mock<IDocumentHistoryRepository>();

            _mocker.GetMock<IStepToolExecutionRepository>().
               Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);

            _documentHistoryRepository.Setup(r => r.Create(It.IsAny<DocumentHistory>())).Returns(true);

            //Act
            await _promptServices.ProcessChatCompletionResult(chatCompletionResponseDto);

            Assert.True(true);
        }

        [Fact(DisplayName = "ProcessChatCompletionResult should throw argument exception")]
        [Trait("ProcessChatCompletionResult", "Fail")]
        public async Task ProcessChatCompletionResult_Fail()
        {
            //Arrange
            var chatCompletionResponseDto = MessagingFixture.FindValidChatCompletionResponseDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var _stepToolExecutionRepository = new Mock<IStepToolExecutionRepository>();
            var _documentHistoryRepository = new Mock<IDocumentHistoryRepository>();
            _stepToolExecutionRepository.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(It.IsAny<StepToolExecution>);
            _documentHistoryRepository.Setup(r => r.Create(It.IsAny<DocumentHistory>())).Returns(true);

            //Act/Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _promptServices.ProcessChatCompletionResult(chatCompletionResponseDto));
        }
    }

}
