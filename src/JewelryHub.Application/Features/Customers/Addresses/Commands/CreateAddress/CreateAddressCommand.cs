using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Customers.Addresses.Common;
using JewelryHub.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Customers.Addresses.Commands.CreateAddress;

public record CreateAddressCommand(
    string Label, string AddressLine1, string? AddressLine2, string City, string State,
    string PostalCode, string? ContactPhone, bool IsDefault) : IRequest<CustomerAddressDto>;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(250);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
    }
}

public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, CustomerAddressDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CustomerAddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.Query().FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("No customer profile is associated with this account.");

        // The customer's very first address is always the default, regardless
        // of what the caller passed — there's no meaningful "non-default"
        // state when it's the only address on file.
        var hasAnyAddress = await _unitOfWork.CustomerAddresses.AnyAsync(a => a.CustomerId == customer.Id && !a.IsDeleted, cancellationToken);
        var makeDefault = request.IsDefault || !hasAnyAddress;

        if (makeDefault)
        {
            var existingDefaults = await _unitOfWork.CustomerAddresses.QueryTracking()
                .Where(a => a.CustomerId == customer.Id && a.IsDefault && !a.IsDeleted)
                .ToListAsync(cancellationToken);
            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }
        }

        var address = new CustomerAddress
        {
            CustomerId = customer.Id,
            Label = request.Label.Trim(),
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim(),
            PostalCode = request.PostalCode.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            IsDefault = makeDefault,
        };
        await _unitOfWork.CustomerAddresses.AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerAddressMapper.ToDto(address);
    }
}
