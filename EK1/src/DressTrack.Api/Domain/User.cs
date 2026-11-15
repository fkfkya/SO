namespace DressTrack.Api.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Login { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public required string DisplayName { get; set; }
    public string? Phone { get; set; }
    public Role Role { get; set; }
    public int? StoreId { get; set; }
    public int? PickupPointId { get; set; }
}
