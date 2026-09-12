using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartPrompt.Application.Features.Users.Commands.CreateUser;

namespace SmartPrompt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var userId = await sender.Send(command, cancellationToken);
        
        // Minimal response adhering to Phase 3.4
        return Created(string.Empty, new { id = userId });
    }
}
