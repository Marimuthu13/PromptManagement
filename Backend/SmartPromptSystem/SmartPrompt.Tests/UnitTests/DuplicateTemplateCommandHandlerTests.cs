using Microsoft.EntityFrameworkCore;
using Moq;
using SmartPrompt.Application.Common.Exceptions;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Features.Templates.Commands.DuplicateTemplate;
using SmartPrompt.Domain.Entities;
using SmartPrompt.Infrastructure.Persistence;
using Xunit;

namespace SmartPrompt.Tests.UnitTests;

public class DuplicateTemplateCommandHandlerTests
{
    private readonly SmartPromptDbContext _context;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Guid _userId = Guid.NewGuid();

    public DuplicateTemplateCommandHandlerTests()
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
    public async Task Handle_TemplateNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new DuplicateTemplateCommandHandler(_context, _currentUserMock.Object);
        var command = new DuplicateTemplateCommand { TemplateId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ValidTemplate_DuplicatesAndExtractsVariables()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var template = new PromptTemplate
        {
            Id = Guid.NewGuid(),
            Title = "Base Template",
            Description = "A cool template",
            Content = "Hello {name}, your {item} is ready.",
            CategoryId = categoryId,
            IsSystemCurated = true
        };

        _context.PromptTemplates.Add(template);
        await _context.SaveChangesAsync();

        var handler = new DuplicateTemplateCommandHandler(_context, _currentUserMock.Object);
        var command = new DuplicateTemplateCommand { TemplateId = template.Id };

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var createdPrompt = await _context.Prompts
            .Include(p => p.Variables)
            .FirstOrDefaultAsync(p => p.Id == resultId);

        Assert.NotNull(createdPrompt);
        Assert.Equal("Base Template (Copy)", createdPrompt.Title);
        Assert.Equal("A cool template", createdPrompt.Description);
        Assert.Equal("Hello {name}, your {item} is ready.", createdPrompt.Content);
        Assert.Equal(categoryId, createdPrompt.CategoryId);
        Assert.Equal(_userId, createdPrompt.UserId);

        Assert.Equal(2, createdPrompt.Variables.Count);
        Assert.Contains(createdPrompt.Variables, v => v.Name == "name" && v.IsRequired);
        Assert.Contains(createdPrompt.Variables, v => v.Name == "item" && v.IsRequired);
    }
}
