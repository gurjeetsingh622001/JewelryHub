using JewelryHub.Application.Features.Catalog.Categories.Commands.CreateCategory;
using JewelryHub.Application.Features.Catalog.Categories.Commands.DeleteCategory;
using JewelryHub.Application.Features.Catalog.Categories.Commands.UpdateCategory;
using JewelryHub.Application.Features.Catalog.Categories.Common;
using JewelryHub.Application.Features.Catalog.Categories.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        // includeInactive is silently ignored for non-admins — an
        // anonymous/customer caller only ever needs the public tree.
        var includeInactive = User.IsInRole("Admin");
        var result = await _mediator.Send(new GetCategoriesQuery(includeInactive), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategories), new { }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryRequestBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(id, body.Name, body.Description, body.IconUrl, body.DisplayOrder, body.IsActive), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>Route id is the source of truth for which category to update, so the body omits it (avoids a mismatched-id footgun).</summary>
public record UpdateCategoryRequestBody(string Name, string? Description, string? IconUrl, int DisplayOrder, bool IsActive);
