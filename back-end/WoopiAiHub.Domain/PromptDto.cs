using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain
{
    public class PromptDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string EmailCreator { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
        public DateTime Created { get; set; }
        public List<PromptVariableDto> Variables { get; set; } = new List<PromptVariableDto>();
    }
}
