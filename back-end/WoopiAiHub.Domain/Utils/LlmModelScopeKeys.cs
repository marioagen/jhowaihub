using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.Utils
{
    public static class LlmModelScopeKeys
    {
        public const string Agents = "agents";
        public const string Questionnaires = "questionnaires";
        public const string Documents = "documents";
        public const string Mcp = "mcp";
        public const string Chat = "chat";

        public static readonly IReadOnlyList<string> All =
        [
            Agents,
            Questionnaires,
            Documents,
            Mcp,
            Chat,
        ];

        public static string ToScopeKey(LlmModelScope scope) =>
            scope switch
            {
                LlmModelScope.Agents => Agents,
                LlmModelScope.Questionnaires => Questionnaires,
                LlmModelScope.Documents => Documents,
                LlmModelScope.Mcp => Mcp,
                LlmModelScope.Chat => Chat,
                _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null),
            };

        public static LlmModelScope FromScopeKey(string scopeKey) =>
            scopeKey switch
            {
                Agents => LlmModelScope.Agents,
                Questionnaires => LlmModelScope.Questionnaires,
                Documents => LlmModelScope.Documents,
                Mcp => LlmModelScope.Mcp,
                Chat => LlmModelScope.Chat,
                _ => throw new ArgumentException($"Invalid LLM model scope: {scopeKey}", nameof(scopeKey)),
            };

        public static bool IsValid(string scopeKey) => All.Contains(scopeKey);
    }
}
