using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class KeyExternalAccessAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        /// <summary>
        /// Validates that in the external request the header key passed exists in the  marketplace database 
        /// </summary>
        /// <param name="context"></param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var logger = (ILogger<KeyExternalAccessAuthorizationAttribute>)context.HttpContext
                                                                                  .RequestServices
                                                                                  .GetRequiredService(typeof(ILogger<KeyExternalAccessAuthorizationAttribute>));

            var _configuration = (IConfiguration)context.HttpContext
                                                        .RequestServices
                                                        .GetRequiredService(typeof(IConfiguration));

            var KeyAccess = context.HttpContext.Request.Headers[HeaderNames.KeyAccess].ToString();
            var internalKeyAccess = _configuration.GetSection("KeyAccess").Get<string>()!;

            if (internalKeyAccess.Equals(KeyAccess) is false)
            {
                logger.LogWarning("Unauthorized access with invalid key.");
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
