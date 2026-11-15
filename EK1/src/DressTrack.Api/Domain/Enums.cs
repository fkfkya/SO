namespace DressTrack.Api.Domain;

public enum Role
{
    Seller,
    Buyer,
    Operator
}

public enum ParcelStatus
{
    Created,
    Shipped,
    AwaitingPickup,
    Issued,
    Returning,
    ReturnedToSeller,
    Cancelled
}
