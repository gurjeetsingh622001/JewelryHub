using FluentValidation;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Unions;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Commands.CreateEvent;

public record CreateEventCommand(
    Guid UnionId, string Title, string? Description, string? Location, DateTime StartsAtUtc, DateTime? EndsAtUtc)
    : IRequest<UnionEventDto>;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x).Must(x => x.EndsAtUtc is null || x.EndsAtUtc > x.StartsAtUtc)
            .WithMessage("EndsAtUtc must be after StartsAtUtc.");
    }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, UnionEventDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateEventCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<UnionEventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        await UnionAuthorization.EnsureOfficerOrAdminAsync(_unitOfWork, _currentUser, request.UnionId, cancellationToken);

        var unionEvent = new UnionEvent
        {
            UnionId = request.UnionId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Location = request.Location,
            StartsAtUtc = request.StartsAtUtc,
            EndsAtUtc = request.EndsAtUtc,
        };
        await _unitOfWork.UnionEvents.AddAsync(unionEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UnionMapper.ToDto(unionEvent);
    }
}
