using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetAnnouncements;

public record GetAnnouncementsQuery(Guid UnionId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<UnionAnnouncementDto>>;

public class GetAnnouncementsQueryHandler : IRequestHandler<GetAnnouncementsQuery, PagedResult<UnionAnnouncementDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAnnouncementsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<UnionAnnouncementDto>> Handle(GetAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.UnionAnnouncements.Query()
            .Include(a => a.PublishedByMember).ThenInclude(m => m.Seller)
            .Where(a => a.UnionId == request.UnionId)
            .OrderByDescending(a => a.IsPinned).ThenByDescending(a => a.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(UnionMapper.ToDto).ToList();
        return new PagedResult<UnionAnnouncementDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
