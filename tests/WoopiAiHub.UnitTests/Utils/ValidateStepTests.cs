using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Utils
{
    [Collection(nameof(WorkflowCollection))]
    public class ValidateStepTests
    {
        private readonly AutoMocker _mocker;
        private readonly ValidateStep _validateStep;
        private readonly WorkflowFixture _fixture;

        public ValidateStepTests(WorkflowFixture fixture)
        {
            _mocker = new AutoMocker();
            _validateStep = _mocker.CreateInstance<ValidateStep>();
            _fixture = fixture;
        }

        [Fact(DisplayName = "ValidateCreateStep - Should throw AppException when steps collection is null")]
        [Trait("ValidateCreateStep", "Fail")]
        public void ValidateCreateStep_StepsNull_ThrowsAppException()
        {
            // Arrange
            ICollection<StepCreateDto>? stepsCreateDto = null;

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateCreateStep(stepsCreateDto!));

            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal(StepLabel.Required, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateCreateStep - Should throw AppException when steps collection is empty")]
        [Trait("ValidateCreateStep", "Fail")]
        public void ValidateCreateStep_StepsEmpty_ThrowsAppException()
        {
            // Arrange
            var stepsCreateDto = new List<StepCreateDto>();

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateCreateStep(stepsCreateDto));

            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal(StepLabel.Required, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateCreateStep - Should throw AppException when step name is empty")]
        [Trait("ValidateCreateStep", "Fail")]
        public void ValidateCreateStep_StepNameEmpty_ThrowsAppException()
        {
            // Arrange
            var stepsCreateDto = new List<StepCreateDto>
            {
                new StepCreateDto
                {
                    Name = string.Empty,
                    Order = 1,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateCreateStep(stepsCreateDto));

            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal(StepLabel.NameRequired, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateCreateStep - Should throw AppException when step name is null")]
        [Trait("ValidateCreateStep", "Fail")]
        public void ValidateCreateStep_StepNameNull_ThrowsAppException()
        {
            // Arrange
            var stepsCreateDto = new List<StepCreateDto>
            {
                new StepCreateDto
                {
                    Name = string.Empty,
                    Order = 1,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateCreateStep(stepsCreateDto));

            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal(StepLabel.NameRequired, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateCreateStep - Should throw AppException when step order is negative")]
        [Trait("ValidateCreateStep", "Fail")]
        public void ValidateCreateStep_StepOrderNegative_ThrowsAppException()
        {
            // Arrange
            var stepsCreateDto = new List<StepCreateDto>
            {
                new StepCreateDto
                {
                    Name = "Step 1",
                    Order = -1,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateCreateStep(stepsCreateDto));

            Assert.Equal(ErrorCode.InvalidValue, exception.ErrorCode);
            Assert.Equal(StepLabel.OrderInvalid, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateCreateStep - Should throw AppException when duplicate step names exist")]
        [Trait("ValidateCreateStep", "Fail")]
        public void ValidateCreateStep_DuplicateStepNames_ThrowsAppException()
        {
            // Arrange
            var stepsCreateDto = new List<StepCreateDto>
            {
                new StepCreateDto
                {
                    Name = "Step 1",
                    Order = 1,
                    ProfileId = 1,
                    StatusId = 1
                },
                new StepCreateDto
                {
                    Name = "Step 1",
                    Order = 2,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateCreateStep(stepsCreateDto));

            Assert.Equal(ErrorCode.Conflict, exception.ErrorCode);
            Assert.Equal(StepLabel.NameAlreadyExists, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateCreateStep - Should throw AppException when duplicate step orders exist")]
        [Trait("ValidateCreateStep", "Fail")]
        public void ValidateCreateStep_DuplicateStepOrders_ThrowsAppException()
        {
            // Arrange
            var stepsCreateDto = new List<StepCreateDto>
            {
                new StepCreateDto
                {
                    Name = "Step 1",
                    Order = 1,
                    ProfileId = 1,
                    StatusId = 1
                },
                new StepCreateDto
                {
                    Name = "Step 2",
                    Order = 1,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateCreateStep(stepsCreateDto));

            Assert.Equal(ErrorCode.Conflict, exception.ErrorCode);
            Assert.Equal(StepLabel.OrderInvalid, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateCreateStep - Should not throw exception with valid steps")]
        [Trait("ValidateCreateStep", "Success")]
        public void ValidateCreateStep_ValidSteps_Success()
        {
            // Arrange
            var stepsCreateDto = new List<StepCreateDto>
            {
                WorkflowFixture.FindValidStepCreateDto(),
                new StepCreateDto
                {
                    Name = "Step 2",
                    Order = 3,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            _validateStep.ValidateCreateStep(stepsCreateDto);
        }

        [Fact(DisplayName = "ValidateUpdateStep - Should throw AppException when steps collection is null")]
        [Trait("ValidateUpdateStep", "Fail")]
        public void ValidateUpdateStep_StepsNull_ThrowsAppException()
        {
            // Arrange
            var workflow = new Workflow(1, DateTime.UtcNow, new List<Team>(), "Workflow 1");
            ICollection<StepUpdateDto>? stepsUpdateDto = null;

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateUpdateStep(workflow, stepsUpdateDto!));

            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal(StepLabel.Required, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateUpdateStep - Should throw AppException when steps collection is empty")]
        [Trait("ValidateUpdateStep", "Fail")]
        public void ValidateUpdateStep_StepsEmpty_ThrowsAppException()
        {
            // Arrange
            var workflow = new Workflow(1, DateTime.UtcNow, new List<Team>(), "Workflow 1");
            var stepsUpdateDto = new List<StepUpdateDto>();

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateUpdateStep(workflow, stepsUpdateDto));

            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal(StepLabel.Required, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateUpdateStep - Should throw AppException when workflow is null")]
        [Trait("ValidateUpdateStep", "Fail")]
        public void ValidateUpdateStep_WorkflowNull_ThrowsAppException()
        {
            // Arrange
            Workflow? workflow = null;
            var stepsUpdateDto = new List<StepUpdateDto>
            {
                WorkflowFixture.FindValidStepUpdateDto()
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateUpdateStep(workflow!, stepsUpdateDto));

            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal(WorkflowLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateUpdateStep - Should throw AppException when step name is empty")]
        [Trait("ValidateUpdateStep", "Fail")]
        public void ValidateUpdateStep_StepNameEmpty_ThrowsAppException()
        {
            // Arrange
            var workflow = new Workflow(1, DateTime.UtcNow, new List<Team>(), "Workflow 1");
            var stepsUpdateDto = new List<StepUpdateDto>
            {
                new StepUpdateDto
                {
                    Id = 1,
                    Name = string.Empty,
                    Order = 1,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateUpdateStep(workflow, stepsUpdateDto));

            Assert.Equal(ErrorCode.RequiredField, exception.ErrorCode);
            Assert.Equal(StepLabel.NameRequired, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateUpdateStep - Should throw AppException when step order is negative")]
        [Trait("ValidateUpdateStep", "Fail")]
        public void ValidateUpdateStep_StepOrderNegative_ThrowsAppException()
        {
            // Arrange
            var workflow = new Workflow(1, DateTime.UtcNow, new List<Team>(), "Workflow 1");
            var stepsUpdateDto = new List<StepUpdateDto>
            {
                new StepUpdateDto
                {
                    Id = 1,
                    Name = "Step 1",
                    Order = -1,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateUpdateStep(workflow, stepsUpdateDto));

            Assert.Equal(ErrorCode.InvalidValue, exception.ErrorCode);
            Assert.Equal(StepLabel.OrderInvalid, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateUpdateStep - Should throw AppException when duplicate step names exist")]
        [Trait("ValidateUpdateStep", "Fail")]
        public void ValidateUpdateStep_DuplicateStepNames_ThrowsAppException()
        {
            // Arrange
            var workflow = new Workflow(1, DateTime.UtcNow, new List<Team>(), "Workflow 1");
            var stepsUpdateDto = new List<StepUpdateDto>
            {
                new StepUpdateDto
                {
                    Id = 1,
                    Name = "Step 1",
                    Order = 1,
                    ProfileId = 1,
                    StatusId = 1
                },
                new StepUpdateDto
                {
                    Id = 2,
                    Name = "Step 1",
                    Order = 2,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateUpdateStep(workflow, stepsUpdateDto));

            Assert.Equal(ErrorCode.Conflict, exception.ErrorCode);
            Assert.Equal(StepLabel.NameAlreadyExists, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateUpdateStep - Should throw AppException when duplicate step orders exist")]
        [Trait("ValidateUpdateStep", "Fail")]
        public void ValidateUpdateStep_DuplicateStepOrders_ThrowsAppException()
        {
            // Arrange
            var workflow = new Workflow(1, DateTime.UtcNow, new List<Team>(), "Workflow 1");
            var stepsUpdateDto = new List<StepUpdateDto>
            {
                new StepUpdateDto
                {
                    Id = 1,
                    Name = "Step 1",
                    Order = 1,
                    ProfileId = 1,
                    StatusId = 1
                },
                new StepUpdateDto
                {
                    Id = 2,
                    Name = "Step 2",
                    Order = 1,
                    ProfileId = 1,
                    StatusId = 1
                }
            };

            // Act & Assert
            var exception = Assert.Throws<AppException>(() =>
                _validateStep.ValidateUpdateStep(workflow, stepsUpdateDto));

            Assert.Equal(ErrorCode.Conflict, exception.ErrorCode);
            Assert.Equal(StepLabel.OrderInvalid, exception.LabelError);
        }

        [Fact(DisplayName = "ValidateUpdateStep - Should not throw exception with valid steps")]
        [Trait("ValidateUpdateStep", "Success")]
        public void ValidateUpdateStep_ValidSteps_Success()
        {
            // Arrange
            var workflow = new Workflow(1, DateTime.UtcNow, new List<Team>(), "Workflow 1");
            var stepsUpdateDto = WorkflowFixture.FindValidStepUpdateDto();
            var stepsUpdateDto2 = WorkflowFixture.FindValidStepUpdateDto();
            stepsUpdateDto2.Order = stepsUpdateDto.Order + 1;
            var stepsUpdateDtoList = new List<StepUpdateDto>
            {
                stepsUpdateDto,
                stepsUpdateDto2
            };

            // Act & Assert (should not throw)
            _validateStep.ValidateUpdateStep(workflow, stepsUpdateDtoList);
        }

        [Fact(DisplayName = "ValidateDeleteStep - Should throw AppException when steps are in use")]
        [Trait("ValidateDeleteStep", "Fail")]
        public async Task ValidateDeleteStep_StepsInUse_ThrowsAppException()
        {
            // Arrange
            var stepIds = new List<int> { 1, 2, 3 };
            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(r => r.CountByStepsInUse(It.IsAny<ICollection<int>>()))
                .ReturnsAsync(5);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(async () =>
                await _validateStep.ValidateDeleteStep(stepIds));

            Assert.Equal(ErrorCode.Conflict, exception.ErrorCode);
            Assert.Equal(CardLabel.CardsOpened, exception.LabelError);
            cardRepository.Verify(r => r.CountByStepsInUse(It.IsAny<ICollection<int>>()), Times.Once);
        }

        [Fact(DisplayName = "ValidateDeleteStep - Should not throw exception when steps are not in use")]
        [Trait("ValidateDeleteStep", "Success")]
        public async Task ValidateDeleteStep_StepsNotInUse_Success()
        {
            // Arrange
            var stepIds = new List<int> { 1, 2, 3 };
            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(r => r.CountByStepsInUse(It.IsAny<ICollection<int>>()))
                .ReturnsAsync(0);

            // Act & Assert (should not throw)
            await _validateStep.ValidateDeleteStep(stepIds);

            cardRepository.Verify(r => r.CountByStepsInUse(It.IsAny<ICollection<int>>()), Times.Once);
        }
    }
}
