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

namespace WoopiAiHub.UnitTests.Services
{
    public class PromptServicesTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IPromptRepository> _promptRepository = new();
        private readonly Mock<IValidatePrompt> _validatePrompt = new();
        private readonly Mock<IUserServices> _userServices = new();
        private readonly Mock<IStepToolExecutionRepository> _stepToolExecutionRepository = new();
        private readonly Mock<IStepToolOutputRepository> _stepToolOutputRepository = new();
        private readonly Mock<IHubNotifier> _hubNotifier = new();

        private PromptServices CreateService()
        {
            return new PromptServices(
                _unitOfWork.Object,
                _promptRepository.Object,
                _validatePrompt.Object,
                _userServices.Object,
                _stepToolExecutionRepository.Object,
                _stepToolOutputRepository.Object,
                _hubNotifier.Object
            );
        }

        [Fact(DisplayName = "Create unique prompt success")]
        [Trait("Create", "Success")]
        public void CreateUniquePrompt_Success()
        {
            //Arrange
            var dto = new PromptCreateDto { Name = "Teste", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>())).Returns(true);
            _promptRepository.Setup(r => r.CreateUniquePrompt(It.IsAny<Prompt>())).Returns(true);

            //Act
            var service = CreateService();
            var result = service.CreateUniquePrompt(dto, email);

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

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>())).Returns(true);
            _promptRepository.Setup(r => r.CreateUniquePrompt(It.IsAny<Prompt>())).Returns(false);

            //Act/Assert
            var service = CreateService();
            Assert.Throws<AppException>(() => service.CreateUniquePrompt(dto, email));
        }

        [Fact(DisplayName = "Create unique prompt should returns false when data is empty")]
        [Trait("Create", "Fail")]
        public void CreateUniquePrompt_ShouldReturnFalse_Empty()
        {
            //Arrange
            var dto = new PromptCreateDto { Name = "Teste", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>())).Returns(false);

            //Act
            var service = CreateService();
            var result = service.CreateUniquePrompt(dto, email);

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

            _validatePrompt.Setup(v => v.ValidateOwnership(1, email));
            _promptRepository.Setup(r => r.FindById(1)).Returns(promptDto);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>()));
            _promptRepository.Setup(r => r.Update(It.IsAny<Prompt>())).Returns(true);
            _unitOfWork.Setup(u => u.BeginTransaction());
            _unitOfWork.Setup(u => u.Commit());

            //Act
            var service = CreateService();
            var result = service.Update(dto, email);

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

            _validatePrompt.Setup(v => v.ValidateOwnership(1, email));
            _promptRepository.Setup(r => r.FindById(1)).Returns((PromptDto)null);

            //Act/Assert
            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.Update(dto, email));
        }

        [Fact(DisplayName = "Delete prompts by ids success")]
        [Trait("Delete", "Success")]
        public void DeleteByIds_Success()
        {
            //Arrange
            var ids = new List<int> { 1, 2 };
            _promptRepository.Setup(r => r.Delete(ids)).Returns(true);

            //Act
            var service = CreateService();
            var result = service.DeleteByIds(ids);

            //Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Delete prompts by should throw argumentException")]
        [Trait("Delete", "Success")]
        public void DeleteByIds_ShouldThrowArgumentException()
        {
            //Arrange
            var ids = new List<int> { 1, 2 };
            _promptRepository.Setup(r => r.Delete(ids)).Returns(false);

            //Act/Assert
            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.DeleteByIds(ids));
        }

        [Fact(DisplayName = "Find prompts by id success")]
        [Trait("FindById", "Success")]
        public void FindById_Success()
        {
            //Arrange
            var promptDto = new PromptDto { Id = 1, Name = "Teste", Description = "Desc", Text = "Texto", IdUser = Guid.NewGuid(), Created = DateTime.Now };
            _promptRepository.Setup(r => r.FindById(1)).Returns(promptDto);

            //Act
            var service = CreateService();
            var result = service.FindById(1);

            //Assert
            Assert.Equal(promptDto, result);
        }

        [Fact(DisplayName = "Find prompts by id should throw argument exception")]
        [Trait("FindById", "Fail")]
        public void FindById_ShouldThrowArgumentException()
        {
            //Arrange
            _promptRepository.Setup(r => r.FindById(1)).Returns((PromptDto)null);

            var service = CreateService();
            //Act/Assert
            Assert.Throws<ArgumentException>(() => service.FindById(1));
        }

        [Fact(DisplayName = "Find all prompts success")]
        [Trait("FindAll", "Success")]
        public void FindAll_Success()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var queryable = new List<PromptDto> { new PromptDto { Id = 1, Name = "Teste", Description = "Desc", Text = "Texto", IdUser = idUser, Created = DateTime.Now } }.AsQueryable();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindAllWithOwnerStatus(idUser)).Returns(queryable);

            //Act
            var service = CreateService();
            var result = service.FindAll(email);

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

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindAllWithOwnerStatus(idUser)).Returns((IQueryable<PromptDto>)null);

            //Act/Assert
            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindAll(email));
        }

        [Fact(DisplayName = "Find all prompts should throw argumentException when user id is invalid")]
        [Trait("FindAll", "Fail")]
        public void FindAll_ShouldThrowArgumentException_InvalidUserId()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.Empty;
            var queryable = new List<PromptDto>().AsQueryable();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindAllWithOwnerStatus(idUser)).Returns(queryable);

            //Act/Assert
            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindAll(email));
        }

        [Fact(DisplayName = "Find all prompts paged should throw argumentException when user page is invalid")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_ShouldThrowArgumentException_InvalidPage()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var pagedDataDto = new PagedDataDto { Page = 0, PageSize = 10 };

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);

            //Act/Assert
            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindAllPaged(pagedDataDto, email));
        }

        [Fact(DisplayName = "Find all prompts paged success")]
        [Trait("FindAllPaged", "Success")]
        public void FindAllPaged_Success()
        {
            //Arrange
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 10 };

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);

            //Act
            var service = CreateService();
             var result = service.FindAllPaged(pagedDataDto, email);

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

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindByIdUser(idUser)).Returns((IQueryable<PromptDto>)null);

            //Act/Assert
            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindByIdUserPaged(pagedDataDto, email));
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

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindByIdUser(idUser)).Returns(queryable);

            //Act/Assert
            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindByIdUserPaged(pagedDataDto, email));
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

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindByIdUser(idUser)).Returns(queryable);

            //Act
            var service = CreateService();
            var result = service.FindByIdUserPaged(pagedDataDto, email);

            //Assert
            Assert.NotNull(result);
        }
    }

}
