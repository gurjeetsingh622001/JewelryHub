using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Customers.Addresses.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Customers.Addresses.Queries.GetMyAddresses;

public record GetMyAddressesQuery : IRequest<IReadOnlyList<CustomerAddressDto>>;

public class GetMyAddressesQueryHandler : IRequestHandler<GetMyAddressesQuery, IReadOnlyList<CustomerAddressDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyAddressesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CustomerAddressDto>> Handle(GetMyAddressesQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _unitOfWork.CustomerAddresses.Query()
            .Where(a => a.Customer.UserId == _currentUser.UserId && !a.IsDeleted)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return addresses.Select(CustomerAddressMapper.ToDto).ToList();
    }
}
