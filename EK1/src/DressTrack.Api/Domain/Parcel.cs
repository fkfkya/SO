namespace DressTrack.Api.Domain;

public class Parcel
{
    public static readonly TimeSpan StoragePeriod = TimeSpan.FromDays(7);

    private Parcel()
    {
    }

    public Parcel(string trackingNumber, string orderNumber, int storeId, Guid recipientId,
        int pickupPointId, IEnumerable<ParcelItem> items, Guid createdBy, DateTimeOffset now)
    {
        TrackingNumber = trackingNumber;
        OrderNumber = orderNumber;
        StoreId = storeId;
        RecipientId = recipientId;
        PickupPointId = pickupPointId;
        CreatedAt = now;
        Items.AddRange(items);
        RecordChange(null, ParcelStatus.Created, createdBy, now);
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TrackingNumber { get; private set; } = string.Empty;
    public string OrderNumber { get; private set; } = string.Empty;
    public int StoreId { get; private set; }
    public Guid RecipientId { get; private set; }
    public int PickupPointId { get; private set; }
    public ParcelStatus Status { get; private set; } = ParcelStatus.Created;
    public string? StorageCell { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ArrivedAt { get; private set; }
    public DateTimeOffset? StorageDeadline { get; private set; }
    public List<ParcelItem> Items { get; private set; } = [];
    public List<ParcelStatusChange> History { get; private set; } = [];

    public void Ship(Guid sellerId, DateTimeOffset now)
    {
        RequireStatus(ParcelStatus.Created);
        RecordChange(Status, ParcelStatus.Shipped, sellerId, now);
    }

    public void Arrive(string storageCell, Guid operatorId, DateTimeOffset now)
    {
        RequireStatus(ParcelStatus.Shipped);
        StorageCell = storageCell;
        ArrivedAt = now;
        StorageDeadline = now + StoragePeriod;
        RecordChange(Status, ParcelStatus.AwaitingPickup, operatorId, now);
    }

    private void RequireStatus(ParcelStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Transition requires status {expected}, current status is {Status}");
        }
    }

    private void RecordChange(ParcelStatus? from, ParcelStatus to, Guid? changedBy,
        DateTimeOffset now)
    {
        Status = to;
        History.Add(new ParcelStatusChange
        {
            FromStatus = from,
            ToStatus = to,
            ChangedBy = changedBy,
            ChangedAt = now
        });
    }
}

public class ParcelItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Size { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class ParcelStatusChange
{
    public long Id { get; set; }
    public ParcelStatus? FromStatus { get; set; }
    public ParcelStatus ToStatus { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? Reason { get; set; }
}
