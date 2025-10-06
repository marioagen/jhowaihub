using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Repository
{
    public interface IPromptVariableRepository
    {
        bool DeleteByPromptId(int promptId);
    }
}
