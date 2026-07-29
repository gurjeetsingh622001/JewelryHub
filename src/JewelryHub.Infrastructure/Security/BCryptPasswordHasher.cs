using JewelryHub.Application.Common.Interfaces;

namespace JewelryHub.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    // 12 work factor: deliberately above BCrypt's default (10) — jewelry
    // orders/payment data justify the extra ~4x hashing cost per login.
    private const int WorkFactor = 12;

    public string Hash(string plainTextPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainTextPassword, workFactor: WorkFactor);

    public bool Verify(string plainTextPassword, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
}
