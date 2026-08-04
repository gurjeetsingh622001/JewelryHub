// Mirrors JewelryHub.Application.Features.Auth.Common.AuthResponse. ASP.NET
// Core's default System.Text.Json setup serializes records as camelCase, so
// these fields line up with the wire format directly — no mapping layer.
export interface AuthResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}

export interface AuthenticatedUser {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

// Mirrors RegisterCustomerCommand.
export interface RegisterCustomerRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
}

// Mirrors RegisterSellerCommand.
export interface RegisterSellerRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  businessName: string;
  gstNumber: string;
  businessRegistrationNumber: string;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state: string;
  postalCode: string;
}
