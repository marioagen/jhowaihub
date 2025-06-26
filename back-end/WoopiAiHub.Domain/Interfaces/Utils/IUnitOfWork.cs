using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Utils;
using Google.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface IUnitOfWork
    {

        void BeginTransaction();

        void Commit();

        void Rollback();

    }
}
