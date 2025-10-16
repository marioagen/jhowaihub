using System.Linq.Dynamic.Core;
using System.Text.Json;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository;

namespace WoopiAiHub.Application.Services
{
    public class PromptServices : IPromptServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPromptRepository _promptRepository;
        private readonly IValidatePrompt _validatePrompt;
        private readonly IUserServices _userServices;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IHubNotifier _hubNotifier;
        public PromptServices(IUnitOfWork unitOfWork,
                              IPromptRepository promptRepository,
                              IValidatePrompt validatePrompt,
                              IUserServices userServices,
                              IStepToolExecutionRepository stepToolExecutionRepository,
                              IStepToolOutputRepository stepToolOutputRepository,
                              IHubNotifier hubNotifier)
        {
            _unitOfWork = unitOfWork;
            _promptRepository = promptRepository;
            _validatePrompt = validatePrompt;
            _userServices = userServices;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolOutputRepository = stepToolOutputRepository;
            _hubNotifier = hubNotifier;
        }

        /// <summary>
        /// Create a new prompt
        /// </summary>
        /// <param name="promptCreateDto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool CreateUniquePrompt(PromptCreateDto promptCreateDto, string email)
        {
            var idUser = _userServices.FindIdByEmail(email);
            var prompt = GeneratePromptToCreate(promptCreateDto,idUser);

            _validatePrompt.ValidatePromptFields(prompt);

            var createPromptResult = _promptRepository.CreateUniquePrompt(prompt);
            if (!createPromptResult)
            {
                //throw new AppException(ErrorCode.DuplicatedPrompt, "Duplicated Prompt");
            }

            return createPromptResult;
        }

        /// <summary>
        /// Update a prompt
        /// </summary>
        /// <param name="promptUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Update(PromptUpdateDto promptUpdateDto, string emailCreator)
        {
            _validatePrompt.ValidateOwnership(promptUpdateDto.Id,
                                              emailCreator);

            var promptDto = _promptRepository.FindById(promptUpdateDto.Id);
            if (promptDto == null)
            {
                throw new ArgumentException("Prompt not found");
            }

            var prompt = GeneratePromptToUpdate(promptDto, promptUpdateDto);

            _validatePrompt.ValidatePromptFields(prompt);

            _unitOfWork.BeginTransaction();
            try
            {
                var promptUpdateResult = _promptRepository.Update(prompt);

                if (!promptUpdateResult)
                {
                    //throw new AppException(ErrorCode.DuplicatedPrompt, "Duplicated Prompt");
                }

                _unitOfWork.Commit();

                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Find all prompts by email creator paginated
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        public PagedResultDto<PromptDto> FindByIdUserPaged(PagedDataDto pagedDataDto,
                                                           string emailCreator)
        {
            var idUser = _userServices.FindIdByEmail(emailCreator);
            var query = _promptRepository.FindByIdUser(idUser);

            query = pagedDataDto.IsAscending ?
                query.OrderBy(nameof(Domain.Models.Prompt.Name)) :
                query.OrderBy(nameof(Domain.Models.Prompt.Name) + " descending");

            var result = PromptPagination(query, new PagedDataDto());

            return result;
        }

        /// <summary>
        /// Find all prompts paginated
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        public PagedResultDto<PromptDto> FindAllPaged(PagedDataDto pagedDataDto,
                                                      string emailCreator)
        {
            var idUser = _userServices.FindIdByEmail(emailCreator);
            if (pagedDataDto.Page > 0)
            {
                var query = _promptRepository.FindAllWithOwnerStatus(idUser);

                query = pagedDataDto.IsAscending ?
                    query.OrderBy(nameof(Domain.Models.Prompt.Name)) :
                    query.OrderBy(nameof(Domain.Models.Prompt.Name) + " descending");

                var result = PromptPagination(query, pagedDataDto);
                return result;
            }
            else
            {
                throw new ArgumentException("The number of pages must be greater than 0");
            }
        }

        /// <summary>
        /// Delete a prompt by id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            return _promptRepository.Delete(ids);
        }

        /// <summary>
        /// Pagination of the prompt
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        private static PagedResultDto<PromptDto> PromptPagination(IQueryable<PromptDto> query,
                                                                  PagedDataDto pagedDataDto)
        {
            int pageCount, currentPage = 0;

            if (!string.IsNullOrEmpty(pagedDataDto.Search))
            {
                string search = pagedDataDto.Search.ToLower();
                query = query.Where(i => i.Id.ToString().Contains(search) ||
                                         i.Name.Contains(search) ||
                                         i.Description.Contains(search) ||
                                         i.Text.Contains(search));
            }

            var count = query.Count();

            if (pagedDataDto.PageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)count / pagedDataDto.PageSize);
                currentPage = pagedDataDto.Page <= pageCount ? pagedDataDto.Page : 1;
                query = query.Skip((currentPage - 1) * pagedDataDto.PageSize)
                             .Take(pagedDataDto.PageSize);
            }

