namespace WoopiAiHub.Domain.Utils.ErrorLabels
{
    public static class ToolLabel
    {
        public const string NotFound = "tools.toolNotFound";
        public const string DependencyToolNotFound = "tools.dependencyToolNotFound";
        public const string DependecyRequired = "tools.dependencyRequired";
        public const string OcrDependencyRequired = "tools.ocrDependencyRequired";
        public const string OcrOrPromptDependencyRequired = "tools.ocrOrPromptDependencyRequired";
        public const string PromptApiOrQuizDependencyRequired = "tools.promptApiOrQuizDependencyRequired";
        public const string EmbeddingDependencyRequired = "tools.embeddingDependencyRequired";
        public const string QuizDependencyRequired = "tools.quizDependencyRequired";
    }
}
