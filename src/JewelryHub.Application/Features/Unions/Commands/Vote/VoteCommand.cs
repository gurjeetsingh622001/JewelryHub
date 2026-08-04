using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.Vote;

/// <summary>Votes are final — no changing your mind after submitting, matching how a real ballot works. One VoteCommand call casts every selection at once.</summary>
public record VoteCommand(Guid PollId, List<Guid> OptionIds) : IRequest;

public class VoteCommandValidator : AbstractValidator<VoteCommand>
{
    public VoteCommandValidator()
    {
        RuleFor(x => x.OptionIds).NotEmpty().WithMessage("At least one option must be selected.");
    }
}

public class VoteCommandHandler : IRequestHandler<VoteCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public VoteCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(VoteCommand request, CancellationToken cancellationToken)
    {
        var poll = await _unitOfWork.UnionPolls.Query().Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == request.PollId, cancellationToken)
            ?? throw new NotFoundException(nameof(UnionPoll), request.PollId);

        var membership = await UnionAuthorization.GetActiveMembershipAsync(_unitOfWork, _currentUser, poll.UnionId, cancellationToken);

        if (poll.Status != PollStatus.Active || (poll.ClosesAtUtc is not null && poll.ClosesAtUtc <= DateTime.UtcNow))
        {
            throw new BusinessRuleException("This poll is not currently open for voting.");
        }

        if (!poll.AllowMultipleSelections && request.OptionIds.Count > 1)
        {
            throw new BusinessRuleException("This poll only allows a single selection.");
        }

        var validOptionIds = poll.Options.Select(o => o.Id).ToHashSet();
        if (!request.OptionIds.All(validOptionIds.Contains))
        {
            throw new BusinessRuleException("One or more selected options do not belong to this poll.");
        }

        var alreadyVoted = await _unitOfWork.PollVotes.Query()
            .AnyAsync(v => v.UnionMemberId == membership.Id && v.PollOption.PollId == request.PollId, cancellationToken);
        if (alreadyVoted)
        {
            throw new BusinessRuleException("You have already voted in this poll.");
        }

        foreach (var optionId in request.OptionIds.Distinct())
        {
            await _unitOfWork.PollVotes.AddAsync(new PollVote { PollOptionId = optionId, UnionMemberId = membership.Id }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
