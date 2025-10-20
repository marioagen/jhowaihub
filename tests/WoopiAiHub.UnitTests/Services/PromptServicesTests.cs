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

        [Fact]
        public void CreateUniquePrompt_Success()
        {
            var dto = new PromptCreateDto { Name = "Teste", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>()));
            _promptRepository.Setup(r => r.CreateUniquePrompt(It.IsAny<Prompt>())).Returns(true);

            var service = CreateService();
            var result = service.CreateUniquePrompt(dto, email);

            Assert.True(result);
        }

        [Fact]
        public void CreateUniquePrompt_Failure_Duplicated()
        {
            var dto = new PromptCreateDto { Name = "Teste", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>()));
            _promptRepository.Setup(r => r.CreateUniquePrompt(It.IsAny<Prompt>())).Returns(false);

            var service = CreateService();
            Assert.Throws<AppException>(() => service.CreateUniquePrompt(dto, email));
        }

        [Fact]
        public void Update_Success()
        {
            var dto = new PromptUpdateDto { Id = 1, Name = "Novo", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";
            var promptDto = new PromptDto { Id = 1, Name = "Antigo", Description = "Desc", Text = "Texto", IdUser = Guid.NewGuid(), Created = DateTime.Now };

            _validatePrompt.Setup(v => v.ValidateOwnership(1, email));
            _promptRepository.Setup(r => r.FindById(1)).Returns(promptDto);
            _validatePrompt.Setup(v => v.ValidatePromptFields(It.IsAny<Prompt>()));
            _promptRepository.Setup(r => r.Update(It.IsAny<Prompt>())).Returns(true);
            _unitOfWork.Setup(u => u.BeginTransaction());
            _unitOfWork.Setup(u => u.Commit());

            var service = CreateService();
            var result = service.Update(dto, email);

            Assert.True(result);
        }

        [Fact]
        public void Update_Failure_PromptNotFound()
        {
            var dto = new PromptUpdateDto { Id = 1, Name = "Novo", Description = "Desc", Text = "Texto" };
            var email = "user@teste.com";

            _validatePrompt.Setup(v => v.ValidateOwnership(1, email));
            _promptRepository.Setup(r => r.FindById(1)).Returns((PromptDto)null);

            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.Update(dto, email));
        }

        [Fact]
        public void DeleteByIds_Success()
        {
            var ids = new List<int> { 1, 2 };
            _promptRepository.Setup(r => r.Delete(ids)).Returns(true);

            var service = CreateService();
            var result = service.DeleteByIds(ids);

            Assert.True(result);
        }

        [Fact]
        public void DeleteByIds_Failure()
        {
            var ids = new List<int> { 1, 2 };
            _promptRepository.Setup(r => r.Delete(ids)).Returns(false);

            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.DeleteByIds(ids));
        }

        [Fact]
        public void FindById_Success()
        {
            var promptDto = new PromptDto { Id = 1, Name = "Teste", Description = "Desc", Text = "Texto", IdUser = Guid.NewGuid(), Created = DateTime.Now };
            _promptRepository.Setup(r => r.FindById(1)).Returns(promptDto);

            var service = CreateService();
            var result = service.FindById(1);

            Assert.Equal(promptDto, result);
        }

        [Fact]
        public void FindById_Failure()
        {
            _promptRepository.Setup(r => r.FindById(1)).Returns((PromptDto)null);

            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindById(1));
        }

        [Fact]
        public void FindAll_Success()
        {
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var queryable = new List<PromptDto> { new PromptDto { Id = 1, Name = "Teste", Description = "Desc", Text = "Texto", IdUser = idUser, Created = DateTime.Now } }.AsQueryable();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindAllWithOwnerStatus(idUser)).Returns(queryable);

            var service = CreateService();
            var result = service.FindAll(email);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void FindAll_Failure_PromptNotFound()
        {
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindAllWithOwnerStatus(idUser)).Returns((IQueryable<PromptDto>)null);

            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindAll(email));
        }

        [Fact]
        public void FindAll_Failure_InvalidUserId()
        {
            var email = "user@teste.com";
            var idUser = Guid.Empty;
            var queryable = new List<PromptDto>().AsQueryable();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindAllWithOwnerStatus(idUser)).Returns(queryable);

            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindAll(email));
        }

        [Fact]
        public void FindAllPaged_Failure_InvalidPage()
        {
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var pagedDataDto = new PagedDataDto { Page = 0, PageSize = 10 };

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);

            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindAllPaged(pagedDataDto, email));
        }

        [Fact]
        public void FindByIdUserPaged_Failure_PromptNotFound()
        {
            var email = "user@teste.com";
            var idUser = Guid.NewGuid();
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 10 };

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindByIdUser(idUser)).Returns((IQueryable<PromptDto>)null);

            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindByIdUserPaged(pagedDataDto, email));
        }

        [Fact]
        public void FindByIdUserPaged_Failure_InvalidUserId()
        {
            var email = "user@teste.com";
            var idUser = Guid.Empty;
            var pagedDataDto = new PagedDataDto { Page = 1, PageSize = 10 };
            var queryable = new List<PromptDto>().AsQueryable();

            _userServices.Setup(u => u.FindIdByEmail(email)).Returns(idUser);
            _promptRepository.Setup(r => r.FindByIdUser(idUser)).Returns(queryable);

            var service = CreateService();
            Assert.Throws<ArgumentException>(() => service.FindByIdUserPaged(pagedDataDto, email));
        }
    }

}
