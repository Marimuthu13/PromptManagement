using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPrompt.Application.Common.Models;
using SmartPrompt.Application.Features.Templates.Commands.DuplicateTemplate;
using SmartPrompt.Application.Features.Templates.Queries.GetTemplateById;
using SmartPrompt.Application.Features.Templates.Queries.GetTemplates;

namespace SmartPrompt.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates([FromQuery] GetTemplatesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplateById(Guid id)
    {
        var result = await _mediator.Send(new GetTemplateByIdQuery { Id = id });
        return Ok(result);
    }

    [HttpPost("{id}/duplicate")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DuplicateTemplate(Guid id)
    {
        var result = await _mediator.Send(new DuplicateTemplateCommand { TemplateId = id });
        return Ok(result);
    }
}
