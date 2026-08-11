using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Admin.Common;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Admin.Queries.GetDashboardOverview;

public record GetDashboardOverviewQuery : IRequest<DashboardOverviewDto>;

public class GetDashboardOverviewQueryHandler : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardOverviewQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<DashboardOverviewDto> Handle(GetDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalCustomers = await _unitOfWork.Customers.Query().CountAsync(cancellationToken);

        var totalSellers = await _unitOfWork.Sellers.Query().CountAsync(cancellationToken);
        var pendingSellers = await _unitOfWork.Sellers.Query().CountAsync(
            s => s.VerificationStatus == SellerVerificationStatus.PendingApproval || s.VerificationStatus == SellerVerificationStatus.UnderReview,
            cancellationToken);
        var approvedSellers = await _unitOfWork.Sellers.Query().CountAsync(s => s.VerificationStatus == SellerVerificationStatus.Approved, cancellationToken);

        var totalUnions = await _unitOfWork.Unions.Query().CountAsync(cancellationToken);
        var approvedUnions = await _unitOfWork.Unions.Query().CountAsync(u => u.IsApprovedByAdmin, cancellationToken);
        var pendingUnions = totalUnions - approvedUnions;

        var totalOrders = await _unitOfWork.Orders.Query().CountAsync(cancellationToken);
        var ordersThisMonth = await _unitOfWork.Orders.Query().CountAsync(o => o.CreatedAtUtc >= startOfMonth, cancellationToken);
        var revenueThisMonth = await _unitOfWork.Orders.Query()
            .Where(o => o.CreatedAtUtc >= startOfMonth && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
            .SumAsync(o => (decimal?)o.GrandTotal, cancellationToken) ?? 0m;

        var totalReviews = await _unitOfWork.Reviews.Query().CountAsync(cancellationToken);
        var flaggedReviews = await _unitOfWork.Reviews.Query().CountAsync(r => r.IsFlagged, cancellationToken);

        return new DashboardOverviewDto(
            totalCustomers, totalSellers, pendingSellers, approvedSellers,
            totalUnions, pendingUnions, approvedUnions,
            totalOrders, ordersThisMonth, revenueThisMonth,
            totalReviews, flaggedReviews);
    }
}
