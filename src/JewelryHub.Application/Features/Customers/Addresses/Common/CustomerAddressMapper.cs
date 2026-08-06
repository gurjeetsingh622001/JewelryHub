using JewelryHub.Domain.Customers;

namespace JewelryHub.Application.Features.Customers.Addresses.Common;

public static class CustomerAddressMapper
{
    public static CustomerAddressDto ToDto(CustomerAddress a) => new(
        a.Id, a.Label, a.AddressLine1, a.AddressLine2, a.City, a.State, a.PostalCode, a.Country, a.ContactPhone, a.IsDefault);
}
