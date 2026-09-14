using SmartPrompt.Application.Common.Utils;
using Xunit;

namespace SmartPrompt.Tests.UnitTests;

public class PromptVariableParserTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractVariables_EmptyOrNullString_ReturnsEmpty()
    {
        // Act
        var resultNull = PromptVariableParser.ExtractVariables(null!);
        var resultEmpty = PromptVariableParser.ExtractVariables("");
        var resultWhitespace = PromptVariableParser.ExtractVariables("   ");

        // Assert
        Assert.Empty(resultNull);
        Assert.Empty(resultEmpty);
        Assert.Empty(resultWhitespace);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractVariables_NoVariables_ReturnsEmpty()
    {
        // Arrange
        var content = "This is a prompt with no variables.";

        // Act
        var result = PromptVariableParser.ExtractVariables(content);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractVariables_SingleVariable_ReturnsVariable()
    {
        // Arrange
        var content = "Hello {name}, how are you?";

        // Act
        var result = PromptVariableParser.ExtractVariables(content).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("name", result[0]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractVariables_MultipleUniqueVariables_ReturnsAll()
    {
        // Arrange
        var content = "Write a {tone} email to {customer_name} about {topic}.";

        // Act
        var result = PromptVariableParser.ExtractVariables(content).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("tone", result);
        Assert.Contains("customer_name", result);
        Assert.Contains("topic", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractVariables_DuplicateVariables_ReturnsDistinct()
    {
        // Arrange
        var content = "Hello {name}. I said hello to {name}. Also {age} is here.";

        // Act
        var result = PromptVariableParser.ExtractVariables(content).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("name", result);
        Assert.Contains("age", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExtractVariables_InvalidVariableFormat_IgnoresInvalid()
    {
        // Arrange
        // Variables contain invalid characters like spaces or dashes not allowed in our regex
        var content = "Hello {invalid name} and {dashed-name} and {valid_name}.";

        // Act
        var result = PromptVariableParser.ExtractVariables(content).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("valid_name", result[0]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SubstituteVariables_VariablesMatch_ReturnsSubstitutedString()
    {
        var content = "Hello {name}, your code is {code}.";
        var vars = new Dictionary<string, string>
        {
            { "name", "Alice" },
            { "code", "1234" }
        };

        var result = PromptVariableParser.SubstituteVariables(content, vars);

        Assert.Equal("Hello Alice, your code is 1234.", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SubstituteVariables_MissingVariable_LeavesPlaceholder()
    {
        var content = "Hello {name}, your code is {code}.";
        var vars = new Dictionary<string, string>
        {
            { "name", "Alice" }
            // 'code' is missing
        };

        var result = PromptVariableParser.SubstituteVariables(content, vars);

        Assert.Equal("Hello Alice, your code is {code}.", result);
    }
}
