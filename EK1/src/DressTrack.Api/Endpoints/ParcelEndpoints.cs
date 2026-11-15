using System.Security.Claims;
using DressTrack.Api.Access;
using DressTrack.Api.Auth;
using DressTrack.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DressTrack.Api.Endpoints;

public static class ParcelEndpoints
{
    public static void MapParcelEndpoints(this IEndpointRouteBuilder app)
    {
        var parcels = app.MapGroup("/parcels");
        parcels.MapGet("/", ListAsync);
        parcels.MapGet("/{id:guid}", GetAsync);
        parcels.MapGet("/{id:guid}/history", GetHistoryAsync);
    }

    private static async Task<IResult> ListAsync(ClaimsPrincipal principal, AppDbContext db)
    {
        var user = await CurrentUser.LoadAsync(principal, db);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var visible = await db.Parcels.AsNoTracking()
            .VisibleTo(user)
            .Include(p => p.Items)
            .OrderBy(p => p.TrackingNumber)
            .ToListAsync();
        return Results.Ok(visible.Select(p => ParcelViews.For(p, user.Role)));
    }

    private static async Task<IResult> GetAsync(Guid id, ClaimsPrincipal principal,
        AppDbContext db)
    {
        var user = await CurrentUser.LoadAsync(principal, db);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var parcel = await db.Parcels.AsNoTracking()
            .VisibleTo(user)
            .Include(p => p.Items)
            .SingleOrDefaultAsync(p => p.Id == id);
        return parcel is null ? Results.NotFound() : Results.Ok(ParcelViews.For(parcel, user.Role));
    }

    private static async Task<IResult> GetHistoryAsync(Guid id, ClaimsPrincipal principal,
        AppDbContext db)
    {
        var user = await CurrentUser.LoadAsync(principal, db);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var parcel = await db.Parcels.AsNoTracking()
            .VisibleTo(user)
            .Include(p => p.History)
            .SingleOrDefaultAsync(p => p.Id == id);
        return parcel is null
            ? Results.NotFound()
            : Results.Ok(parcel.History.OrderBy(c => c.Id).Select(ParcelViews.ForChange));
    }
}
