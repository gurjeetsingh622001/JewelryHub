using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Commands.OpenPoll;

public record OpenPollCommand(Guid PollId) : IRequest;

public class OpenPollCommandHandler : IRequestHandler<OpenPollCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public OpenPollCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(OpenPollCommand request, CancellationToken cancellationToken)
    {
        var poll = await _unitOfWork.UnionPolls.GetByIdAsync(request.PollId, cancellationToken)
            ?? throw new NotFoundException(nameof(UnionPoll), request.PollId);

        await UnionAuthorization.EnsureOfficerOrAdminAsync(_unitOfWork, _currentUser, poll.UnionId, cancellationToken);

        if (poll.Status != PollStatus.Draft)
        {
            throw new BusinessRuleException("Only a Draft poll can be opened for voting.");
        }

        poll.Status = PollStatus.Active;
        _unitOfWork.UnionPolls.Update(poll);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
