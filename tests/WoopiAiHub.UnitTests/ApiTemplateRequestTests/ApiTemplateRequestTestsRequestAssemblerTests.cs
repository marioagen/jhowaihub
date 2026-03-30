using WoopiAiHub.Application.ApiTemplateRequestTests;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.ApiTemplateRequestTests
{
    public class ApiTemplateRequestTestsRequestAssemblerTests
    {
        private static Dictionary<string, string> Vars(params (string Key, string Value)[] pairs) =>
            pairs.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);

        [Fact(DisplayName = "Assemble should normalize method to upper trimmed")]
        [Trait("Assemble", "Success")]
        public void Assemble_NormalizesMethod_ToUpperTrimmed()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "  post  ",
                "https://api.example.com/r",
                null,
                null,
                null,
                Vars());

            Assert.Equal("POST", r.Method);
        }

        [Fact(DisplayName = "Assemble should treat empty or whitespace method as empty string")]
        [Trait("Assemble", "Success")]
        public void Assemble_EmptyOrWhitespaceMethod_BecomesEmptyString()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "   ",
                "https://api.example.com/r",
                null,
                null,
                null,
                Vars());

            Assert.Equal(string.Empty, r.Method);
        }

        [Fact(DisplayName = "Assemble should substitute variables in URL, query, header and body")]
        [Trait("Assemble", "Success")]
        public void Assemble_SubstitutesVariables_InUrlQueryHeaderAndBody()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/v1/{{id}}",
                """[{"key":"q","value":"{{qval}}"}]""",
                """[{"key":"X-T","value":"{{h}}"}]""",
                """{"x":"{{b}}"}""",
                Vars(("id", "7"), ("qval", "qq"), ("h", "hv"), ("b", "bv")));

            Assert.Equal("https://api.example.com/v1/7", r.Url.Split('?')[0]);
            Assert.Contains("q=qq", r.Url, StringComparison.Ordinal);
            Assert.NotNull(r.Headers);
            Assert.Equal("hv", r.Headers["X-T"]);
            Assert.Equal("""{"x":"bv"}""", r.Body);
        }

        [Fact(DisplayName = "Assemble should replace placeholder with empty when variable value is empty")]
        [Trait("Assemble", "Success")]
        public void Assemble_VariableValueEmpty_ReplacesPlaceholderWithEmpty()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/{{x}}end",
                null,
                null,
                null,
                Vars(("x", string.Empty)));

            Assert.Equal("https://api.example.com/end", r.Url);
        }

        [Fact(DisplayName = "Assemble should return nulls for optional parts when query, header and body are null")]
        [Trait("Assemble", "Success")]
        public void Assemble_NullQueryHeaderBody_ReturnsNullsForOptionalParts()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/z",
                null,
                null,
                null,
                Vars());

            Assert.False(r.Url.Contains('?', StringComparison.Ordinal));
            Assert.Null(r.Headers);
            Assert.Null(r.Body);
        }

        [Fact(DisplayName = "Assemble should leave URL unchanged for empty query JSON array")]
        [Trait("Assemble", "Success")]
        public void Assemble_QueryTemplate_EmptyJsonArray_LeavesUrlUnchanged()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/a",
                "[]",
                null,
                null,
                Vars());

            Assert.Equal("https://api.example.com/a", r.Url);
        }

        [Fact(DisplayName = "Assemble should leave URL unchanged when query template JSON is null")]
        [Trait("Assemble", "Success")]
        public void Assemble_QueryTemplate_JsonNull_LeavesUrlUnchanged()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/a",
                "null",
                null,
                null,
                Vars());

            Assert.Equal("https://api.example.com/a", r.Url);
        }

        [Fact(DisplayName = "Assemble should leave URL unchanged when query template has only blank keys")]
        [Trait("Assemble", "Success")]
        public void Assemble_QueryTemplate_OnlyBlankKeys_LeavesUrlUnchanged()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/a",
                """[{"key":"","value":"1"},{"key":" ","value":"2"}]""",
                null,
                null,
                Vars());

            Assert.Equal("https://api.example.com/a", r.Url);
            Assert.False(r.Url.Contains('?', StringComparison.Ordinal));
        }

        [Fact(DisplayName = "Assemble should return null headers for empty header JSON array")]
        [Trait("Assemble", "Success")]
        public void Assemble_HeaderTemplate_EmptyJsonArray_ReturnsNullHeaders()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/a",
                null,
                "[]",
                null,
                Vars());

            Assert.Null(r.Headers);
        }

        [Fact(DisplayName = "Assemble should return null headers when header template JSON is null")]
        [Trait("Assemble", "Success")]
        public void Assemble_HeaderTemplate_JsonNull_ReturnsNullHeaders()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/a",
                null,
                "null",
                null,
                Vars());

            Assert.Null(r.Headers);
        }

        [Fact(DisplayName = "Assemble should return null headers when header template has only blank keys")]
        [Trait("Assemble", "Success")]
        public void Assemble_HeaderTemplate_OnlyBlankKeys_ReturnsNullHeaders()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/a",
                null,
                """[{"key":"","value":"x"},{"key":"  ","value":"y"}]""",
                null,
                Vars());

            Assert.Null(r.Headers);
        }

        [Fact(DisplayName = "Assemble should skip blank keys in query template")]
        [Trait("Assemble", "Success")]
        public void Assemble_QueryTemplate_SkipsBlankKeys()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/b",
                """[{"key":"","value":"1"},{"key":"ok","value":"2"}]""",
                null,
                null,
                Vars());

            Assert.DoesNotContain("=1", r.Url, StringComparison.Ordinal);
            Assert.Contains("ok=2", r.Url, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "Assemble should merge header template keys case-insensitively")]
        [Trait("Assemble", "Success")]
        public void Assemble_HeaderTemplate_CaseInsensitiveKeys()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/c",
                null,
                """[{"key":"Authorization","value":"a"},{"key":"authorization","value":"b"}]""",
                null,
                Vars());

            Assert.NotNull(r.Headers);
            Assert.Single(r.Headers);
            Assert.Equal("b", r.Headers["authorization"]);
        }

        [Fact(DisplayName = "Assemble should treat null header value as empty string")]
        [Trait("Assemble", "Success")]
        public void Assemble_HeaderTemplate_NullValue_BecomesEmptyString()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/d",
                null,
                """[{"key":"X","value":null}]""",
                null,
                Vars());

            Assert.NotNull(r.Headers);
            Assert.Equal(string.Empty, r.Headers["X"]);
        }

        [Fact(DisplayName = "Assemble should throw for invalid query template JSON")]
        [Trait("Assemble", "Fail")]
        public void Assemble_InvalidQueryTemplateJson_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                ApiTemplateRequestTestsRequestAssembler.Assemble(
                    "GET",
                    "https://api.example.com/e",
                    "not-json",
                    null,
                    null,
                    Vars()));

            Assert.Contains("Query template", ex.Message, StringComparison.Ordinal);
            Assert.IsType<System.Text.Json.JsonException>(ex.InnerException);
        }

        [Fact(DisplayName = "Assemble should throw for invalid header template JSON")]
        [Trait("Assemble", "Fail")]
        public void Assemble_InvalidHeaderTemplateJson_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                ApiTemplateRequestTestsRequestAssembler.Assemble(
                    "GET",
                    "https://api.example.com/f",
                    null,
                    "{",
                    null,
                    Vars()));

            Assert.Contains("Header template", ex.Message, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "Assemble should throw when URL is not absolute after substitution")]
        [Trait("Assemble", "Fail")]
        public void Assemble_UrlNotAbsoluteAfterVariables_Throws()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                ApiTemplateRequestTestsRequestAssembler.Assemble(
                    "GET",
                    "/relative",
                    null,
                    null,
                    null,
                    Vars()));

            Assert.Contains("absolute URI", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "Assemble should skip query merge for whitespace-only query template")]
        [Trait("Assemble", "Success")]
        public void Assemble_WhitespaceOnlyQueryTemplate_SkipsMerge()
        {
            var r = ApiTemplateRequestTestsRequestAssembler.Assemble(
                "GET",
                "https://api.example.com/g",
                "   ",
                null,
                null,
                Vars());

            Assert.Equal("https://api.example.com/g", r.Url);
        }

        [Fact(DisplayName = "ToDraft should map all fields from ApiTemplate")]
        [Trait("ToDraft", "Success")]
        public void ToDraft_MapsAllFields_FromApiTemplate()
        {
            var model = new ApiTemplate(
                "N1",
                "POST",
                "https://x/",
                """[{"key":"q","value":"1"}]""",
                """[{"key":"H","value":"v"}]""",
                "{}");

            var dto = ApiTemplateRequestTestsRequestAssembler.ToDraft(model);

            Assert.Equal("N1", dto.Name);
            Assert.Equal("POST", dto.Method);
            Assert.Equal("https://x/", dto.Url);
            Assert.Equal("""[{"key":"q","value":"1"}]""", dto.QueryTemplate);
            Assert.Equal("""[{"key":"H","value":"v"}]""", dto.HeaderTemplate);
            Assert.Equal("{}", dto.BodyTemplate);
        }
    }
}
