using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Auth.Common;
using JewelryHub.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

/// <summary>
/// Implements rotation: every use of a refresh token immediately revokes
/// it and issues a brand new one. If a revoked token is ever presented
/// again, that's a signal of token theft/reuse — every refresh token for
/// the user is revoked as a precaution.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _tokenService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtTokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var incomingHash = _tokenService.HashRefreshToken(request.RefreshToken);

        var existingToken = await _unitOfWork.RefreshTokens.Query()
            .Include(rt => rt.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.TokenHash == incomingHash, cancellationToken);

        if (existingToken is null)
        {
            throw new AuthenticationFailedException("Invalid refresh token.");
        }

        if (existingToken.RevokedAtUtc is not null)
        {
            // Reuse of an already-rotated token: revoke everything for this user.
            var allTokens = await _unitOfWork.RefreshTokens.Query()
                .Where(rt => rt.UserId == existingToken.UserId && rt.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);
            foreach (var t in allTokens)
            {
                t.RevokedAtUtc = DateTime.UtcNow;
                _unitOfWork.RefreshTokens.Update(t);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new AuthenticationFailedException("This refresh token has already been used. All sessions have been signed out for your security.");
        }

        if (existingToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new AuthenticationFailedException("Refresh token has expired. Please log in again.");
        }

        var user = existingToken.User;
        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var tokens = _tokenService.GenerateTokenPair(user.Id, user.Email, roleNames);

        existingToken.RevokedAtUtc = DateTime.UtcNow;
        existingToken.ReplacedByTokenHash = _tokenService.HashRefreshToken(tokens.RefreshToken);
        _unitOfWork.RefreshTokens.Update(existingToken);

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = existingToken.ReplacedByTokenHash,
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc,
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, roleNames,
            tokens.AccessToken, tokens.AccessTokenExpiresAtUtc, tokens.RefreshToken, tokens.RefreshTokenExpiresAtUtc);
    }
}
