using Azure.AI.FormRecognizer.DocumentAnalysis;
using Bogus;
using WoopiAiHub.Application.Dto;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using WoopiAiHub.Domain.DTOs;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class DocumentFixture
    {
        public Document FindValidDocument()
        {
            Document Document = new Faker<Document>("pt_BR")
            .CustomInstantiator(f => new Document(f.Random.AlphaNumeric(10), 
                                                  f.Lorem.Paragraph(), 
                                                  f.Random.AlphaNumeric(10), 
                                                  0, 
                                                  true, 
                                                  f.Person.Email, 
                                                  f.IndexFaker, 
                                                  f.Date.Past()));
            return Document;
        }

        public TenantInfoDto FindValidTenantInfoDto()
        {
            var faker = new Faker<TenantInfoDto>("pt_BR")
              .CustomInstantiator(f => new TenantInfoDto
              {
                  ChunkSize = f.Random.Int(),
                  DatabaseName = f.Random.String(),
                  Email = f.Random.String(),
                  EmbeddingModelName = f.Random.String(),
                  KValue = f.Random.Int(),
                  MaxTokens = f.Random.Int(),
                  Model = f.Random.String(),
                  Name = f.Random.String(),
                  OcrModel = f.Random.String(),
                  RefineTemplate = f.Random.String(),
                  SearchMode = f.Random.String(),
                  Template = f.Random.String(),
              });

            return faker;
        }

        public static AnalyzeResult FindValidAnalyseResult()
        {
            var documentLines = new List<DocumentLine> { DocumentAnalysisModelFactory.DocumentLine("Document line") };
            var documentPages = new List<DocumentPage> { DocumentAnalysisModelFactory.DocumentPage(1, 0, 100, 100, null, null, null, null, documentLines) };
            var documentCells = new List<DocumentTableCell> { DocumentAnalysisModelFactory.DocumentTableCell(DocumentTableCellKind.Content, 0, 0, 0, 0, "Cell content") };
            var boundingRegions = new List<BoundingRegion> { DocumentAnalysisModelFactory.BoundingRegion(1) };
            var documentTables = new List<DocumentTable> { DocumentAnalysisModelFactory.DocumentTable(1, 1, documentCells, boundingRegions) };
            return DocumentAnalysisModelFactory.AnalyzeResult("Analysis", "Content", documentPages, documentTables);
        }

        public HttpResponseMessage FindHttpResponseMessage()
        {
            var filePath = @"../../../Files/TestPDF.pdf";
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var stream = new MemoryStream();
                fileStream.CopyTo(stream);

                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponseMessage.Content = new StreamContent(stream);
                httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = "TestPDF";
                httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                return httpResponseMessage;
            }
        }

        public HttpResponseMessage FindInvalidHttpResponseMessage()
        {
            var filePath = @"../../../Files/TestPDF.pdf";
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var stream = new MemoryStream();
                fileStream.CopyTo(stream);

                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                httpResponseMessage.Content = new StreamContent(stream);
                httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = "TestPDF";
                httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                return httpResponseMessage;
            }
        }

        public DocumentHistory FindValidDocumentHistory()
        {
            DocumentHistory DocumentHistory = new Faker<DocumentHistory>("pt_BR")
            .CustomInstantiator(f => new DocumentHistory
            (
                created: f.Date.Past(),
                id: f.IndexFaker,
                idDocument: f.IndexFaker,
                input: f.Lorem.Paragraph(),
                output: f.Lorem.Paragraph()

            ));

            return DocumentHistory;
        }

        public List<DocumentHistory> FindValidDocumentHistoryList()
        {
            List<DocumentHistory> DocumentHistory = new Faker<DocumentHistory>("pt_BR")
            .CustomInstantiator(f => new DocumentHistory
            (
                created: f.Date.Past(),
                id: f.IndexFaker,
                idDocument: f.IndexFaker,
                input: f.Lorem.Paragraph(),
                output: f.Lorem.Paragraph()
            )).Generate(10);

            return DocumentHistory;
        }

        public DocumentNormalized FindValidDocumentNormalized()
        {
            DocumentNormalized DocumentNormalized = new Faker<DocumentNormalized>("pt_BR")
            .CustomInstantiator(f => new DocumentNormalized
            (
                id: f.IndexFaker,
                idDocument: f.IndexFaker,
                content: f.Lorem.Text(),
                created: f.Date.Past()
            ));

            return DocumentNormalized;
        }

        public DocumentPagedDataDto FindValidDocumentPagedDataDto()
        {
            DocumentPagedDataDto documentPagedDataDto = new Faker<DocumentPagedDataDto>("pt_BR")
            .RuleFor(a => a.Page, 1)
            .RuleFor(a => a.PageSize, 1)
            .RuleFor(a => a.Search, f => f.Lorem.Text())
            .RuleFor(a => a.IsAscending, true)
            .RuleFor(f => f.ColType, f => f.PickRandom<ColTypeDocument>());

            return documentPagedDataDto;
        }

        public DocumentPagedDataDto FindInvalidDocumentPagedDataDto()
        {
            DocumentPagedDataDto documentPagedDataDto = new Faker<DocumentPagedDataDto>("pt_BR")
            .RuleFor(a => a.Page, 0)
            .RuleFor(a => a.PageSize, 0)
            .RuleFor(a => a.Search, f => f.Lorem.Text())
            .RuleFor(a => a.IsAscending, true)
            .RuleFor(f => f.ColType, f => f.PickRandom<ColTypeDocument>());

            return documentPagedDataDto;
        }

        public UpdateHistoryDto FindValidUpdateHistoryDto()
        {
            UpdateHistoryDto updateHistoryDto = new Faker<UpdateHistoryDto>("pt_BR")
             .RuleFor(a => a.IdDocument, f => f.IndexFaker)
             .RuleFor(a => a.OldOutput, f => f.Lorem.Text())
             .RuleFor(a => a.UpdatedOutput, f => f.Lorem.Text());
            
            return updateHistoryDto;
        }

        public QuestionnaireDto FindValidQuestionnaireDto()
        {
            var typeDoc = new TypeDoc("name", "email", 1, DateTime.Now);
            QuestionnaireDto questionnaireDto = new Faker<QuestionnaireDto>("pt_BR")
             .RuleFor(a => a.Id, f => f.IndexFaker)
             .RuleFor(a => a.Title,"title" )
             .RuleFor(a => a.TypeDocId, 1)
             .RuleFor(a => a.EmailCreator, f => f.Person.Email)
             .RuleFor(a => a.TypeDoc, typeDoc)
             .RuleFor(a => a.TypeDocName,"name")
             .RuleFor(a => a.Questions, FindValidQuestion());

            return questionnaireDto;
        }

        public ICollection<Question> FindValidQuestion()
        {
            var Question = new Faker<Question>("pt_BR")
            .CustomInstantiator(f => new Question
            (
                description: f.Lorem.Text(),
                id: f.IndexFaker,
                emailCreator: f.Person.Email,
                created: f.Date.Past()
            ));

            return Question.Generate(1);
        }

        public RequestCreateDocumentDto FindValidRequestCreateDocumentDto()
        {
            var filePath = @"../../../Files/TestPDF.pdf";
            var file = new FileInfo(filePath);
            var formFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, file.Length, "Chunk", file.Name);
            var faker = new Faker("pt_BR");

            var dto = new RequestCreateDocumentDto(
                Chunk: formFile,
                Filename: "title",
                IsLast: true,
                Name: "idea",
                Description: "desc",
                EmailCreator: faker.Internet.Email(),
                TeamsIds: new List<int> { 10 }
            );

            return dto;
        }

        public FileUploadSummaryDto FindValidFileUploadSummaryDto()
        {
            FileUploadSummaryDto fileUploadSummaryDto = new Faker<FileUploadSummaryDto>("pt_BR")
             .RuleFor(a => a.TotalSizeUploaded, "test")
             .RuleFor(a => a.GuidId, "test")
             .RuleFor(a => a.FileName, "test");

             return fileUploadSummaryDto;
        }

        public DocumentAnalysisResponseDto FindValidDocumentAnalysisResponseDto()
        {
            DocumentAnalysisResponseDto documentAnalysisResponseDto = new Faker<DocumentAnalysisResponseDto>("pt_BR")
             .RuleFor(a => a.Id, 1)
             .RuleFor(a => a.EmailCreator, f => f.Person.Email)
             .RuleFor(a => a.Tenant, "test")
             .RuleFor(a => a.KeyMongoAcess, "key")
             .RuleFor(a => a.Embeddings_model_name, "text-embedding-3-large");

            return documentAnalysisResponseDto;
        }

        public HeadersDto FindValidHeadersDto()
        {
            HeadersDto headersDto = new Faker<HeadersDto>("pt_BR")
            .RuleFor(a => a.EmailCreator, f => f.Person.Email)
            .RuleFor(a => a.Tenant, "test")
            .RuleFor(a => a.KeyMongoAccess, "key")
            .RuleFor(a => a.Language, "PT");

            return headersDto;
        }

        public DocumentInputDto FindValidDocumentInputDto()
        {
            DocumentInputDto documentInputDto = new Faker<DocumentInputDto>("pt_BR")
            .RuleFor(a => a.Id, 1)
            .RuleFor(a => a.Input, "test");

            return documentInputDto;
        }

        public DocumentQuestionnaireDto FindDocumentQuestionnaireDto()
        {
            DocumentQuestionnaireDto documentQuestionnaireDto = new Faker<DocumentQuestionnaireDto>("pt_BR")
            .RuleFor(a => a.IdDocument, 1)
            .RuleFor(a => a.IdQuestionnaire, 1);

            return documentQuestionnaireDto;
        }
    }

    [CollectionDefinition(nameof(DocumentCollection))]
    public class DocumentCollection : ICollectionFixture<DocumentFixture>
    {
    }
}
