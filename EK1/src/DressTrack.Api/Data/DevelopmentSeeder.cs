using DressTrack.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DressTrack.Api.Data;

public static class DevelopmentSeeder
{
    public static async Task SeedAsync(AppDbContext db, string password, TimeProvider clock)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Seed:Password must be configured");
        }
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var stores = new[] { new Store { Name = "Магазин 1" }, new Store { Name = "Магазин 2" } };
        var points = new[]
        {
            new PickupPoint { Address = "Пункт 1, ул. Учебная, 1" },
            new PickupPoint { Address = "Пункт 2, ул. Учебная, 2" }
        };
        db.AddRange(stores);
        db.AddRange(points);
        await db.SaveChangesAsync();

        var users = CreateUsers(stores, points, password);
        db.Users.AddRange(users.Values);
        db.Parcels.AddRange(CreateParcels(users, stores, points, clock.GetUtcNow()));
        await db.SaveChangesAsync();
    }

    private static Dictionary<string, User> CreateUsers(Store[] stores, PickupPoint[] points,
        string password)
    {
        var hasher = new PasswordHasher<User>();
        var users = new[]
        {
            new User { Login = "seller1", DisplayName = "Продавец 1", Role = Role.Seller, StoreId = stores[0].Id },
            new User { Login = "seller2", DisplayName = "Продавец 2", Role = Role.Seller, StoreId = stores[1].Id },
            new User { Login = "buyer1", DisplayName = "Покупатель 1", Phone = "+70000000001", Role = Role.Buyer },
            new User { Login = "buyer2", DisplayName = "Покупатель 2", Phone = "+70000000002", Role = Role.Buyer },
            new User { Login = "operator1", DisplayName = "Оператор 1", Role = Role.Operator, PickupPointId = points[0].Id },
            new User { Login = "operator2", DisplayName = "Оператор 2", Role = Role.Operator, PickupPointId = points[1].Id }
        };
        foreach (var user in users)
        {
            user.PasswordHash = hasher.HashPassword(user, password);
        }
        return users.ToDictionary(u => u.Login);
    }

    private static IEnumerable<Parcel> CreateParcels(Dictionary<string, User> users,
        Store[] stores, PickupPoint[] points, DateTimeOffset now)
    {
        var awaitingPickup = new Parcel("DT-000001", "ORD-1001", stores[0].Id,
            users["buyer1"].Id, points[0].Id, CreateItems("Куртка", "M", 7990m),
            users["seller1"].Id, now);
        awaitingPickup.Ship(users["seller1"].Id, now);
        awaitingPickup.Arrive("A-01", users["operator1"].Id, now);

        var shipped = new Parcel("DT-000002", "ORD-2001", stores[1].Id,
            users["buyer2"].Id, points[1].Id, CreateItems("Платье", "S", 4590m),
            users["seller2"].Id, now);
        shipped.Ship(users["seller2"].Id, now);

        var created = new Parcel("DT-000003", "ORD-1002", stores[0].Id,
            users["buyer2"].Id, points[1].Id, CreateItems("Джинсы", "L", 3290m),
            users["seller1"].Id, now);

        return [awaitingPickup, shipped, created];
    }

    private static ParcelItem[] CreateItems(string name, string size, decimal price) =>
        [new ParcelItem { Name = name, Size = size, Quantity = 1, Price = price }];
}
