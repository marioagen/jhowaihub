using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Utils;
using Refit;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface IFileRepositoryApi
    {
        [Multipart]
        [Post("/File/upload")]
        Task<FileUploadSummaryDto> Upload([AliasAs("file")] ByteArrayPart bytes,
                                          [Header(HeaderNames.XTenant)] string tenant);

        [Delete("/File/delete")]
        Task<HttpResponseMessage> Delete([AliasAs("GuidfileName")] string guidFileName);
    }
}
