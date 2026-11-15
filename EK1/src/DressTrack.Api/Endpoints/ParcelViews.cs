using DressTrack.Api.Domain;

namespace DressTrack.Api.Endpoints;

public record ItemView(string Name, string Size, int Quantity, decimal Price);

public record OperatorItemView(string Name, string Size, int Quantity);

public record ParcelView(Guid Id, string TrackingNumber, string OrderNumber, int StoreId,
    Guid RecipientId, int PickupPointId, ParcelStatus Status, string? StorageCell,
    DateTimeOffset? StorageDeadline, IReadOnlyList<ItemView> Items);

public record OperatorParcelView(Guid Id, string TrackingNumber, Guid RecipientId,
    int PickupPointId, ParcelStatus Status, string? StorageCell,
    DateTimeOffset? StorageDeadline, IReadOnlyList<OperatorItemView> Items);

public record StatusChangeView(ParcelStatus? From, ParcelStatus To, Guid? ChangedBy,
    DateTimeOffset ChangedAt, string? Reason);

public static class ParcelViews
{
    public static object For(Parcel parcel, Role viewerRole) =>
        viewerRole == Role.Operator ? ForOperator(parcel) : ForOwner(parcel);

    private static ParcelView ForOwner(Parcel p) =>
        new(p.Id, p.TrackingNumber, p.OrderNumber, p.StoreId, p.RecipientId, p.PickupPointId,
            p.Status, p.StorageCell, p.StorageDeadline,
            p.Items.Select(i => new ItemView(i.Name, i.Size, i.Quantity, i.Price)).ToList());

    private static OperatorParcelView ForOperator(Parcel p) =>
        new(p.Id, p.TrackingNumber, p.RecipientId, p.PickupPointId, p.Status, p.StorageCell,
            p.StorageDeadline,
            p.Items.Select(i => new OperatorItemView(i.Name, i.Size, i.Quantity)).ToList());

    public static StatusChangeView ForChange(ParcelStatusChange c) =>
        new(c.FromStatus, c.ToStatus, c.ChangedBy, c.ChangedAt, c.Reason);
}
