using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.CreatePoll;

/// <summary>Created as Draft — an officer must explicitly OpenPollCommand it before members can vote, so a poll can be reviewed/edited-by-recreation before it goes live.</summary>
public record CreatePollCommand(Guid UnionId, string Question, bool AllowMultipleSelections, DateTime? ClosesAtUtc, List<string> Options)
    : IRequest<PollDto>;

public class CreatePollCommandValidator : AbstractValidator<CreatePollCommand>
{
    public CreatePollCommandValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Options).Must(o => o.Count >= 2).WithMessage("A poll needs at least two options.");
        RuleForEach(x => x.Options).NotEmpty().MaximumLength(250);
    }
}

public class CreatePollCommandHandler : IRequestHandler<CreatePollCommand, PollDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreatePollCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PollDto> Handle(CreatePollCommand request, CancellationToken cancellationToken)
    {
        var creator = await UnionAuthorization.GetActiveOfficerMembershipAsync(_unitOfWork, _currentUser, request.UnionId, cancellationToken);

        var poll = new UnionPoll
        {
            UnionId = request.UnionId,
            Question = request.Question.Trim(),
            AllowMultipleSelections = request.AllowMultipleSelections,
            ClosesAtUtc = request.ClosesAtUtc,
            CreatedByMemberId = creator.Id,
        };
        await _unitOfWork.UnionPolls.AddAsync(poll, cancellationToken);

        for (var i = 0; i < request.Options.Count; i++)
        {
            await _unitOfWork.PollOptions.AddAsync(new PollOption
            {
                PollId = poll.Id,
                Text = request.Options[i].Trim(),
                DisplayOrder = i,
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var withDetails = await _unitOfWork.UnionPolls.Query().WithFullDetails()
            .FirstOrDefaultAsync(p => p.Id == poll.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(UnionPoll), poll.Id);
        return PollMapper.ToDto(withDetails);
    }
}
