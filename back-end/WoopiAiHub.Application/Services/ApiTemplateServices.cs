using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class ApiTemplateServices(
        IApiTemplateRepository templateRepository,
        ILogger<ApiTemplateServices> logger
    ) : IApiTemplateServices
    {
        private readonly IApiTemplateRepository _templateRepository = templateRepository;
        private readonly ILogger<ApiTemplateServices> _logger = logger;
        private const string NotFoundMessage = "Template not found";

        /// <summary>
        /// Retrieves a template by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<ApiTemplateDto> FindById(int id)
        {
            var template = await _templateRepository.FindById(id);
            if (template == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, ApiTemplateLabel.NotFound);
            }

            return template;
        }

        /// <summary>
        /// Deletes a template by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> DeleteById(int id)
        {
            var template = await _templateRepository.FindByIdReturnModel(id);
            if (template == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, ApiTemplateLabel.NotFound);
            }

            return await _templateRepository.DeleteById(id);
        }

        /// <summary>
        /// Retrieves a collection of API templates that match the specified filter criteria.
        /// </summary>
        /// <param name="filter">An object containing the filter criteria to apply when searching for API templates. Only templates matching
        /// these criteria are returned. Cannot be null.</param>
        /// <returns></returns>
        public async Task<ICollection<ApiTemplateDto>> FindAll(ApiTemplateFilterDto filter)
        {
            return await _templateRepository.FindAll(filter);
        }

        /// <summary>
        /// This method sends the current page  
        /// and search text to repository and return an PaginatedListDto<ApiTemplateDto>.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public PaginatedListDto<ApiTemplateDto> FindAllPaged(ApiTemplatePagedFilterDto filter)
        {
            if (filter.Page > 0)
            {
                var templates = _templateRepository.FindAllPaged(filter);
                var paginatedList = PaginationHelper.Paginate(templates, filter.Page, filter.PageSize);
                return paginatedList;
            }
            else
            {
                var ex = new ArgumentException("Invalid Page");
                _logger.LogError(ex,
                    $"An argument exception occurred in the {nameof(ApiTemplateServices)} in the {nameof(FindAllPaged)} method");
                throw ex;
            }
        }

        /// <summary>
        /// Creates a new API template asynchronously based on the provided data transfer object.
        /// </summary>
        /// <param name="templateCreateDto">The data transfer object containing the details of the template to be created.</param>
        /// <returns>True if the template was created successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown if required fields are missing.</exception>
        public async Task<bool> CreateAsync(ApiTemplateCreateDto templateCreateDto)
        {
            var template = new ApiTemplate(
                templateCreateDto.Name,
                templateCreateDto.Method,
                templateCreateDto.Url,
                templateCreateDto.QueryTemplate,
                templateCreateDto.HeaderTemplate,
                templateCreateDto.BodyTemplate,
                templateCreateDto.Description,
                templateCreateDto.EnableAccessFromMcp
            );

            return await _templateRepository.CreateAsync(template);
        }

        /// <summary>
        /// Updates an existing API template asynchronously based on the provided data transfer object.
        /// </summary>
        /// <param name="templateUpdateDto">The data transfer object containing the updated details of the template.</param>
        /// <returns>True if the template was updated successfully; otherwise, false.</returns>
        /// <exception cref="AppException">Thrown if the template is not found or if required fields are missing.</exception>
        public async Task<bool> UpdateAsync(ApiTemplateUpdateDto templateUpdateDto)
        {
            var existingTemplate = await _templateRepository.FindByIdReturnModel(templateUpdateDto.Id) ?? throw new AppException(ErrorCode.NotFound, NotFoundMessage, ApiTemplateLabel.NotFound);

            existingTemplate.UpdateName(templateUpdateDto.Name);
            existingTemplate.UpdateMethod(templateUpdateDto.Method);
            existingTemplate.UpdateUrl(templateUpdateDto.Url);
            existingTemplate.UpdateQueryTemplate(templateUpdateDto.QueryTemplate);
            existingTemplate.UpdateHeaderTemplate(templateUpdateDto.HeaderTemplate);
            existingTemplate.UpdateBodyTemplate(templateUpdateDto.BodyTemplate);
            existingTemplate.UpdateDescription(templateUpdateDto.Description);
            existingTemplate.UpdateEnableAccessFromMcp(templateUpdateDto.EnableAccessFromMcp);

            existingTemplate.Validate();

            var templateUpdated =  await _templateRepository.UpdateAsync(existingTemplate);
            if(!existingTemplate.EnableAccessFromMcp && templateUpdated) {
                await _templateRepository.RemovePromptLinked(existingTemplate.Id);
            }

            return await _templateRepository.UpdateAsync(existingTemplate);
        }
    }
}
