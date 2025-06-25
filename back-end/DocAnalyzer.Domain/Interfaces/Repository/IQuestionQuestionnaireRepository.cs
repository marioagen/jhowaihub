using DocAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Interfaces.Repository
{
    public interface IQuestionQuestionnaireRepository
    {
        bool Delete(ICollection<Question> questions);
    }
}
