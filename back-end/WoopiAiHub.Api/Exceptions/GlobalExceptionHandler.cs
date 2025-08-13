using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Api.Exceptions
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
                                                    Exception exception,
                                                    CancellationToken cancellationToken)
        {
            logger.LogError(exception, "An unhandled exception occurred.");

            var problemDetails = new AppProblemDetails
            {
                Title = "An error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred. Please try again later.", 
                Instance = httpContext.Request.Path 
            };

            switch (exception)
            {
                case ArgumentException:
                case InvalidOperationException:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Detail = "There was a problem with your request.";
                    break;

                case Refit.ApiException apiException:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Detail = apiException.Message;
                    break;

                case UnauthorizedAccessException:
                    problemDetails.Status = StatusCodes.Status401Unauthorized;
                    problemDetails.Detail = "You are not authorized to access this resource.";
                    break;

                case AppException appException:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Detail = appException.Message;
                    problemDetails.ErrorCode = appException.ErrorCode;
                    break;

                case KeyNotFoundException:
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    problemDetails.Detail = "The requested resource was not found.";
                    break;

                default:
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
