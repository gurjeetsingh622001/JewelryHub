using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Customers.Addresses.Commands.DeleteAddress;

public record DeleteAddressCommand(Guid AddressId) : IRequest;

public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _unitOfWork.CustomerAddresses.QueryTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.Customer.UserId == _currentUser.UserId && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(CustomerAddress), request.AddressId);

        // Soft-deleted (AuditableEntitySaveChangesInterceptor converts this
        // Remove into IsDeleted = true) — any past Order referencing this
        // address can still resolve it; see CustomerAddress's remarks.
        _unitOfWork.CustomerAddresses.Remove(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
