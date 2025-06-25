using DocAnalyzer.Domain.DTOs.Refit;
using DocAnalyzer.Domain.Utils;
using Refit;

namespace DocAnalyzer.Domain.Interfaces.Refit
{
    public interface IFileRepositoryApi
    {
        [Multipart]
        [Post("/File/upload")]
        Task<FileUploadSummaryDto> Upload([AliasAs("file")] ByteArrayPart bytes,
                                          [Header(HeaderNames.XTenant)] string tenant);
    }
}
