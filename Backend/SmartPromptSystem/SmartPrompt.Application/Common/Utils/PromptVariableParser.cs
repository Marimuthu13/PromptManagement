using System.Text.RegularExpressions;

namespace SmartPrompt.Application.Common.Utils;

public static class PromptVariableParser
{
    private static readonly Regex VariableRegex = new(@"\{([a-zA-Z0-9_]+)\}", RegexOptions.Compiled);

    public static IEnumerable<string> ExtractVariables(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Enumerable.Empty<string>();
        }

        var matches = VariableRegex.Matches(content);
        return matches
            .Select(m => m.Groups[1].Value)
            .Distinct();
    }

    public static string SubstituteVariables(string content, IDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        if (variables == null || !variables.Any())
        {
            return content;
        }

        return VariableRegex.Replace(content, match =>
        {
            var varName = match.Groups[1].Value;
            if (variables.TryGetValue(varName, out var value))
            {
                return value;
            }
            return match.Value; // Leave unchanged if not provided
        });
    }
}
