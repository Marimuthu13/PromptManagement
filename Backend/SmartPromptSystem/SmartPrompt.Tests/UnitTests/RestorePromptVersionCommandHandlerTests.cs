using Microsoft.EntityFrameworkCore;
using Moq;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Features.Prompts.Commands.RestorePromptVersion;
using SmartPrompt.Domain.Entities;
using SmartPrompt.Infrastructure.Persistence;
using Xunit;

namespace SmartPrompt.Tests.UnitTests;

public class RestorePromptVersionCommandHandlerTests
{
    private readonly SmartPromptDbContext _context;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Guid _userId = Guid.NewGuid();

    public RestorePromptVersionCommandHandlerTests()
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
    public async Task Handle_ValidRestore_SavesCurrentAsVersionAndRestores()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Test" };
        var prompt = new Prompt
        {
            Id = Guid.NewGuid(),
            Title = "My Prompt",
            Description = "Desc",
            Content = "Current Content",
            CategoryId = category.Id,
            UserId = _userId
        };

        var version = new PromptVersion
        {
            Id = Guid.NewGuid(),
            PromptId = prompt.Id,
            Content = "V1 Content",
            VersionNumber = 1
        };

        _context.Categories.Add(category);
        _context.Prompts.Add(prompt);
        _context.PromptVersions.Add(version);
        await _context.SaveChangesAsync();

        var handler = new RestorePromptVersionCommandHandler(_context, _currentUserMock.Object);
        var command = new RestorePromptVersionCommand
        {
            PromptId = prompt.Id,
            VersionId = version.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("V1 Content", result.Content);

        var promptInDb = await _context.Prompts.FindAsync(prompt.Id);
        Assert.Equal("V1 Content", promptInDb!.Content);

        // Check that "Current Content" was saved as version 2
        var versions = await _context.PromptVersions.Where(v => v.PromptId == prompt.Id).OrderBy(v => v.VersionNumber).ToListAsync();
        Assert.Equal(2, versions.Count);
        Assert.Equal("Current Content", versions.Last().Content);
        Assert.Equal(2, versions.Last().VersionNumber);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_DifferentUser_ThrowsNotFoundException()
    {
        // Arrange
        var prompt = new Prompt
        {
            Id = Guid.NewGuid(),
            Title = "My Prompt",
            Content = "Current Content",
            UserId = Guid.NewGuid() // Different user!
        };
        _context.Prompts.Add(prompt);
        await _context.SaveChangesAsync();

        var handler = new RestorePromptVersionCommandHandler(_context, _currentUserMock.Object);
        var command = new RestorePromptVersionCommand
        {
            PromptId = prompt.Id,
            VersionId = Guid.NewGuid()
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
