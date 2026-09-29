using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartPrompt.Application.Features.Vision.Commands.AnalyzeImage;

namespace SmartPrompt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisionController : ControllerBase
{
    private readonly IMediator _mediator;

    public VisionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeImage([FromBody] AnalyzeImageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ImageData))
        {
            return BadRequest("ImageData is required.");
        }

        var command = new AnalyzeImageCommand
        {
            ImageData = request.ImageData
        };

        var result = await _mediator.Send(command);
        
        return Ok(new { result });
    }
}

public class AnalyzeImageRequest
{
    public string ImageData { get; set; } = string.Empty;
}
