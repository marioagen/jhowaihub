namespace WoopiAiHub.Domain.DTOs.Response.Account
{
    public class LoginDataDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public bool IsAdmin { get; set; }
        public Array? Permissions { get; set; }
    }
}