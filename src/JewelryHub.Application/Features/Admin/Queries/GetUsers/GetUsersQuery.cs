using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Admin.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery(string? Search, string? Role, bool? IsActive, int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<UserSummaryDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<UserSummaryDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Users.Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(u =>
                u.Email.Contains(request.Search) || u.FirstName.Contains(request.Search) || u.LastName.Contains(request.Search));
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == request.Role));
        }

        if (request.IsActive is not null)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        query = query.OrderByDescending(u => u.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(u => new UserSummaryDto(
            u.Id, u.Email, u.FirstName, u.LastName,
            u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            u.IsActive, u.IsLockedOut, u.CreatedAtUtc, u.LastLoginAtUtc)).ToList();

        return new PagedResult<UserSummaryDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
