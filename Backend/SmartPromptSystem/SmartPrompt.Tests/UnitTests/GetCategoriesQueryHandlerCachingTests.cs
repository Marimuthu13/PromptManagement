using Microsoft.EntityFrameworkCore;
using Moq;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Features.Categories.Queries.GetCategories;
using SmartPrompt.Domain.Entities;
using SmartPrompt.Infrastructure.Persistence;
using Xunit;

namespace SmartPrompt.Tests.UnitTests;

public class GetCategoriesQueryHandlerCachingTests
{
    private readonly SmartPromptDbContext _context;
    private readonly Mock<ICacheService> _cacheServiceMock;

    public GetCategoriesQueryHandlerCachingTests()
    {
        var options = new DbContextOptionsBuilder<SmartPromptDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SmartPromptDbContext(options);
        _cacheServiceMock = new Mock<ICacheService>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenCacheMisses_QueriesDatabaseAndSetsCache()
    {
        // Arrange
        _context.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "DB Category" });
        await _context.SaveChangesAsync();

        _cacheServiceMock
            .Setup(x => x.GetAsync<List<CategoryDto>>("Categories_All", It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<CategoryDto>?)null);

        var handler = new GetCategoriesQueryHandler(_context, _cacheServiceMock.Object);
        var query = new GetCategoriesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("DB Category", result[0].Name);

        _cacheServiceMock.Verify(x => x.GetAsync<List<CategoryDto>>("Categories_All", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync("Categories_All", It.Is<List<CategoryDto>>(r => r.Count == 1 && r[0].Name == "DB Category"), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenCacheHits_ReturnsCachedDataWithoutQueryingDatabase()
    {
        // Arrange
        // Add data to DB to prove it's NOT queried
        _context.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "DB Category" });
        await _context.SaveChangesAsync();

        var cachedCategories = new List<CategoryDto>
        {
            new CategoryDto { Id = Guid.NewGuid(), Name = "Cached Category" }
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<List<CategoryDto>>("Categories_All", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedCategories);

        var handler = new GetCategoriesQueryHandler(_context, _cacheServiceMock.Object);
        var query = new GetCategoriesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Cached Category", result[0].Name);

        _cacheServiceMock.Verify(x => x.GetAsync<List<CategoryDto>>("Categories_All", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<List<CategoryDto>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
