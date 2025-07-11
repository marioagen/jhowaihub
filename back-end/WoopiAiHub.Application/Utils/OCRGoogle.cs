using Google.Cloud.Vision.V1;
using Google.Protobuf;
using WoopiAiHub.Domain.Interfaces.Services;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Text;

namespace WoopiAiHub.Application.Utils
{
    public class OcrGoogle : IOcrGoogle
    {
        /// <summary>
        /// Performs Google OCR processing
        /// </summary>
        /// <param name="bytesFile"></param>
        /// <returns></returns>
        public async Task<ICollection<string>> ProcessResult(byte[] bytesFile)
        {
            List<byte[]> pdfParts = SplitPDF(bytesFile, 5);
            var texts = new List<string>();
            int pageNumber = 1;

            foreach (var pdfPart in pdfParts)
            {
                var response = await this.SendRequest(pdfPart);
                this.FormatTextResponse(response,
                                        ref texts,
                                        ref pageNumber);
            }

            return texts;
        }

        /// <summary>
        /// Splits the PDF into parts of 5 pages, as this is the maximum that Google OCR supports.
        /// </summary>
        /// <param name="pdfBytes"></param>
        /// <param name="pagesPerPart"></param>
        /// <returns></returns>
        private List<byte[]> SplitPDF(byte[] pdfBytes,
                                      int pagesPerPart)
        {
            List<byte[]> pdfParts = new List<byte[]>();

            using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
            using (PdfDocument inputDocument = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import))
            {
                int pageCount = inputDocument.PageCount;

                for (int i = 0; i < pageCount; i += pagesPerPart)
                {
                    int startPage = i;
                    int endPage = Math.Min(i + pagesPerPart, pageCount);

                    using (MemoryStream partStream = new MemoryStream())
                    {
                        using (PdfDocument outputDocument = new PdfDocument())
                        {
                            for (int page = startPage; page < endPage; page++)
                            {
                                outputDocument.AddPage(inputDocument.Pages[page]);
                            }

                            outputDocument.Save(partStream);
                        }

                        pdfParts.Add(partStream.ToArray());
                    }
                }
            }

            return pdfParts;
        }

        /// <summary>
        /// Send the request to Google to obtain the text
        /// </summary>
        /// <param name="pdfPart"></param>
        /// <returns></returns>
        private async Task<BatchAnnotateFilesResponse> SendRequest(Byte[] pdfPart)
        {
            var client = await ImageAnnotatorClient.CreateAsync();

            var contentByte = ByteString.CopyFrom(pdfPart);

            var syncRequest = new AnnotateFileRequest
            {
                InputConfig = new InputConfig
                {
                    Content = contentByte,
                    MimeType = "application/pdf"
                }
            };

            syncRequest.Features.Add(new Feature
            {
                Type = Feature.Types.Type.DocumentTextDetection
            });

            List<AnnotateFileRequest> requests = new List<AnnotateFileRequest>
                {
                    syncRequest
                };

            var response = await client.BatchAnnotateFilesAsync(requests);

            return response;
        }

        /// <summary>
        /// Formats the text returned by Google and sorts the result
        /// </summary>
        /// <param name="response"></param>
        /// <param name="texts"></param>
        /// <param name="pageNumber"></param>
        private void FormatTextResponse(BatchAnnotateFilesResponse response,
                                        ref List<string> texts,
                                        ref int pageNumber)
        {
            foreach (var fullTextAnnotation in response.Responses[0].Responses.Select(u => u.FullTextAnnotation))
            {
                if (fullTextAnnotation != null)
                {
                    var sortedBlocks = fullTextAnnotation.Pages[0].Blocks
                                                         .OrderBy(block => block.BoundingBox.NormalizedVertices[0].Y) // Ordenar pelo canto superior esquerdo Y
                                                         .ToList();


                    StringBuilder pageText = new StringBuilder();
                    pageText.AppendLine($"----------- Página {pageNumber} do PDF -----------\n");

                    foreach (var block in sortedBlocks)
                    {
                        foreach (var paragraph in block.Paragraphs)
                        {
                            var words = paragraph.Words.Select(word =>
                                string.Join("", word.Symbols.Select(symbol => symbol.Text)));

                            var paragraphText = string.Join(" ", words);
                            pageText.AppendLine(paragraphText);
                        }
                    }

                    pageNumber++;
                    texts.Add(pageText.ToString());
                }
            }
        }
    }
}
