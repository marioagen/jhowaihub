using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class JwtTokenServices : IJwtTokenServices
    {   
        
        private readonly IConfiguration _config;

        public JwtTokenServices(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Method used by the Account service and MCP service to generate the JWT token to authentication
        /// </summary>
        /// <param name="jwtKey"></param>
        /// <param name="jwtIssuer"></param>
        /// <param name="jwtAudience"></param>
        /// <param name="user"></param>
        /// <param name="tokenExpirationTime"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string GenerateTokenWithParameters(string? jwtKey, string jwtIssuer, string jwtAudience, string user, int? tokenExpirationTime = null)
        {
            var key = jwtKey ?? throw new ArgumentException("JWT key is not configured.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expirationMinutes = tokenExpirationTime ?? _config.GetValue("JWT:AccessTokenExpirationMinutes", 60);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(jwtIssuer,
                jwtAudience,
                claims,
                expires: DateTime.Now.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}