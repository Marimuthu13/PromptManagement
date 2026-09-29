using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Application.Common.Interfaces;

namespace SmartPrompt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public AnalyticsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("tokens")]
    public async Task<IActionResult> GetTokenAnalytics(CancellationToken cancellationToken)
    {
        var totalSystemTokens = await _context.PromptExecutionLogs
            .SumAsync(x => x.TotalTokens, cancellationToken);

        var recentExecutions = await _context.PromptExecutionLogs
            .Include(x => x.Prompt)
            .OrderByDescending(x => x.ExecutedAt)
            .Take(10)
            .Select(x => new
            {
                FeatureType = x.FeatureType,
                DisplayTitle = x.Prompt != null ? x.Prompt.Title : (x.DisplayTitle ?? x.FeatureType ?? "AI Operation"),
                ModelUsed = x.ModelUsed ?? "Unknown",
                TotalTokens = x.TotalTokens,
                LatencyMs = x.LatencyMs,
                Timestamp = x.ExecutedAt,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);

        var topPrompts = await _context.PromptExecutionLogs
            .Include(x => x.Prompt)
            .GroupBy(x => new { x.PromptId, Title = x.Prompt != null ? x.Prompt.Title : (x.DisplayTitle ?? "Utility AI Call") })
            .Select(g => new
            {
                PromptId = g.Key.PromptId,
                Title = g.Key.Title ?? "Unknown",
                TotalTokens = g.Sum(x => x.TotalTokens)
            })
            .OrderByDescending(x => x.TotalTokens)
            .Take(5)
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            TotalSystemTokens = totalSystemTokens,
            RecentExecutions = recentExecutions,
            TopPromptsByUsage = topPrompts
        });
    }
}
