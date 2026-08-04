using FluentValidation;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Unions;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Commands.CreateAnnouncement;

public record CreateAnnouncementCommand(Guid UnionId, string Title, string Body, bool IsPinned) : IRequest<UnionAnnouncementDto>;

public class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}

public class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, UnionAnnouncementDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateAnnouncementCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<UnionAnnouncementDto> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var member = await UnionAuthorization.GetActiveOfficerMembershipAsync(_unitOfWork, _currentUser, request.UnionId, cancellationToken);

        var announcement = new UnionAnnouncement
        {
            UnionId = request.UnionId,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            IsPinned = request.IsPinned,
            PublishedByMemberId = member.Id,
        };
        await _unitOfWork.UnionAnnouncements.AddAsync(announcement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        announcement.PublishedByMember = member;
        return UnionMapper.ToDto(announcement);
    }
}
