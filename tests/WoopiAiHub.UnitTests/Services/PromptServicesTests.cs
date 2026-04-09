using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class PromptServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly PromptServices _promptServices;

        public PromptServicesTests()
        {
            _mocker = new AutoMocker();
            var mockPromptSettings = new Mock<IOptions<PromptSettings>>();
            mockPromptSettings.Setup(x => x.Value).Returns(new PromptSettings
            {
                TemplateFileName = "name.json", Folder = "folder"
            });
            var mockChatSettings = new Mock<IOptions<ChatCompletionSettings>>();
            mockChatSettings.Setup(x => x.Value).Returns(new ChatCompletionSettings
            {
                Model = "model", ApiVersion = "v1", MaxTokens = 100, Temperature = 0.5f
            });

            _mocker.Use(mockPromptSettings);
            _mocker.Use(mockChatSettings);

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
            var promptDto = new PromptDto
            {
                Id = 1,
                Name = "Antigo",
                Description = "Desc",
                Text = "Texto",
                IdUser = Guid.NewGuid(),
                Created = DateTime.Now
            };
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
            var promptDto = new PromptDto
            {
                Id = 1,
                Name = "Teste",
                Description = "Desc",
                Text = "Texto",
                IdUser = Guid.NewGuid(),
                Created = DateTime.Now
            };
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
            var queryable = new List<PromptDto>
            {
                new PromptDto
                {
                    Id = 1,
                    Name = "Teste",
                    Description = "Desc",
                    Text = "Texto",
                    IdUser = idUser,
                    Created = DateTime.Now
                }
            }.AsQueryable();
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

            var templatesResponse = new PromptTemplatesResponse
            {
                Prompts = new List<PromptTemplateDto>
                {
                    new PromptTemplateDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Template 1",
                        Description = "Desc 1",
                        Text = "Text 1",
                        Created = DateTime.Now
                    },
                    new PromptTemplateDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Template 2",
                        Description = "Desc 2",
                        Text = "Text 2",
                        Created = DateTime.Now
                    }
                }
            };
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
            Assert.Equal(2, result.Count);
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

        [Fact(DisplayName = "Import prompts by ids success")]
        [Trait("ImportPromptsByIds", "Success")]
        public async Task ImportPromptsByIds_Success()
        {
            //Arrange
            var promptId = Guid.NewGuid();
            var templateIds = new List<Guid> { promptId };
            var email = "test@example.com";
            var templatesResponse = new PromptTemplatesResponse
            {
                Prompts = new List<PromptTemplateDto>
                {
                    new PromptTemplateDto
                    {
                        Id = promptId,
                        Name = "Template 1",
                        Description = "Desc 1",
                        Text = "Text 1",
                        Created = DateTime.Now
                    }
                }
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
            _mocker.GetMock<IUserServices>().Setup(u => u.FindIdByEmail(email)).Returns(Guid.NewGuid());
            _mocker.GetMock<IPromptRepository>().Setup(r => r.CreateByRange(It.IsAny<List<Prompt>>())).Returns(true);

            //Act
            var result = await _promptServices.ImportPromptsByIds(templateIds, email);

            //Assert
            Assert.True(result);
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
            var expectedPrompts = new List<PromptInternalDto>
            {
                new PromptInternalDto { Id = 1, Name = "Prompt 1", Description = "Description 1" },
                new PromptInternalDto { Id = 2, Name = "Prompt 2", Description = "Description 2" },
                new PromptInternalDto { Id = 3, Name = "Prompt 3", Description = "Description 3" }
            };

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
            _mocker.GetMock<IChatCompletionApi>()
                .Setup(a => a.GetChatCompletion(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ChatCompletionDto>()))
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

        [Fact(DisplayName = "TestPromptWithContextAsync success and logs token usage")]
        [Trait("TestPromptWithContext", "Success")]
        public async Task TestPromptWithContextAsync_Success_LogsTokens()
        {
            var promptText = "Extraia os pontos principais";
            var contextText = "Texto do PDF de exemplo";
            var tenantId = "tenant-id";
            var email = "user@test.com";
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "gateway-key" };
            ChatCompletionDto? capturedDto = null;
            var chatCompletionResponse = new ChatCompletionResponseDto
            {
                Choices = new List<ChatChoiceDto>
                {
                    new ChatChoiceDto { Message = new ChatMessageResponseDto { Content = "Resposta da IA" } }
                },
                Usage = new ChatUsageDto { TotalTokens = 42 }
            };

            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync(tenantId)).ReturnsAsync(tenantInfo);
            _mocker.GetMock<IChatCompletionApi>()
                .Setup(a => a.GetChatCompletion(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ChatCompletionDto>()))
                .Callback((string _, string _, string _, string _, ChatCompletionDto dto) => capturedDto = dto)
                .ReturnsAsync(chatCompletionResponse);
            _mocker.GetMock<IUsageDailyServices>()
                .Setup(u => u.AddByValuesAsync(MetricNames.Token, email, 42, "model"))
                .ReturnsAsync(true);

            var result = await _promptServices.TestPromptWithContextAsync(promptText, contextText, tenantId, email);

            Assert.Equal("Resposta da IA", result);
            Assert.NotNull(capturedDto);
            Assert.Single(capturedDto!.Messages);
            Assert.Equal("system", capturedDto.Messages[0].Role);
            Assert.Equal(
                "Baseado no: \"Texto do PDF de exemplo\" e seguindo as orientações a seguir: Extraia os pontos principais",
                capturedDto.Messages[0].Content);

            _mocker.GetMock<IUsageDailyServices>().Verify(
                u => u.AddByValuesAsync(MetricNames.Token, email, 42, "model"),
                Times.Once);
        }

        [Fact(DisplayName = "TestPromptWithContextAsync should throw argument exception when prompt text is empty")]
        [Trait("TestPromptWithContext", "Fail")]
        public async Task TestPromptWithContextAsync_ShouldThrowArgumentException_WhenPromptTextEmpty()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _promptServices.TestPromptWithContextAsync("   ", "ctx", "tenant", "email@test.com"));

            Assert.Equal("Prompt text is required", exception.Message);
        }

        [Fact(DisplayName = "TestPromptWithContextAsync should throw argument exception when tenant gateway is invalid")]
        [Trait("TestPromptWithContext", "Fail")]
        public async Task TestPromptWithContextAsync_ShouldThrowArgumentException_InvalidTenantInfo()
        {
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = null, AiGatewayKey = string.Empty };
            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync("tenant")).ReturnsAsync(tenantInfo);

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _promptServices.TestPromptWithContextAsync("prompt", "ctx", "tenant", "email@test.com"));

            Assert.Equal("AiGateway ApplicationId not found", exception.Message);
        }

        [Fact(DisplayName = "TestPromptWithContextAsync should throw app exception when AI returns empty content")]
        [Trait("TestPromptWithContext", "Fail")]
        public async Task TestPromptWithContextAsync_ShouldThrowAppException_WhenAiContentEmpty()
        {
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var chatCompletionResponse = new ChatCompletionResponseDto
            {
                Choices = new List<ChatChoiceDto>
                {
                    new ChatChoiceDto { Message = new ChatMessageResponseDto { Content = string.Empty } }
                },
                Usage = new ChatUsageDto { TotalTokens = 10 }
            };

            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mocker.GetMock<IChatCompletionApi>()
                .Setup(a => a.GetChatCompletion(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ChatCompletionDto>()))
                .ReturnsAsync(chatCompletionResponse);
            _mocker.GetMock<IUsageDailyServices>()
                .Setup(u => u.AddByValuesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var exception = await Assert.ThrowsAsync<AppException>(async () =>
                await _promptServices.TestPromptWithContextAsync("p", "c", "t", "e@mail.com"));

            Assert.Equal("Empty response from AI Gateway", exception.Message);
        }

        [Fact(DisplayName = "TestPromptWithContextAsync logs zero tokens when usage is missing")]
        [Trait("TestPromptWithContext", "Success")]
        public async Task TestPromptWithContextAsync_LogsZeroTokens_WhenUsageMissing()
        {
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var chatCompletionResponse = new ChatCompletionResponseDto
            {
                Choices = new List<ChatChoiceDto>
                {
                    new ChatChoiceDto { Message = new ChatMessageResponseDto { Content = "Ok" } }
                },
                Usage = null!
            };

            _mocker.GetMock<ITenantCacheServices>().Setup(s => s.FindTenantAsync("t")).ReturnsAsync(tenantInfo);
            _mocker.GetMock<IChatCompletionApi>()
                .Setup(a => a.GetChatCompletion(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ChatCompletionDto>()))
                .ReturnsAsync(chatCompletionResponse);
            _mocker.GetMock<IUsageDailyServices>()
                .Setup(u => u.AddByValuesAsync(MetricNames.Token, "e@mail.com", 0, "model"))
                .ReturnsAsync(true);

            var result = await _promptServices.TestPromptWithContextAsync("prompt", "", "t", "e@mail.com");

            Assert.Equal("Ok", result);
            _mocker.GetMock<IUsageDailyServices>().Verify(
                u => u.AddByValuesAsync(MetricNames.Token, "e@mail.com", 0, "model"),
                Times.Once);
        }
    }
}
