using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Catalog.Categories.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Catalog.Categories.Queries.GetCategories;

/// <summary>Public endpoint; IncludeInactive is only honored for callers in the Admin role (enforced by the controller's authorization, not here).</summary>
public record GetCategoriesQuery(bool IncludeInactive = false) : IRequest<IReadOnlyList<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Categories.Query();
        if (!request.IncludeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.Description, c.IconUrl, c.ParentCategoryId, c.DisplayOrder, c.IsActive))
            .ToListAsync(cancellationToken);
    }
}
