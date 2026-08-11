// Mirrors JewelryHub.Application.Features.Users.Common.MyProfileDto.
export interface MyProfile {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  photoUrl: string | null;
  roles: string[];
}

export interface UpdateMyProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  photoUrl: string | null;
}
