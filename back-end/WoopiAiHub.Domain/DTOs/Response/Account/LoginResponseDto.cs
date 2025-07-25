namespace WoopiAiHub.Domain.DTOs.Response.Account
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public LoginDataDto? Data { get; set; }
    }
}