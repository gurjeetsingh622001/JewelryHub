using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetPollById;

public record GetPollByIdQuery(Guid PollId) : IRequest<PollDto>;

public class GetPollByIdQueryHandler : IRequestHandler<GetPollByIdQuery, PollDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetPollByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PollDto> Handle(GetPollByIdQuery request, CancellationToken cancellationToken)
    {
        var poll = await _unitOfWork.UnionPolls.Query().WithFullDetails()
            .FirstOrDefaultAsync(p => p.Id == request.PollId, cancellationToken)
            ?? throw new NotFoundException(nameof(UnionPoll), request.PollId);

        if (!_currentUser.IsInRole("Admin"))
        {
            await UnionAuthorization.GetActiveMembershipAsync(_unitOfWork, _currentUser, poll.UnionId, cancellationToken);
        }

        return PollMapper.ToDto(poll);
    }
}
