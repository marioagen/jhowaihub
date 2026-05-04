using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using Xunit;
using WoopiAiHub.UnitTests.Fixture;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using System.Text.Json;

namespace WoopiAiHub.UnitTests.Services
{
    public class PromptServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly PromptServices _promptServices;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public PromptServicesTests()
        {
            _mocker = new AutoMocker();
            var mockPromptSettings = new Mock<IOptions<PromptSettings>>();
            mockPromptSettings.Setup(x => x.Value).Returns(new PromptSettings
            {
                TemplateFileName = "name.json",
                Folder = "folder"
            });
            var mockChatSettings = new Mock<IOptions<ChatCompletionSettings>>();
            mockChatSettings.Setup(x => x.Value).Returns(new ChatCompletionSettings
            {
                Model = "model",
                ApiVersion = "v1",
                MaxTokens = 100,
                Temperature = 0.5f
            });

            _mocker.Use(mockPromptSettings);
            _mocker.Use(mockChatSettings);

            var mocker = new AutoMocker();

            _unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _promptServices = _mocker.CreateInstance<PromptServices>();
        }

        [Fact(DisplayName = "Create unique prompt success")]
        [Trait("Create", "Success")]
        public void CreateUniquePrompt_Success()
        {
            //Arrange
            var dto = MessagingFixture.FindValidPromptCreateDto();
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
            var dto = MessagingFixture.FindValidPromptCreateDto();
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
            var dto = MessagingFixture.FindValidPromptCreateDto();
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
        public async Task Update_Success()
        {
            //Arrange
            var (dto, promptDto) = MessagingFixture.FindValidPromptUpdateDtoAndPromptDto();
            var email = "user@teste.com";
            var _validatePrompt = _mocker.GetMock<IValidatePrompt>();
            var _promptRepository = _mocker.GetMock<IPromptRepository>();
            var _unitOfWork = _mocker.GetMock<IUnitOfWork>();


            _validatePrompt.Setup(v => v.ValidateOwnership(promptDto.Id, email));
            _promptRepository.Setup(r => r.FindById(promptDto.Id)).Returns(promptDto);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>()));
            _promptRepository.Setup(r => r.UpdateAndRemovePromptApisFromPrompt(It.IsAny<Prompt>(), It.IsAny<List<int>>())).ReturnsAsync(true);
            _unitOfWork.Setup(u => u.BeginTransaction());
            _unitOfWork.Setup(u => u.Commit());

            //Act
            var result = await _promptServices.Update(dto, email);

