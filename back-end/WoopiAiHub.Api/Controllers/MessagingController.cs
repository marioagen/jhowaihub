using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.Interfaces.Messenging;

namespace WoopiAiHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagingController : ControllerBase
    {
        private readonly IMessagePublisher<TestMessage> _publisher;

        public MessagingController(IMessagePublisher<TestMessage> publisher)
        {
            _publisher = publisher;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] TestMessage message)
        {
            // Publica a mensagem na fila "publicar-dados"
            await _publisher.PublishAsync("ocrQueue", message);

            return Ok(new { Status = "Published", Message = message });
        }
    }

    // DTO de teste
    public class TestMessage
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public string? Content { get; set; }
    }
}
