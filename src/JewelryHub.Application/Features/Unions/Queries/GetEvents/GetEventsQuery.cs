using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Common;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Queries.GetEvents;

/// <summary>Public — union-organized trade fairs/training sessions are meant to draw attendance beyond the membership.</summary>
public record GetEventsQuery(Guid UnionId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<UnionEventDto>>;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, PagedResult<UnionEventDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEventsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<UnionEventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.UnionEvents.Query()
            .Where(e => e.UnionId == request.UnionId)
            .OrderBy(e => e.StartsAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(UnionMapper.ToDto).ToList();
        return new PagedResult<UnionEventDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
