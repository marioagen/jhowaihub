using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IAnonymizationServices
    {
        Task ProcessAnonymization(ProcessAnonymizationRequestDto requestDto, HeadersDto headersDto);
    }
}
