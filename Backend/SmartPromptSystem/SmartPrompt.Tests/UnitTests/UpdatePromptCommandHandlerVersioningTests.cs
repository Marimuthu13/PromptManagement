using Microsoft.EntityFrameworkCore;
using Moq;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Features.Prompts.Commands.UpdatePrompt;
using SmartPrompt.Domain.Entities;
using SmartPrompt.Infrastructure.Persistence;
using Xunit;

namespace SmartPrompt.Tests.UnitTests;

public class UpdatePromptCommandHandlerVersioningTests
{
    private readonly SmartPromptDbContext _context;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdatePromptCommandHandlerVersioningTests()
    {
        var options = new DbContextOptionsBuilder<SmartPromptDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SmartPromptDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ContentChanged_CreatesNewVersion()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Test" };
        var prompt = new Prompt
        {
            Id = Guid.NewGuid(),
            Title = "Old Title",
            Description = "Old Desc",
            Content = "Old Content",
            CategoryId = category.Id,
            UserId = _userId
        };

        _context.Categories.Add(category);
        _context.Prompts.Add(prompt);
        await _context.SaveChangesAsync();

        var handler = new UpdatePromptCommandHandler(_context, _currentUserMock.Object);
        var command = new UpdatePromptCommand
        {
            Id = prompt.Id,
            Title = "New Title",
            Description = "New Desc",
            Content = "New Content",
            CategoryId = category.Id
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var versions = await _context.PromptVersions.Where(v => v.PromptId == prompt.Id).ToListAsync();
        Assert.Single(versions);
        Assert.Equal("Old Content", versions.First().Content);
        Assert.Equal(1, versions.First().VersionNumber);

        var updatedPrompt = await _context.Prompts.FindAsync(prompt.Id);
        Assert.Equal("New Content", updatedPrompt!.Content);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ContentUnchanged_DoesNotCreateVersion()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Test" };
        var prompt = new Prompt
        {
            Id = Guid.NewGuid(),
            Title = "Old Title",
            Description = "Old Desc",
            Content = "Same Content",
            CategoryId = category.Id,
            UserId = _userId
        };

        _context.Categories.Add(category);
        _context.Prompts.Add(prompt);
        await _context.SaveChangesAsync();

        var handler = new UpdatePromptCommandHandler(_context, _currentUserMock.Object);
        var command = new UpdatePromptCommand
        {
            Id = prompt.Id,
            Title = "New Title",
            Description = "New Desc",
            Content = "Same Content", // Only changing title/desc
            CategoryId = category.Id
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var versions = await _context.PromptVersions.Where(v => v.PromptId == prompt.Id).ToListAsync();
        Assert.Empty(versions); // No version should be created

        var updatedPrompt = await _context.Prompts.FindAsync(prompt.Id);
        Assert.Equal("Same Content", updatedPrompt!.Content);
        Assert.Equal("New Title", updatedPrompt.Title);
    }
}
