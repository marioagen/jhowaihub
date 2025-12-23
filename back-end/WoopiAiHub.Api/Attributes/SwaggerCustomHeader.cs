using WoopiAiHub.Domain.Utils;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WoopiAiHub.Api.Attributes
{
    public class SwaggerCustomHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters is null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            var hasOptionalTenantHeader = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<OptionalTenantHeaderAttribute>()
                .Any();

            //operation.Parameters.Add(new OpenApiParameter
            //{
            //    Name = HeaderNames.XTenant,
            //    In = ParameterLocation.Header,
            //    Description = "Tenant Name",
            //    Required = hasOptionalTenantHeader is false,
            //});
        }
    }
}
