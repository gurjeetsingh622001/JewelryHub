using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetDocuments;

/// <summary>Members-only document library for a union (bylaws, financial reports, meeting records).</summary>
public record GetDocumentsQuery(Guid UnionId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<UnionDocumentDto>>;

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, PagedResult<UnionDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetDocumentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<UnionDocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("Admin"))
        {
            await UnionAuthorization.GetActiveMembershipAsync(_unitOfWork, _currentUser, request.UnionId, cancellationToken);
        }

        var query = _unitOfWork.UnionDocuments.Query()
            .Include(d => d.UploadedByMember).ThenInclude(m => m.Seller)
            .Where(d => d.UnionId == request.UnionId)
            .OrderByDescending(d => d.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(UnionMapper.ToDto).ToList();
        return new PagedResult<UnionDocumentDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
