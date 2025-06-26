using Microsoft.AspNetCore.Mvc;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public class AuthenticateHeaderDto
    {
        private string authorization = string.Empty;
        [FromHeader(Name = "Authorization")]
        public string Authorization
        {
            get
            {
                return authorization.Replace("Bearer ", "");
            }
            set
            {
                authorization = value;
            }
        }
    }
}
