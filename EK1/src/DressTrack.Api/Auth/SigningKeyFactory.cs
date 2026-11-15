using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DressTrack.Api.Auth;

public static class SigningKeyFactory
{
    private const int MinKeyBytes = 32;

    public static SymmetricSecurityKey Create(string? configuredKey, bool isDevelopment)
    {
        if (!string.IsNullOrEmpty(configuredKey))
        {
            var keyBytes = Encoding.UTF8.GetBytes(configuredKey);
            if (keyBytes.Length < MinKeyBytes)
            {
                throw new InvalidOperationException($"Jwt:Key must be at least {MinKeyBytes} bytes");
            }
            return new SymmetricSecurityKey(keyBytes);
        }
        if (!isDevelopment)
        {
            throw new InvalidOperationException("Jwt:Key must be configured outside Development");
        }
        return new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(MinKeyBytes));
    }
}
