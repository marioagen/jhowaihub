using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Connector;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Services.Automation;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class AutomationController : ControllerBase
    {
        private readonly IAutomationServices _automationServices;

        public AutomationController(IAutomationServices automationServices)
        {
            _automationServices = automationServices;
        }

        /// <summary>
        /// Endpoint that receives the request to return connector workflows by toolId
        /// </summary>
        /// <param name="toolId"></param>
        /// <returns></returns>
        [HttpGet("Workflows/N8n/{toolId}")]
        [SwaggerOperation("Endpoint that receives the request to return connector workflows by toolId")]
        [ProducesResponseType(typeof(ICollection<ConnectorDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindN8nWorkflowsByToolId(int toolId)
        {
            var result = await _automationServices.FindN8nWorkflowsByToolId(toolId);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to list tool's webhook inputs
        /// </summary>
        /// <param name="webhookInputDto"></param>
        /// <returns></returns>
        [HttpGet("Workflow/N8n/WebhookInputs")]
        [SwaggerOperation("Endpoint that receives the request to return connector workflows")]
        [ProducesResponseType(typeof(ICollection<ConnectorDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindN8nWebhookInputs([FromQuery] WebhookInputDto webhookInputDto)
        {
            var result = await _automationServices.FindN8nWebhookInputs(webhookInputDto);
            return Ok(result);
        }
    }
}
