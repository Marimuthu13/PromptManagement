using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPrompt.Application.Features.Prompts.Commands.CreatePrompt;
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
    public async Task<ActionResult<List<PromptDto>>> GetPrompts(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPromptsQuery(), cancellationToken);
        return Ok(result);
    }
}
