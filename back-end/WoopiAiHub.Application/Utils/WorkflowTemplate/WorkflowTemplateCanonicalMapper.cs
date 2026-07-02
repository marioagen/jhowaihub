using System.Text.RegularExpressions;

namespace WoopiAiHub.Application.Utils.WorkflowTemplate
{
    public static class WorkflowTemplateCanonicalMapper
    {
        private static readonly Dictionary<string, string> KnownTeamCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = "ADMIN",
        };

        private static readonly Dictionary<string, string> KnownProfileCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = "ADMIN",
            ["IA"] = "IA",
            ["Analista"] = "ANALYST",
        };

        private static readonly Dictionary<string, string> KnownStatusCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["AwaitingAnalysis"] = "AWAITING_ANALYSIS",
            ["Analyzed"] = "ANALYZED",
            ["WaitingForApproval"] = "WAITING_FOR_APPROVAL",
            ["Approved"] = "APPROVED",
            ["Done"] = "DONE",
            ["Rejected"] = "REJECTED",
            ["Fail"] = "FAIL",
        };

        public static string ToTeamCode(string name) =>
            KnownTeamCodes.TryGetValue(name, out var code) ? code : SlugToCode(name);

        public static string ToProfileCode(string name) =>
            KnownProfileCodes.TryGetValue(name, out var code) ? code : SlugToCode(name);

        public static string ToStatusCode(string name) =>
            KnownStatusCodes.TryGetValue(name, out var code) ? code : SlugToCode(name);

        public static int ResolveTeamId(
            IEnumerable<(int Id, string Name)> teams,
            IReadOnlyList<string> teamCodes,
            IReadOnlyList<string> teamNames)
        {
            var byCode = teams.ToDictionary(t => ToTeamCode(t.Name), t => t.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var code in teamCodes)
            {
                if (!string.IsNullOrWhiteSpace(code) && byCode.TryGetValue(code, out var id))
                    return id;
            }

            foreach (var name in teamNames)
            {
                var match = teams.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (match.Id != 0)
                    return match.Id;
            }

            return 0;
        }

        public static int ResolveProfileId(
            IEnumerable<(int Id, string Name)> profiles,
            string? profileCode,
            string profileName,
            string fallbackName = "Admin")
        {
            var byCode = profiles.ToDictionary(p => ToProfileCode(p.Name), p => p.Id, StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(profileCode) && byCode.TryGetValue(profileCode, out var byCodeId))
                return byCodeId;

            if (!string.IsNullOrWhiteSpace(profileName))
            {
                var match = profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
                if (match.Id != 0)
                    return match.Id;
            }

            if (byCode.TryGetValue(ToProfileCode(fallbackName), out var fallbackId))
                return fallbackId;

            return profiles.First().Id;
        }

        public static int ResolveStatusId(
            IEnumerable<(int Id, string Name)> statuses,
            string? statusCode,
            string statusName,
            string fallbackName = "AwaitingAnalysis")
        {
            var byCode = statuses.ToDictionary(s => ToStatusCode(s.Name), s => s.Id, StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(statusCode) && byCode.TryGetValue(statusCode, out var byCodeId))
                return byCodeId;

            if (!string.IsNullOrWhiteSpace(statusName))
            {
                var match = statuses.FirstOrDefault(s => s.Name.Equals(statusName, StringComparison.OrdinalIgnoreCase));
                if (match.Id != 0)
                    return match.Id;
            }

            if (byCode.TryGetValue(ToStatusCode(fallbackName), out var fallbackId))
                return fallbackId;

            return statuses.First().Id;
        }

        private static string SlugToCode(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var slug = Regex.Replace(name.Trim(), @"[^A-Za-z0-9]+", "_").Trim('_');
            return slug.ToUpperInvariant();
        }
    }
}
