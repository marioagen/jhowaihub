using Azure.AI.FormRecognizer.DocumentAnalysis;
using Bogus;
using Microsoft.AspNetCore.Http;
using PdfSharp.Pdf.IO;
using System;
using System.Net;
using System.Text;
using WoopiAiHub.Application.Dto;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.AnalyzeResultAzure;
using Xunit;

namespace WoopiAiHub.UnitTests.Fixture
{
    public class DocumentFixture
    {
        public Document FindValidDocument()
        {
            Document Document = new Faker<Document>("pt_BR")
            .CustomInstantiator(f => new Document(
                f.Random.AlphaNumeric(10),
                f.Lorem.Paragraph(),
                f.Random.AlphaNumeric(10),
                Domain.Enum.DocumentStatus.ReadyForAnalysis,
                true,
                f.Person.Email,
                f.IndexFaker,
                new List<Workflow>(),
                f.Date.Past())
            );
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
             .RuleFor(a => a.Title, "title")
             .RuleFor(a => a.TypeDocId, 1)
             .RuleFor(a => a.EmailCreator, f => f.Person.Email)
             .RuleFor(a => a.TypeDoc, typeDoc)
             .RuleFor(a => a.TypeDocName, "name")
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
                Workflows: new List<int> { 10 }
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

        public static Team FindValidTeam()
        {
            Team team = new Faker<Team>("pt_BR")
            .CustomInstantiator(f => new Team(
                f.Company.CompanyName(),
                f.IndexFaker,
                f.Date.Past()
            ));

            var user = new User(
                        Guid.Parse("20c41dd6-1518-468b-8b0c-b5d8c0d31dec"),
                        "Name",
                        "Mail",
                        true,
                        DateTime.Now
                );
            team.AddUser(user);
            return team;
        }

        public static Workflow FindValidWorkflow()
        {
            Workflow workflow = new Faker<Workflow>("pt_BR")
            .CustomInstantiator(f => new Workflow(
                    f.IndexFaker,
                    f.Date.Past(),
                    new List<Team> { FindValidTeam() },
                    f.Lorem.Word()
                )
            );
            return workflow;
        }

        public static Workflow FindValidWorkflowList()
        {
            Workflow workflow = new Faker<Workflow>("pt_BR")
            .CustomInstantiator(f => new Workflow(
                    f.IndexFaker,
                    f.Date.Past(),
                    new List<Team> { FindValidTeam() },
                    f.Lorem.Word()
                )
            );
            return workflow;
        }

        public static Step FindValidStep()
        {
            Step step = new Faker<Step>("pt_BR")
            .CustomInstantiator(f => new Step(
                    f.IndexFaker,
                    f.Date.Past(),
                    f.IndexFaker,
                    f.Lorem.Word(),
                    f.Random.Int(1, 1),
                    f.IndexFaker,
                    f.IndexFaker
                )
            );
            return step;
        }

        public static ProcessOcrResultDto FindValidProcessOcrResultDto()
        {
            var faker = new Faker<ProcessOcrResultDto>("pt_BR")
                .CustomInstantiator(f => new ProcessOcrResultDto
                {
                    ReferenceFile = f.Random.String(),
                    Tenant = f.Random.String(),
                    Email = f.Random.String(),
                    AnalyzeResult = FindValidAnalyzeResultCustomDto(),
                    Data = new MetaDataAutomationDto(361, 456)
                });
            return faker;
        }

        public static MetaDataAutomationDto FindValidProcessOcrDataAutomationDto()
        {
            var faker = new Faker("pt_BR");
            var dto = new MetaDataAutomationDto
            {
                CardId = 361,
                StepToolId = 456,

            };
            return dto;
        }
        public static StepToolExecution FindValidStepToolExecution()
        {
            var guid = Guid.NewGuid();
            var faker = new Faker("pt_BR");
            var card = new Card(1, DateTime.Now, 456, 1, "name", 1, true, guid);
            var stepTool = new StepTool(1, DateTime.Now, 456, 456, 456, 1, 1);
            var execution = new StepToolExecution
            (
                 1,
                 DateTime.Now,
                 456,
                 StatusExecution.Running,
                 361
            )
            {
                Card = card,
                StepTool = stepTool
            };
            return execution;
        }

        public static AnalyzeResultCustomDto FindValidAnalyzeResultCustomDto()
        {
            var faker = new Faker<AnalyzeResultCustomDto>("pt_BR")
                .CustomInstantiator(f => new AnalyzeResultCustomDto
                {
                    Pages = new List<CustomDocumentPage>
                    {
                        new CustomDocumentPage
                        {
                            PageNumber = 1,
                            Lines = new List<CustomDocumentLine>
                            {
                                new CustomDocumentLine { Content = "Line 1" },
                                new CustomDocumentLine { Content = "Line 2" }
                            }
                        }
                    },
                    Tables = new List<CustomDocumentTable>() { new CustomDocumentTable(){
                        BoundingRegions = new List<BoundingRegionCustom> { new BoundingRegionCustom { PageNumber = 1 } },
                        Cells = new List<CustomDocumentTableCell>
                            {
                                new CustomDocumentTableCell
                                {
                                    ColumnIndex = 0,
                                    RowIndex = 0,
                                    Content = "Cell 1"
                                },
                                new CustomDocumentTableCell
                                {
                                    ColumnIndex = 1,
                                    RowIndex = 0,
                                    Content = "Cell 2"
                                }
                            }
                        }
                    }
                });
            return faker;
        }

        public static DocumentEmbeddingsResultDto FindValidDocumentEmbeddingsResultDto()
        {
            var faker = new Faker<DocumentEmbeddingsResultDto>("pt_BR")
                .CustomInstantiator(f => new DocumentEmbeddingsResultDto
                {
                    ReferenceFile = f.Random.String(),
                    Tenant = f.Random.String(),
                    Email = f.Random.String(),
                    KeyMongoAccess = f.Random.String(),
                    TotalPages = f.Random.Int(1, 100),
                    Data = new MetaDataAutomationDto(361, 456)
                });
            return faker;
        }
    }

    [CollectionDefinition(nameof(DocumentCollection))]
    public class DocumentCollection : ICollectionFixture<DocumentFixture>
    {
    }
}
