using FluentValidation;
using JewelryHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _tokenService;

    public LogoutCommandHandler(IUnitOfWork unitOfWork, IJwtTokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashRefreshToken(request.RefreshToken);

        var token = await _unitOfWork.RefreshTokens.Query()
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash && rt.RevokedAtUtc == null, cancellationToken);

        // Logging out with an already-invalid token is a no-op, not an
        // error — the caller's goal (be logged out) is already satisfied.
        if (token is null) return;

        token.RevokedAtUtc = DateTime.UtcNow;
        _unitOfWork.RefreshTokens.Update(token);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
