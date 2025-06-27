using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Application.Utils
{
    public class AppProblemDetails : ProblemDetails
    {
        public ErrorCode ErrorCode { get; set; } = ErrorCode.DefaultError;
    }
}
