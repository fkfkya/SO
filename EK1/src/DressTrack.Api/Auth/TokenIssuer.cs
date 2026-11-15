using System.Security.Claims;
using DressTrack.Api.Domain;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DressTrack.Api.Auth;

public class TokenIssuer(SymmetricSecurityKey signingKey, TimeProvider clock)
{
    public const string Issuer = "dresstrack";
    public const string Audience = "dresstrack-api";
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

    public string Issue(User user)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())]),
            NotBefore = now,
            Expires = now + TokenLifetime,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
