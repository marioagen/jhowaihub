using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using Newtonsoft.Json;

namespace WoopiAiHub.Application.Services
{
    public class ToolFactoryHandlerServices : IToolFactoryHandlerServices
    {
        public IToolHandlerServices GetHandler(ToolType toolType)
        {
            return new OCRHandler();
        }
    }

    public class OCRHandler : IToolHandlerServices
    {
        /// <summary>
        /// Builds and returns an execution payload for processing OCR tasks.
        /// </summary>
        /// <remarks>The returned payload includes metadata such as the tenant, reference file, model,
        /// email, and response queue, along with the provided <paramref name="stepToolId"/> and <paramref
        /// name="cardId"/>. The payload is serialized as JSON and sent to the "ocrQueue".</remarks>
        /// <param name="input">The input string used to generate the payload. This parameter is currently unused in the payload
        /// construction.</param>
        /// <param name="stepToolId">The identifier of the step tool associated with the OCR process.</param>
        /// <param name="cardId">The identifier of the card associated with the OCR process.</param>
        /// <returns>An <see cref="ExecutionMessageDto"/> containing the serialized OCR processing data and the target queue
        /// name.</returns>
        public ExecutionMessageDto BuildPayload(string input, int stepToolId, int cardId)
        {
            return new ExecutionMessageDto()
            {
                Message = JsonConvert.SerializeObject(new ProcessOcrDto
                {
                    Tenant = "Stefanini_gtoliveira4@latam.stefanini.com",
                    ReferenceFile = "a0c6ed1553c854ed4b8574079331c34b5",
                    Model = "GPT-4o",
                    Email = "cjandreazza@latam.stefanini.com",
                    ResponseQueue = "OcrQueueAiHubResponse",
                    Data = new MetaDataAutomationDto(cardId, stepToolId)
                }),
                Queue = "ocrQueue"
            };
        }
    }
}
