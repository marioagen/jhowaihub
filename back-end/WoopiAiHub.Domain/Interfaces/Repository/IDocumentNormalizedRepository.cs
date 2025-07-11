using WoopiAiHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IDocumentNormalizedRepository
    {
        bool Create(DocumentNormalized documentNormalized);
        DocumentNormalized FindById(int idDocument);
        int FindDocumentNormalizedCount();
        bool Update(DocumentNormalized documentNormalized);
    }
}
