namespace JewelryHub.Application.Features.Customers.Addresses.Common;

public record CustomerAddressDto(
    Guid Id,
    string Label,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? ContactPhone,
    bool IsDefault);