            return new PagedResultDto<PromptDto>()
            {
                Items = query,
                CurrentPage = currentPage,
                TotalPages = pageCount,
                Count = count
            };
        }

        /// <summary>
        /// Find a prompt by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public PromptDto FindById(int id)
        {
            var promptDto = _promptRepository.FindById(id);
            if (promptDto == null)
            {
                throw new ArgumentException("Prompt not found");
            }
            return promptDto;
        }

        /// <summary>
        /// Find all prompts with owner status
        /// </summary>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        public IQueryable<PromptDto> FindAll(string emailCreator)
        {
            var idUser = _userServices.FindIdByEmail(emailCreator);
            var query = _promptRepository.FindAllWithOwnerStatus(idUser);

            return query;
        }

        /// <summary>
        /// Generate a new prompt
        /// </summary>
        /// <param name="promptCreateDto"></param>
        /// <param name="emailCreator"></param>
        private static Domain.Models.Prompt GeneratePromptToCreate(PromptCreateDto promptCreateDto, Guid idUser)
        {
            var prompt = new Domain.Models.Prompt(
                0,
                DateTime.Now,
                promptCreateDto.Name,
                promptCreateDto.Description,
                promptCreateDto.Text,
                idUser);

            return prompt;
        }

        /// <summary>
        /// Generate a new prompt
        /// </summary>
        /// <param name="promptDto"></param>
        /// <param name="promptUpdateDto"></param>
        private static Domain.Models.Prompt GeneratePromptToUpdate(PromptDto promptDto, PromptUpdateDto promptUpdateDto)
        {
            var prompt = new Domain.Models.Prompt(
                promptDto.Id,
                promptDto.Created,
                promptUpdateDto.Name,
                promptUpdateDto.Description,
                promptUpdateDto.Text,
                promptDto.IdUser);

            return prompt;
        }

        /// <summary>
        /// Process the chat completion result and create a DocumentAnswer.
        /// </summary>
        /// <param name="chatCompletionResponseDto"></param>
        /// <returns></returns>
        public async Task ProcessChatCompletionResult(ChatCompletionResponseDto chatCompletionResponseDto)
        {
            var dataDto = JsonSerializer.Deserialize<MetaDataAutomationDto>(chatCompletionResponseDto.Data.ToString());
            var execution = await _stepToolExecutionRepository.FindByStepToolIdAndCardIdAsync(dataDto.StepToolId,
                                                                                              dataDto.CardId);
            await UpdateExecutionAsync(execution!, chatCompletionResponseDto.Email);
            await SaveStepToolOutputAsync(execution!, chatCompletionResponseDto.Choices[0].Message.Content);
        }

        /// <summary>
        /// Updates StepToolExecution status and send notification 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task UpdateExecutionAsync(StepToolExecution execution, string email)
        {
            var count = await _stepToolExecutionRepository.ExecutionsByStepIdCountAsync(execution.StepTool!.StepId,
                                                                                        execution.CardId);
            var percent = ((double)execution.StepTool.Order / count) * 100;

            execution.UpdateStatusExecution(StatusExecution.Ready);
            await _stepToolExecutionRepository.UpdateAsync(execution);

            await _hubNotifier.CardProgessAsync(email, execution.CardId, percent, execution.StepTool.StepId);
        }

        /// <summary>
        /// Updates StepToolExecution output
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="outputStepTool"></param>
        /// <returns></returns>
        private async Task SaveStepToolOutputAsync(StepToolExecution execution, string outputStepTool)
        {
            var output = new StepToolOutput(
                0,
                DateTime.Now,
                execution.StepToolId,
                execution.CardId,
                outputStepTool
            );

            await _stepToolOutputRepository.CreateAsync(output);
        }
    }
}
