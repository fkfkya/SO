namespace DressTrack.Api.Domain;

public class Store
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class PickupPoint
{
    public int Id { get; set; }
    public required string Address { get; set; }
}
