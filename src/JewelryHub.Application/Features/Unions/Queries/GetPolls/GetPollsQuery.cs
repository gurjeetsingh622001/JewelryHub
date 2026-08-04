using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Common;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Queries.GetPolls;

public record GetPollsQuery(Guid UnionId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<PollSummaryDto>>;

public class GetPollsQueryHandler : IRequestHandler<GetPollsQuery, PagedResult<PollSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetPollsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PollSummaryDto>> Handle(GetPollsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("Admin"))
        {
            await UnionAuthorization.GetActiveMembershipAsync(_unitOfWork, _currentUser, request.UnionId, cancellationToken);
        }

        var query = _unitOfWork.UnionPolls.Query()
            .Where(p => p.UnionId == request.UnionId)
            .OrderByDescending(p => p.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(PollMapper.ToSummaryDto).ToList();
        return new PagedResult<PollSummaryDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
