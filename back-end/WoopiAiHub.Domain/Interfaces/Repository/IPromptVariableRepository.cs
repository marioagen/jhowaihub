using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IPromptVariableRepository
    {
        bool DeleteByPromptId(int promptId);
    }
}
