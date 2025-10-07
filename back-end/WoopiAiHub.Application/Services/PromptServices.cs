using System.Linq.Dynamic.Core;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class PromptServices : IPromptServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPromptRepository _promptRepository;
        private readonly IValidatePrompt _validatePrompt;

        public PromptServices(IUnitOfWork unitOfWork,
                              IPromptRepository promptRepository,
                              IValidatePrompt validatePrompt)
        {
            _unitOfWork = unitOfWork;
            _promptRepository = promptRepository;
            _validatePrompt = validatePrompt;
        }

        /// <summary>
        /// Create a new prompt
        /// </summary>
        /// <param name="promptCreateDto"></param>
        /// <param name="emailCreator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool CreateUniquePrompt(PromptCreateDto promptCreateDto)
        {
            var prompt = GeneratePromptToCreate(promptCreateDto);

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
        public bool Update(PromptUpdateDto promptUpdateDto)
        {
            _validatePrompt.ValidateOwnership(promptUpdateDto.Id,
                                              promptUpdateDto.IdUser);

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
                                                           Guid idUser)
        {
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
                                                      Guid idUser)
        {
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
        public IQueryable<PromptDto> FindAll(Guid idUser)
        {
            var query = _promptRepository.FindAllWithOwnerStatus(idUser);

            return query;
        }

        /// <summary>
        /// Generate a new prompt
        /// </summary>
        /// <param name="promptCreateDto"></param>
        /// <param name="emailCreator"></param>
        private static Domain.Models.Prompt GeneratePromptToCreate(PromptCreateDto promptCreateDto)
        {
            var prompt = new Domain.Models.Prompt(
                0,
                DateTime.Now,
                promptCreateDto.Name,
                promptCreateDto.Description,
                promptCreateDto.Text,
                promptCreateDto.IdUser);

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
    }
}
