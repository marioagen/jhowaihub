using WoopiAiHub.Domain.DTOs.Messaging;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IExternalFileUploadServices
    {
        Task ProcessExternalFileUpload(ExternalFileUploadDto externalFileUploadDto);
    }
}
