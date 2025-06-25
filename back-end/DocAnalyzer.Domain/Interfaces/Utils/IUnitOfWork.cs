using DocAnalyzer.Domain.Interfaces.Refit;
using DocAnalyzer.Domain.Utils;
using Google.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Interfaces.Utils
{
    public interface IUnitOfWork
    {

        void BeginTransaction();

        void Commit();

        void Rollback();

    }
}
