using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartPrompt.Application.Features.Categories.Commands.CreateCategory;
using SmartPrompt.Application.Features.Categories.Queries.GetCategories;

namespace SmartPrompt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoryId = await sender.Send(command, cancellationToken);
        
        // As requested, returning 201 Created with the ID without introducing a GetById query just for CreatedAtAction
        return Created(string.Empty, new { id = categoryId });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }
}
