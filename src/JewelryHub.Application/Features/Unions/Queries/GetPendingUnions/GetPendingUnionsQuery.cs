using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Application.Features.Unions.Queries.GetUnions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetPendingUnions;

/// <summary>Admin queue of newly-created unions awaiting approval before they appear in the public directory.</summary>
public record GetPendingUnionsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<UnionDto>>;

public class GetPendingUnionsQueryHandler : IRequestHandler<GetPendingUnionsQuery, PagedResult<UnionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingUnionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<UnionDto>> Handle(GetPendingUnionsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Unions.Query().Include(u => u.CreatedBySeller)
            .Where(u => !u.IsApprovedByAdmin)
            .OrderBy(u => u.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = await GetUnionsQueryHandler.MapWithMemberCountsAsync(_unitOfWork, paged.Items, cancellationToken);
        return new PagedResult<UnionDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
