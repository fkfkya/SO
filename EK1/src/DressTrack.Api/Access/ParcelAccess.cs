using DressTrack.Api.Domain;

namespace DressTrack.Api.Access;

public static class ParcelAccess
{
    public static IQueryable<Parcel> VisibleTo(this IQueryable<Parcel> parcels, User user) =>
        user.Role switch
        {
            Role.Buyer => parcels.Where(p => p.RecipientId == user.Id),
            Role.Seller when user.StoreId is int storeId =>
                parcels.Where(p => p.StoreId == storeId),
            Role.Operator when user.PickupPointId is int pointId =>
                parcels.Where(p => p.PickupPointId == pointId),
            _ => parcels.Where(_ => false)
        };
}
