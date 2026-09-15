using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models.AI;
using SmartPrompt.Application.Features.Prompts.Commands.ExecutePrompt;
using SmartPrompt.Domain.Entities;
using SmartPrompt.Infrastructure.Persistence;
using Xunit;

namespace SmartPrompt.Tests.UnitTests;

public class ExecutePromptCommandHandlerTests
{
    private readonly SmartPromptDbContext _context;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAIProvider> _aiProviderMock;
    private readonly Guid _userId = Guid.NewGuid();

    public ExecutePromptCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SmartPromptDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SmartPromptDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);

        _aiProviderMock = new Mock<IAIProvider>();
        _aiProviderMock.Setup(p => p.ProviderName).Returns("MockProvider");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ValidRequest_ExecutesAndSavesSuccess()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Test" };
        var prompt = new Prompt
        {
            Id = Guid.NewGuid(),
            Title = "My Prompt",
            Content = "Hello {Name}",
            CategoryId = category.Id,
            UserId = _userId
        };
        prompt.Variables.Add(new PromptVariable { Name = "Name", IsRequired = true });

        _context.Categories.Add(category);
        _context.Prompts.Add(prompt);
        await _context.SaveChangesAsync();

        _aiProviderMock.Setup(p => p.ExecutePromptAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIResponse
            {
                Content = "Hi there!",
                TotalTokens = 10,
                Duration = TimeSpan.FromMilliseconds(500)
            });

        var handler = new ExecutePromptCommandHandler(_context, _currentUserMock.Object, new[] { _aiProviderMock.Object });
        var command = new ExecutePromptCommand
        {
            PromptId = prompt.Id,
            ProviderName = "MockProvider",
            ModelName = "gpt-test",
            Variables = new Dictionary<string, string> { { "Name", "World" } }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal("Hi there!", result.ResultContent);
        Assert.Equal(10, result.TokensUsed);
        
        var savedExecution = await _context.PromptExecutions.FirstOrDefaultAsync(e => e.Id == result.Id);
        Assert.NotNull(savedExecution);
        Assert.True(savedExecution.IsSuccessful);
        Assert.Equal("Hi there!", savedExecution.ResultContent);

        _aiProviderMock.Verify(p => p.ExecutePromptAsync(It.Is<AIRequest>(req => req.UserPrompt == "Hello World"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_MissingRequiredVariable_ThrowsValidationException()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Test" };
        var prompt = new Prompt
        {
            Id = Guid.NewGuid(),
            Title = "My Prompt",
            Content = "Hello {Name}",
            CategoryId = category.Id,
            UserId = _userId
        };
        prompt.Variables.Add(new PromptVariable { Name = "Name", IsRequired = true });

        _context.Categories.Add(category);
        _context.Prompts.Add(prompt);
        await _context.SaveChangesAsync();

        var handler = new ExecutePromptCommandHandler(_context, _currentUserMock.Object, new[] { _aiProviderMock.Object });
        var command = new ExecutePromptCommand
        {
            PromptId = prompt.Id,
            ProviderName = "MockProvider",
            ModelName = "gpt-test",
            Variables = new Dictionary<string, string>() // Missing Name
        };

        // Act & Assert
        await Assert.ThrowsAsync<SmartPrompt.Application.Common.Exceptions.ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ProviderThrowsException_SavesFailedExecution()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Test" };
        var prompt = new Prompt
        {
            Id = Guid.NewGuid(),
            Title = "My Prompt",
            Content = "Test",
            CategoryId = category.Id,
            UserId = _userId
        };

        _context.Categories.Add(category);
        _context.Prompts.Add(prompt);
        await _context.SaveChangesAsync();

        _aiProviderMock.Setup(p => p.ExecutePromptAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API rate limit exceeded"));

        var handler = new ExecutePromptCommandHandler(_context, _currentUserMock.Object, new[] { _aiProviderMock.Object });
        var command = new ExecutePromptCommand
        {
            PromptId = prompt.Id,
            ProviderName = "MockProvider",
            ModelName = "gpt-test"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("API rate limit exceeded", result.ErrorMessage);
        Assert.Empty(result.ResultContent);

        var savedExecution = await _context.PromptExecutions.FirstOrDefaultAsync(e => e.Id == result.Id);
        Assert.NotNull(savedExecution);
        Assert.False(savedExecution.IsSuccessful);
        Assert.Equal("API rate limit exceeded", savedExecution.ErrorMessage);
    }
}