            //Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Not update prompt with error")]
        [Trait("Update", "Fail")]
        public async Task Update_Fail_Save_Chages_Repository()
        {
            //Arrange
            var (dto, promptDto) = MessagingFixture.FindValidPromptUpdateDtoAndPromptDto();
            var email = "user@teste.com";
            var _validatePrompt = _mocker.GetMock<IValidatePrompt>();
            var _promptRepository = _mocker.GetMock<IPromptRepository>();
            var _unitOfWork = _mocker.GetMock<IUnitOfWork>();


            _validatePrompt.Setup(v => v.ValidateOwnership(promptDto.Id, email));
            _promptRepository.Setup(r => r.FindById(promptDto.Id)).Returns(promptDto);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>()));
            _promptRepository.Setup(r => r.UpdateAndRemovePromptApisFromPrompt(It.IsAny<Prompt>(), It.IsAny<List<int>>())).ReturnsAsync(false);
            _unitOfWork.Setup(u => u.BeginTransaction());
            _unitOfWork.Setup(u => u.Commit());

            //Act/Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _promptServices.Update(dto, email));
        }

        [Fact(DisplayName = "Update prompt should throw argumentException when is not found")]
        [Trait("Update", "Fail")]
        public async Task Update_ShouldThrowArgumentException_PromptNotFound()
        {
            //Arrange
            var (dto, promptDto) = MessagingFixture.FindValidPromptUpdateDtoAndPromptDto();
            var email = "user@teste.com";
            var _validatePrompt = _mocker.GetMock<IValidatePrompt>();
            var _promptRepository = _mocker.GetMock<IPromptRepository>();

            _validatePrompt.Setup(v => v.ValidateOwnership(promptDto.Id, email));
            _promptRepository.Setup(r => r.FindById(promptDto.Id)).Returns((PromptDto?)null);

            //Act/Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _promptServices.Update(dto, email));
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
            var (_, promptDto) = MessagingFixture.FindValidPromptUpdateDtoAndPromptDto();
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
            
            var (_, promptDto) = MessagingFixture.FindValidPromptUpdateDtoAndPromptDto();
            var queryable = new List<PromptDto> { promptDto }.AsQueryable();
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

            _mocker.GetMock<IUserServices>().Setup(s => s.FindIdByEmail(It.IsAny<string>()))
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

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(stepToolExecution);

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
            _stepToolExecutionRepository.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(It.IsAny<StepToolExecution>);
            _documentHistoryRepository.Setup(r => r.Create(It.IsAny<DocumentHistory>())).Returns(true);

            //Act/Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _promptServices.ProcessChatCompletionResult(chatCompletionResponseDto));
        }

        [Fact(DisplayName = "Find prompt templates success")]
        [Trait("FindPromptTemplates", "Success")]
        public async Task FindPromptTemplates_Success()
        {
            //Arrange            
            var templatesResponse = MessagingFixture.FindValidPromptTemplatesResponseSort();
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(templatesResponse);
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };

            _mocker.GetMock<IConfiguration>().Setup(c => c["RefitExternalSettings:FunctionApiKey"]).Returns("key");

            _mocker.GetMock<IFunctionFileRetriever>()
                .Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);

            //Act
            var result = await _promptServices.FindPromptTemplates("Text", null);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Theory(DisplayName = "Find prompt templates ordering")]
        [Trait("FindPromptTemplates", "Ordering")]
        [InlineData("name_asc")]
        [InlineData("name_desc")]
        [InlineData("created_asc")]
        [InlineData(null)]
        [InlineData("invalid")]
        public async Task FindPromptTemplates_Ordering(string? orderBy)
        {
            // Arrange
            var templatesResponse = MessagingFixture.FindValidPromptTemplatesResponseSort();
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(templatesResponse);
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };

            _mocker.GetMock<IConfiguration>()
                .Setup(c => c["RefitExternalSettings:FunctionApiKey"])
                .Returns("key");

            _mocker.GetMock<IFunctionFileRetriever>()
                .Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _promptServices.FindPromptTemplates(null, orderBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);

            switch (orderBy?.ToLower())
            {
                case "name_asc":
                    Assert.Equal(["A", "B", "C"], result.Select(x => x.Name));
                    break;

                case "name_desc":
                    Assert.Equal(["C", "B", "A"], result.Select(x => x.Name));
                    break;

                case "created_asc":
                    Assert.Equal(
                        [
                            new DateTime(2026, 1, 1),
                            new DateTime(2026, 1, 2),
                            new DateTime(2026, 1, 3)
                        ],
                        result.Select(x => x.Created)
                    );
                    break;

                default:
                    Assert.Equal(
                        new[]
                        {
                            new DateTime(2026, 1, 3),
                            new DateTime(2026, 1, 2),
                            new DateTime(2026, 1, 1)
                        },
                        result.Select(x => x.Created)
                    );
                    break;
            }
        }

        [Fact(DisplayName = "Find prompt templates should throw app exception when request fails")]
        [Trait("FindPromptTemplates", "Fail")]
        public async Task FindPromptTemplates_ShouldThrowAppException_WhenRequestFails()
        {
            //Arrange
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);

            _mocker.GetMock<IConfiguration>().Setup(c => c["RefitExternalSettings:FunctionApiKey"]).Returns("key");
            _mocker.GetMock<IOptions<PromptSettings>>().Setup(o => o.Value).Returns(new PromptSettings());
            _mocker.GetMock<IFunctionFileRetriever>()
                .Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);

            //Act & Assert
            await Assert.ThrowsAsync<AppException>(async () => await _promptServices.FindPromptTemplates(null, null));
        }

        [Fact(DisplayName = "Find prompt templates should return a empty list if any template has any prompt")]
        [Trait("FindPromptTemplates", "Success")]
        public async Task FindPromptTemplates_ShouldReturnEmptyList_WhenTheTemplatesNotHasAnyPrompt()
        {
            //Arrange            
            var templatesResponse = new PromptTemplatesResponse();
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(templatesResponse);
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };

            _mocker.GetMock<IConfiguration>().Setup(c => c["RefitExternalSettings:FunctionApiKey"]).Returns("key");

            _mocker.GetMock<IFunctionFileRetriever>()
                .Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);

            //Act
            var result = await _promptServices.FindPromptTemplates("Template", null);

            //Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact(DisplayName = "Import prompts by ids success")]
        [Trait("ImportPromptsByIds", "Success")]
        public async Task ImportPromptsByIds_Success()
        {
            //Arrange
            var promptId = Guid.NewGuid();
            var templateIds = new List<Guid> { promptId };
            var email = "test@example.com";
            var templatesResponse = MessagingFixture.FindValidPromptTemplatesResponse(promptId);
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(templatesResponse);
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };

            _mocker.GetMock<IConfiguration>().Setup(c => c["RefitExternalSettings:FunctionApiKey"]).Returns("key");
            _mocker.GetMock<IOptions<PromptSettings>>().Setup(o => o.Value).Returns(new PromptSettings());
            _mocker.GetMock<IFunctionFileRetriever>()
                .Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);
            _mocker.GetMock<IUserServices>().Setup(u => u.FindIdByEmail(email)).Returns(Guid.NewGuid());
            _mocker.GetMock<IPromptRepository>().Setup(r => r.CreateByRange(It.IsAny<List<Prompt>>())).Returns(true);

            //Act
            var result = await _promptServices.ImportPromptsByIds(templateIds, email);

            //Assert
            Assert.True(result);
        }
        
        [Fact(DisplayName = "Import prompts by ids should return false when the user not has a valid guid ")]
        [Trait("ImportPromptsByIds", "Fail")]
        public async Task ImportPromptsByIds_FailsIfTheListNotUserWithAValidGuid()
        {
            //Arrange
            var promptId = Guid.NewGuid();
            var templateIds = new List<Guid> { promptId };
            var email = "test@example.com";
            var templatesResponse = new PromptTemplatesResponse();
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(templatesResponse);
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };

            _mocker.GetMock<IConfiguration>().Setup(c => c["RefitExternalSettings:FunctionApiKey"]).Returns("key");
            _mocker.GetMock<IOptions<PromptSettings>>().Setup(o => o.Value).Returns(new PromptSettings());
            _mocker.GetMock<IFunctionFileRetriever>()
                .Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);
            _mocker.GetMock<IUserServices>().Setup(u => u.FindIdByEmail(email)).Returns(Guid.NewGuid());
            _mocker.GetMock<IPromptRepository>().Setup(r => r.CreateByRange(It.IsAny<List<Prompt>>())).Returns(true);

            //Act
            //Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _promptServices.ImportPromptsByIds(templateIds, email));
        }

        [Fact(DisplayName = "Import prompts by ids should return false when list is empty")]
        [Trait("ImportPromptsByIds", "Fail")]
        public async Task ImportPromptsByIds_ShouldReturnFalse_WhenListIsEmpty()
        {
            //Act
            var result = await _promptServices.ImportPromptsByIds(new List<Guid>(), "email");

            //Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Import prompts by ids should throw argument exception when templates not found")]
        [Trait("ImportPromptsByIds", "Fail")]
        public async Task ImportPromptsByIds_ShouldThrowArgumentException_WhenTemplatesNotFound()
        {
            //Arrange
            var templateIds = new List<Guid> { Guid.NewGuid() };
            var templatesResponse = new PromptTemplatesResponse
            {
                Prompts = new List<PromptTemplateDto>() // Empty
            };
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(templatesResponse);
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };

            _mocker.GetMock<IConfiguration>().Setup(c => c["RefitExternalSettings:FunctionApiKey"]).Returns("key");
            _mocker.GetMock<IOptions<PromptSettings>>().Setup(o => o.Value).Returns(new PromptSettings());
            _mocker.GetMock<IFunctionFileRetriever>()
                .Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(responseMessage);

            //Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _promptServices.ImportPromptsByIds(templateIds, "email"));
        }

        [Fact(DisplayName = "Find all basic prompts success")]
        [Trait("FindAllBasic", "Success")]
        public async Task FindAllBasic_Success()
        {
            //Arrange
            var expectedPrompts = MessagingFixture.FindValidPromptInternalDtoList();

            _mocker.GetMock<IPromptRepository>()
                .Setup(r => r.FindAllInternal())
                .ReturnsAsync(expectedPrompts);

            //Act
            var result = await _promptServices.FindAllInternal();

            //Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(expectedPrompts, result);
            _mocker.GetMock<IPromptRepository>().Verify(r => r.FindAllInternal(), Times.Once);
        }

        [Fact(DisplayName = "Find all basic prompts should return empty collection")]
        [Trait("FindAllBasic", "Success")]
        public async Task FindAllBasic_ShouldReturnEmptyCollection()
        {
            //Arrange
            var emptyPrompts = new List<PromptInternalDto>();

            _mocker.GetMock<IPromptRepository>()
                .Setup(r => r.FindAllInternal())
                .ReturnsAsync(emptyPrompts);

            //Act
            var result = await _promptServices.FindAllInternal();

            //Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mocker.GetMock<IPromptRepository>().Verify(r => r.FindAllInternal(), Times.Once);
        }

        [Fact(DisplayName = "AiPromptRefinement success")]
        [Trait("AiPromptRefinement", "Success")]
        public async Task AiPromptRefinement_Success()
        {
            //Arrange
            var prompt = "Minha regra de negócio";
            var tenantId = "tenantId";
            var email = "exemple@email.com";
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var chatCompletionResponse = new ChatCompletionResponseDto
            {
                Choices = new List<ChatChoiceDto>
                {
                    new ChatChoiceDto { Message = new ChatMessageResponseDto { Content = "Prompt refinado" } }
                }
            };
            _mocker.GetMock<IConfiguration>().Setup(c => c["PromptSettings:RefinementPrompt"]).Returns("Texto a ser convertido: {{Regra de negócio}}");
            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync(tenantId)).ReturnsAsync(tenantInfo);
            _mocker.GetMock<IUsageDailyServices>()
                .Setup(s => s.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .ReturnsAsync(true);
            _mocker.GetMock<IRagInvocationRouter>()
                .Setup(a => a.ExecuteChatCompletionAsync(
                    It.IsAny<TenantInfoDto>(),
                    It.IsAny<string>(),
                    It.IsAny<ChatCompletionDto>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatCompletionResponse);

            //Act
            var result = await _promptServices.AiPromptRefinement(prompt, tenantId, email);

            //Assert
            Assert.Equal("Prompt refinado", result);
        }

        [Fact(DisplayName = "AiPromptRefinement should throw argument exception when tenant info is invalid")]
        [Trait("AiPromptRefinement", "Fail")]
        public async Task AiPromptRefinement_ShouldThrowArgumentException_InvalidTenantInfo()
        {
            //Arrange
            var prompt = "Minha regra de negócio";
            var tenantId = "tenantId";
            var email = "exemple@email.com";
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = null, AiGatewayKey = string.Empty };

            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync(tenantId)).ReturnsAsync(tenantInfo);

            //Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _promptServices.AiPromptRefinement(prompt, tenantId, email));
        }

        [Fact(DisplayName = "AiPromptRefinement should throw argument exception when refinement prompt is null or empty")]
        [Trait("AiPromptRefinement", "Fail")]
        public async Task AiPromptRefinement_ShouldThrowArgumentException_RefinementPromptNullOrEmpty()
        {
            //Arrange
            var prompt = "Minha regra de negócio";
            var tenantId = "tenantId";
            var email = "exemple@email.com";
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };

            _mocker.GetMock<IConfiguration>().Setup(c => c["PromptSettings:RefinementPrompt"]).Returns(string.Empty);
            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync(tenantId)).ReturnsAsync(tenantInfo);

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _promptServices.AiPromptRefinement(prompt, tenantId, email));

            Assert.Equal("Refinement prompt template not found", exception.Message);
        }

        [Fact(DisplayName = "ProcessOpenAiResponseResult When the execution not found the data a exception will be fired")]
        [Trait("ProcessOpenAiResponseResult", "Fail")]
        public async Task ProcessOpenAiResponseResult_WhenExecutionNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            var metadata = MessagingFixture.FindValidMetaDataAutomationDto();
            var responseDto = MessagingFixture.FindValidOpenAiResponseConsumerResponseDto();

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(metadata.StepToolId, metadata.CardId))
                .ReturnsAsync((StepToolExecution?)null);

            // Act
            var act = async () => await _promptServices.ProcessOpenAiResponseResult(responseDto);

            // Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(act);
            Assert.Equal("StepToolExecution not found", ex.Message);

            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Never);
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Never);
            _mocker.GetMock<IDocumentHistoryRepository>().Verify(x => x.Create(It.IsAny<DocumentHistory>()), Times.Never);
            _mocker.GetMock<IExecutionServices>().Verify(x => x.HandleExecutionProgress(It.IsAny<StepToolExecution>(), It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "Process OpenAiResponseResult when message is present should create history handle progress and commit")]
        [Trait("ProcessOpenAiResponseResult", "Fail")]
        public async Task ProcessOpenAiResponseResult_WhenMessageIsPresent_ShouldCreateHistory_HandleProgress_AndCommit()
        {
            // Arrange
            var responseDto = MessagingFixture.FindValidOpenAiResponseConsumerResponseDto();
            var metadata = JsonSerializer.Deserialize<MetaDataAutomationDto>(responseDto.Data.ToString());
            var execution = MessagingFixture.FindValidStepToolExecution(metadata);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(metadata.StepToolId, metadata.CardId))
                .ReturnsAsync(execution);

            _mocker.GetMock<IExecutionServices>()
                .Setup(x => x.HandleExecutionProgress(execution, responseDto.Email))
                .Returns(Task.CompletedTask);

            // Act
            await _promptServices.ProcessOpenAiResponseResult(responseDto);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _mocker.GetMock<IDocumentHistoryRepository>().Verify(x => x.Create(It.Is<DocumentHistory>(d =>
                execution.Card!= null  &&
                d.IdDocument == execution.Card.DocumentId &&
                d.Input == "Prompt"
            )), Times.Once);

            _mocker.GetMock<IExecutionServices>().Verify(x => x.HandleExecutionProgress(execution, responseDto.Email), Times.Once);

            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "Process OpenAiResponseResult when message is empty should not create history nor handle progress but commit")]
        [Trait("ProcessOpenAiResponseResult", "Fail")]
        public async Task ProcessOpenAiResponseResult_WhenMessageIsEmpty_ShouldNotCreateHistory_NorHandleProgress_ButCommit()
        {
            // Arrange            
            var responseDto = MessagingFixture.FindValidOpenAiResponseConsumerResponseDto(true);
            var metadata = JsonSerializer.Deserialize<MetaDataAutomationDto>(responseDto.Data.ToString());
            var execution =MessagingFixture.FindValidStepToolExecution(metadata);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(metadata.StepToolId, metadata.CardId))
                .ReturnsAsync(execution);

            // Act
            await _promptServices.ProcessOpenAiResponseResult(responseDto);

            // Assert
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _mocker.GetMock<IDocumentHistoryRepository>().Verify(x => x.Create(It.IsAny<DocumentHistory>()), Times.Never);
            _mocker.GetMock<IExecutionServices>().Verify(x => x.HandleExecutionProgress(It.IsAny<StepToolExecution>(), It.IsAny<string>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Never);
        }

        
        [Fact(DisplayName = "Process OpenAiResponseResult when a exception occurs inside try should rollback and throw AppException")]
        [Trait("ProcessOpenAiResponseResult", "Fail")]
        public async Task ProcessOpenAiResponseResult_WhenExceptionOccursInsideTry_ShouldRollback_AndThrowAppException()
        {
            // Arrange
                    
            var responseDto = MessagingFixture.FindValidOpenAiResponseConsumerResponseDto();
            var metadata = JsonSerializer.Deserialize<MetaDataAutomationDto>(responseDto.Data.ToString());
            var execution =MessagingFixture.FindValidStepToolExecution(metadata);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(metadata.StepToolId, metadata.CardId))
                .ReturnsAsync(execution);

            _mocker.GetMock<IDocumentHistoryRepository>()
                .Setup(x => x.Create(It.IsAny<DocumentHistory>()))
                .Throws(new Exception("erro ao salvar histórico"));

            // Act
            var act = async () => await _promptServices.ProcessOpenAiResponseResult(responseDto);

            // Assert
            var ex = await Assert.ThrowsAsync<AppException>(act);
            Assert.Equal("erro ao salvar histórico", ex.Message);

            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
             _mocker.GetMock<IExecutionServices>().Verify(x => x.HandleExecutionProgress(It.IsAny<StepToolExecution>(), It.IsAny<string>()), Times.Never);
        }

    }
}
