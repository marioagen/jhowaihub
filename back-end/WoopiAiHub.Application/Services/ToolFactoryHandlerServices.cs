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
