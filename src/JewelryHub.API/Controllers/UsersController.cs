using JewelryHub.Application.Features.Users.Commands.UpdateMyProfile;
using JewelryHub.Application.Features.Users.Common;
using JewelryHub.Application.Features.Users.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

/// <summary>
/// Any authenticated user's own account profile — distinct from the
/// per-role "me" endpoints (SellersController's GET /sellers/me returns
/// seller-specific business fields) since this is the generic name/phone/
/// photo view every role needs regardless of what other profile they hold.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("me")]
    [ProducesResponseType(typeof(MyProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyProfileDto>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyProfileQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(MyProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MyProfileDto>> UpdateMe(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
