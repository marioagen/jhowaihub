using WoopiAiHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IQuestionQuestionnaireRepository
    {
        bool Delete(ICollection<Question> questions);
    }
}
