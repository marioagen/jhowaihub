using DocAnalyzer.Domain.Utils;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocAnalyzer.Api.Attributes
{
    public class SwaggerCustomHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation,
                         OperationFilterContext context)
        {
            if (operation.Parameters is null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = HeaderNames.XTenant,
                In = ParameterLocation.Header,
                Description = "Tenant Name",
                Required = true,
            });
        }

    }
}
