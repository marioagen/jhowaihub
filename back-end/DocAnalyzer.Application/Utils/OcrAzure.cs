using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Configuration;
using DocAnalyzer.Domain.Interfaces.Services;
using DocAnalyzer.Domain.DTOs.Response;
using DocAnalyzer.Domain.Enum;
using DocAnalyzer.Domain.Interfaces.Repository.Cache;

namespace DocAnalyzer.Application.Utils
{
    public class OcrAzure : IOcrAzure
    {
        private readonly IConfiguration _config;
        private readonly ITenantCacheServices _tenantCacheServices;

        public OcrAzure(IConfiguration config,
                        ITenantCacheServices tenantCacheServices)
        {
            _config = config;
            _tenantCacheServices = tenantCacheServices;
        }

        /// <summary>
        /// Sends the file to OCR to analyze.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<AnalyzeResult> ProcessResult(Stream stream,
                                                       string tenantName)
        {
            var ApiKey = _config.GetValue<string>("OCRSettings:OCRApiKey");
            var Endpoint = _config.GetValue<string>("OCRSettings:OCREndpoint");
            var tenant = await _tenantCacheServices.FindTenantAsync(tenantName,
                                                                    ColTypeModule.DocAnalyzer);

            AzureKeyCredential Credential = new(ApiKey);
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(Endpoint), Credential);
            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, tenant.OcrModel.ToString(), stream);
            AnalyzeResult result = operation.Value;

            return result;

        }
    }
}
