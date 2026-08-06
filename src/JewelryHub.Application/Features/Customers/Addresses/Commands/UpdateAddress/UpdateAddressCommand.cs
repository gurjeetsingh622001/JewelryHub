using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Customers.Addresses.Common;
using JewelryHub.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Customers.Addresses.Commands.UpdateAddress;

public record UpdateAddressCommand(
    Guid AddressId, string Label, string AddressLine1, string? AddressLine2, string City, string State,
    string PostalCode, string? ContactPhone, bool IsDefault) : IRequest<CustomerAddressDto>;

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(250);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
    }
}

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, CustomerAddressDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CustomerAddressDto> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _unitOfWork.CustomerAddresses.QueryTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.Customer.UserId == _currentUser.UserId && !a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(CustomerAddress), request.AddressId);

        if (request.IsDefault && !address.IsDefault)
        {
            var otherDefaults = await _unitOfWork.CustomerAddresses.QueryTracking()
                .Where(a => a.CustomerId == address.CustomerId && a.IsDefault && a.Id != address.Id && !a.IsDeleted)
                .ToListAsync(cancellationToken);
            foreach (var other in otherDefaults)
            {
                other.IsDefault = false;
            }
        }

        address.Label = request.Label.Trim();
        address.AddressLine1 = request.AddressLine1.Trim();
        address.AddressLine2 = request.AddressLine2?.Trim();
        address.City = request.City.Trim();
        address.State = request.State.Trim();
        address.PostalCode = request.PostalCode.Trim();
        address.ContactPhone = request.ContactPhone?.Trim();
        address.IsDefault = request.IsDefault;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerAddressMapper.ToDto(address);
    }
}
