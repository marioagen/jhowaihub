using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Application.Utils
{
    public class AppException : Exception
    {
        public ErrorCode? ErrorCode { get; private set; }
        public string? LabelError { get; private set; }

        public AppException(ErrorCode? errorCode, 
                            string message,
                            string? labelError) : base(message)
        {
            ErrorCode = errorCode;
            LabelError = labelError;
        }
    }
}
