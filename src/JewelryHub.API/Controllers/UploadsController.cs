using JewelryHub.Application.Features.Uploads.Commands.UploadFile;
using JewelryHub.Application.Features.Uploads.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryHub.API.Controllers;

/// <summary>
/// A generic, purpose-scoped upload endpoint rather than folding file
/// handling into ProductsController/SellersController — both existing
/// commands (CreateProductCommand, SubmitSellerDocumentCommand) already
/// just take a URL string, so the frontend flow is: upload here first to
/// get a URL back, then call the existing create/submit command with it.
/// Only Sellers upload anything today (their own product photos, their own
/// KYC documents); Admin can too for parity with other Seller-gated actions.
/// </summary>
[ApiController]
[Route("api/v1/uploads")]
[Authorize(Roles = "Seller,Admin")]
public class UploadsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UploadsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("product-images")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(UploadedFileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UploadedFileDto>> UploadProductImage(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest("No file was provided.");

        var content = await ReadAllBytesAsync(file, cancellationToken);
        var result = await _mediator.Send(new UploadFileCommand(content, file.FileName, UploadKind.ProductImage), cancellationToken);
        return Ok(result);
    }

    [HttpPost("kyc-documents")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(UploadedFileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UploadedFileDto>> UploadKycDocument(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest("No file was provided.");

        var content = await ReadAllBytesAsync(file, cancellationToken);
        var result = await _mediator.Send(new UploadFileCommand(content, file.FileName, UploadKind.SellerDocument), cancellationToken);
        return Ok(result);
    }

    private static async Task<byte[]> ReadAllBytesAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        return stream.ToArray();
    }
}
