using System.Security.Claims;
using DressTrack.Api.Data;
using DressTrack.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DressTrack.Api.Auth;

public static class CurrentUser
{
    public static async Task<User?> LoadAsync(ClaimsPrincipal principal, AppDbContext db)
    {
        var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(subject, out var userId))
        {
            return null;
        }
        return await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId);
    }
}
