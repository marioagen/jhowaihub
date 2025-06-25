using DocAnalyzer.Domain.Utils;
using Refit;

namespace DocAnalyzer.Domain.Interfaces.Refit.Functions
{
    public interface IFunctionFileRetriever
    {
        [Get("/FileRetrieverAsync")]
        Task<HttpResponseMessage> Get(string fileGuidId, 
                                      [Header(HeaderNames.XFunctionsKey)] string key,
                                      [Header(HeaderNames.XTenant)] string tenant);
    }
}
