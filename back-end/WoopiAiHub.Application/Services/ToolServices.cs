using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class ToolServices : IToolServices
    {
        private readonly IToolRepository _toolRepository;
        private readonly IToolTypeRepository _toolTypeRepository;
        private readonly IStepToolRepository _stepToolTypeRepository;
        private readonly IApiClientFactory _apiClientFactory;
        private readonly IEncryptionService _encryptionService;

        public ToolServices(IToolRepository toolRepository,
                            IToolTypeRepository toolTypeRepository,
                            IStepToolRepository stepToolTypeRepository,
                            IApiClientFactory apiClientFactory,
                            IEncryptionService encryptionService)
        {
            _toolRepository = toolRepository;
            _toolTypeRepository = toolTypeRepository;
            _stepToolTypeRepository = stepToolTypeRepository;
            _apiClientFactory = apiClientFactory;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Creates a new tool asynchronously based on the provided data transfer object.
        /// </summary>
        /// <remarks>This method ensures that the tool being created is unique. If a duplicate tool is
        /// detected,  an exception is thrown to indicate the conflict.</remarks>
        /// <param name="toolCreateDto">The data transfer object containing the details of the tool to be created.  The object must include the
        /// tool's name, type, and associated input and output data identifiers.</param>
        /// <returns></returns>
        /// <exception cref="AppException">Thrown if a tool with the same unique properties already exists.</exception>
        public async Task<bool> CreateAsync(ToolCreateDto toolCreateDto)
        {
            var toolType = await _toolTypeRepository.FindModelByIdAsync(toolCreateDto.ToolTypeId)
                ?? throw new AppException(ErrorCode.NotFound, "ToolType not found", null);

            string encryptedApiKey = string.Empty;
            if (toolType.IsN8nTool())
            {
                if (string.IsNullOrEmpty(toolCreateDto.ConnectorUrl) || string.IsNullOrEmpty(toolCreateDto.ConnectorApiKey))
                {
                    throw new AppException(ErrorCode.RequiredField, "Connector Url and Connector Api Key are required", null);
                }
                encryptedApiKey = _encryptionService.Encrypt(toolCreateDto.ConnectorApiKey);
            }

            var tool = new Tool(
                0,
                DateTime.UtcNow,
                toolCreateDto.Name,
                true,
                toolCreateDto.ToolTypeId,
                toolCreateDto.InputDataId,
                toolCreateDto.OutputDataId,
                toolCreateDto.IsEditableInput,
                toolCreateDto.ConnectorUrl,
                encryptedApiKey
             );

            var result = await _toolRepository.CreateUniqueAsync(tool);
            if (!result)
            {
                throw new AppException(ErrorCode.Duplicated, "Duplicated Tool", null);
            }

            return result;
        }

        /// <summary>
        /// Deletes the specified items asynchronously.
        /// </summary>
        /// <remarks>This method delegates the deletion operation to the underlying repository. Ensure
        /// that the provided identifiers correspond to valid items.</remarks>
        /// <param name="ids">A list of item identifiers to delete. The list cannot be null or empty.</param>
        /// <returns></returns>
        public bool Delete(List<int> ids)
        {
            return _toolRepository.Delete(ids);
        }

        /// <summary>
        /// Retrieves all tools from the repository.
        /// </summary>
        /// <remarks>This method asynchronously retrieves all tools stored in the repository. The returned
        /// collection  may be empty if no tools are available.</remarks>
        /// <returns></returns>
        public async Task<IEnumerable<ToolDto>> FindAllAsync()
        {
            return await _toolRepository.FindAllAsync();
        }

        /// <summary>
        /// Retrieves a paginated list of tools based on the specified paging and sorting criteria.
        /// </summary>
        /// <remarks>The tools are sorted by their name in ascending or descending order, based on the
        /// value of <see cref="ToolPagedDataDto.IsAscending"/>.</remarks>
        /// <param name="toolPagedDataDto">An object containing the paging and sorting parameters, including the page number, page size, and sorting
        /// direction. The <see cref="ToolPagedDataDto.Page"/> property must be greater than 0.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="toolPagedDataDto"/> specifies a page number less than or equal to 0.</exception>
        public PagedResponseDto<ToolDto> FindAllPaged(ToolPagedDataDto toolPagedDataDto)
        {
            if (toolPagedDataDto.Page <= 0)
            {
                throw new ArgumentException("The number of pages must be greater than 0");
            }

            var query = _toolRepository.FindAllPaged();
            query = toolPagedDataDto.IsAscending
                ? query.OrderBy(t => t.Name)
                : query.OrderByDescending(t => t.Name);

            return Pagination(query, toolPagedDataDto);
        }

        /// <summary>
        /// Retrieves a tool by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the tool to retrieve. Must be a positive integer.</param>
        /// <returns></returns>
        public async Task<ToolDto?> FindByIdAsync(int id)
        {
            return await _toolRepository.FindByIdAsync(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toolUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> UpdateAsync(ToolUpdateDto toolUpdateDto)
        {
            var tool = await _toolRepository.FindModelByIdAsync(toolUpdateDto.Id)
                ?? throw new AppException(ErrorCode.NotFound, "Tool not found", null);

            var toolType = await _toolTypeRepository.FindModelByIdAsync(toolUpdateDto.ToolTypeId)
                ?? throw new AppException(ErrorCode.NotFound, "ToolType not found", null);

            ValidateConnector(toolUpdateDto, tool, toolType);

            string encryptedApiKey = GetEncryptedApiKey(tool, toolType, toolUpdateDto.ConnectorApiKey);

            tool.Update(toolUpdateDto.Name,
                        toolUpdateDto.ToolTypeId,
                        toolUpdateDto.InputDataId,
                        toolUpdateDto.OutputDataId,
                        toolUpdateDto.IsEditableInput,
                        toolUpdateDto.ConnectorUrl,
                        encryptedApiKey);

            var result = await _toolRepository.UpdateAsync(tool);
            if (!result)
            {
                throw new AppException(ErrorCode.Duplicated, "Duplicated Tool", null);
            }

            return result;
        }

        /// <summary>
        /// Validate connector url and connector api key
        /// </summary>
        /// <param name="toolUpdateDto"></param>
        /// <param name="tool"></param>
        /// <param name="toolType"></param>
        /// <exception cref="AppException"></exception>
        private static void ValidateConnector(ToolUpdateDto toolUpdateDto, Tool tool, ToolType toolType)
        {
            if (toolType!.IsN8nTool())
            {
                if (string.IsNullOrEmpty(toolUpdateDto.ConnectorUrl))
                {
                    throw new AppException(ErrorCode.RequiredField, "Connector Url is required", null);
                }

                if (string.IsNullOrEmpty(tool.ConnectorApiKey) && string.IsNullOrEmpty(toolUpdateDto.ConnectorApiKey))
                {
                    throw new AppException(ErrorCode.RequiredField, "Connector Api Key is required", null);
                }
            }
        }

        /// <summary>
        /// Get encrypted API key for storage
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="toolType"></param>
        /// <param name="newApiKey"></param>
        /// <returns></returns>
        private string GetEncryptedApiKey(Tool tool, ToolType toolType, string? newApiKey)
        {
            if (!toolType!.IsN8nTool())
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(newApiKey))
            {
                return _encryptionService.Encrypt(newApiKey);
            }

            return tool.ConnectorApiKey ?? string.Empty;
        }

        /// <summary>
        /// Creates a paginated response from a queryable collection of tools based on the specified pagination and
        /// search criteria.
        /// </summary>
        /// <remarks>If the <paramref name="pagedDataDto"/> specifies a search term, the method filters
        /// the collection to include only items whose name or ID contains the search term. If the page size is zero,
        /// the method returns all items in a single page.</remarks>
        /// <param name="totalList">The queryable collection of <see cref="ToolDto"/> objects to paginate. This collection may be filtered based
        /// on the search criteria.</param>
        /// <param name="pagedDataDto"></returns>
        private static PagedResponseDto<ToolDto> Pagination(IQueryable<ToolDto> totalList,
                                                            ToolPagedDataDto toolPagedDataDto)
        {
            int pageCount, currentPage = 0;

            if (!string.IsNullOrEmpty(toolPagedDataDto.Search))
            {
                totalList = totalList.Where(i => i.Name.ToLower().Contains(toolPagedDataDto.Search.ToLower()) ||
                                                 i.Id.ToString().Contains(toolPagedDataDto.Search));
            }

            if (toolPagedDataDto.ToolTypeId.HasValue)
            {
                totalList = totalList.Where(i => i.ToolTypeId == toolPagedDataDto.ToolTypeId.Value);
            }

            var totalListCount = totalList.Count();

            if (toolPagedDataDto.PageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
                toolPagedDataDto.PageSize = totalListCount;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)totalListCount / toolPagedDataDto.PageSize);
                currentPage = toolPagedDataDto.Page <= pageCount ? toolPagedDataDto.Page : 1;
                totalList = totalList.Skip((currentPage - 1) * toolPagedDataDto.PageSize)
                                     .Take(toolPagedDataDto.PageSize);
            }

            return new PagedResponseDto<ToolDto>()
            {
                Items = totalList,
                CurrentPage = currentPage,
                TotalPages = pageCount,
                TotalCount = totalListCount,
            };
        }

        /// <summary>
        /// Validate connector if connects using url and api key 
        /// </summary>
        /// <param name="toolConnectorDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> ValidateConnector(ToolConnectorDto toolConnectorDto)
        {
            if (string.IsNullOrEmpty(toolConnectorDto.ConnectorUrl) || string.IsNullOrEmpty(toolConnectorDto.ConnectorApiKey))
            {
                throw new AppException(ErrorCode.RequiredField, "Connector Url and Connector Api Key are required", null);
            }

            var api = _apiClientFactory.Create(toolConnectorDto.ConnectorUrl);

            var response = await api.FindWorkflows(toolConnectorDto.ConnectorApiKey);

            return response.IsSuccessStatusCode;
        }
    }
}
