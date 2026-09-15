using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPrompt.Application.Features.Prompts.Commands.CreatePrompt;
using SmartPrompt.Application.Features.Prompts.Commands.DeletePrompt;
using SmartPrompt.Application.Features.Prompts.Commands.UpdatePrompt;
using SmartPrompt.Application.Features.Prompts.Queries.GetPromptById;
using SmartPrompt.Application.Features.Prompts.Queries.GetPrompts;
namespace SmartPrompt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PromptsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePrompt([FromBody] CreatePromptCommand command, CancellationToken cancellationToken)
    {
        var promptId = await sender.Send(command, cancellationToken);
        
        return Created(string.Empty, new { id = promptId });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SmartPrompt.Application.Common.Models.PagedResult<PromptDto>>> GetPrompts(
        [FromQuery] Guid? categoryId, 
        [FromQuery] string? search, 
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPromptsQuery 
        { 
            CategoryId = categoryId, 
            Search = search,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptDto>> GetPromptById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPromptByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptDto>> UpdatePrompt(Guid id, [FromBody] UpdatePromptCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePrompt(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePromptCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/versions")]
    [ProducesResponseType(typeof(List<SmartPrompt.Application.Features.Prompts.Queries.GetPromptVersions.PromptVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersions(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SmartPrompt.Application.Features.Prompts.Queries.GetPromptVersions.GetPromptVersionsQuery { PromptId = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/versions/{versionId}/restore")]
    [ProducesResponseType(typeof(PromptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreVersion(Guid id, Guid versionId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SmartPrompt.Application.Features.Prompts.Commands.RestorePromptVersion.RestorePromptVersionCommand 
        { 
            PromptId = id, 
            VersionId = versionId 
        }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/execute")]
    [ProducesResponseType(typeof(SmartPrompt.Application.Features.Prompts.Queries.GetPromptExecutions.PromptExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExecutePrompt(Guid id, [FromBody] SmartPrompt.Application.Features.Prompts.Commands.ExecutePrompt.ExecutePromptCommand command, CancellationToken cancellationToken)
    {
        if (id != command.PromptId)
        {
            return BadRequest("PromptId mismatch");
        }
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}/executions")]
    [ProducesResponseType(typeof(List<SmartPrompt.Application.Features.Prompts.Queries.GetPromptExecutions.PromptExecutionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutions(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SmartPrompt.Application.Features.Prompts.Queries.GetPromptExecutions.GetPromptExecutionsQuery { PromptId = id }, cancellationToken);
        return Ok(result);
    }
}
