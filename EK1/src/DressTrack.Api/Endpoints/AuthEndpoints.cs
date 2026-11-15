using DressTrack.Api.Auth;
using DressTrack.Api.Data;
using DressTrack.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DressTrack.Api.Endpoints;

public record LoginRequest(string Login, string Password);

public record LoginResponse(string Token);

public static class AuthEndpoints
{
    private const string InvalidCredentialsMessage = "Неверный логин или пароль";
    private static readonly PasswordHasher<User> Hasher = new();
    private static readonly User TimingDummyUser = new() { Login = "-", DisplayName = "-" };
    private static readonly string TimingDummyHash = Hasher.HashPassword(TimingDummyUser, "-");

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", LoginAsync).AllowAnonymous();
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, AppDbContext db,
        TokenIssuer tokens)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Login == request.Login);
        if (user is null)
        {
            Hasher.VerifyHashedPassword(TimingDummyUser, TimingDummyHash, request.Password);
            return InvalidCredentials();
        }

        var verification = Hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        return verification == PasswordVerificationResult.Failed
            ? InvalidCredentials()
            : Results.Ok(new LoginResponse(tokens.Issue(user)));
    }

    private static IResult InvalidCredentials() =>
        Results.Problem(InvalidCredentialsMessage, statusCode: StatusCodes.Status401Unauthorized);
}
