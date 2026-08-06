// Mirrors JewelryHub.Application.Features.Customers.Addresses.Common.CustomerAddressDto.
export interface CustomerAddress {
  id: string;
  label: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  contactPhone: string | null;
  isDefault: boolean;
}

// Mirrors CreateAddressCommand.
export interface CreateAddressRequest {
  label: string;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state: string;
  postalCode: string;
  contactPhone?: string | null;
  isDefault: boolean;
}
