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
}
