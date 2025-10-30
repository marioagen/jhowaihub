namespace WoopiAiHub.Domain.DTOs.Response
{
    /// <summary>
    /// Response DTO containing the concatenated OCR text from a document
    /// </summary>
    public record OcrTextResponseDto
    {
        /// <summary>
        /// Full concatenated OCR text from all pages
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether OCR has been executed and is ready
        /// </summary>
        public bool HasOcr { get; set; }

        /// <summary>
        /// Reference file identifier
        /// </summary>
        public string ReferenceFile { get; set; } = string.Empty;
    }
}
