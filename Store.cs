using CodeDemo.Models;

namespace CodeDemo;

public class Store
{
    public string[] WeatherSummaries { get; } =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public List<User> Users { get; } =
    [
        new(1, "Alice", "alice@example.com", "Admin"),
        new(2, "Bob", "bob@example.com", "User"),
        new(3, "Charlie", "charlie@example.com", "User"),
    ];
    public int NextUserId = 4;

    public List<Product> Products { get; } =
    [
        new(1, "Laptop", 999.99m, 50, "Electronics"),
        new(2, "Headphones", 79.99m, 200, "Electronics"),
        new(3, "Desk Chair", 349.99m, 30, "Furniture"),
        new(4, "Notebook", 4.99m, 1000, "Stationery"),
    ];
    public int NextProductId = 5;

    public List<Order> Orders { get; } =
    [
        new(1, 1, [new OrderItem(1, 1, 999.99m), new OrderItem(2, 2, 79.99m)], DateTime.UtcNow.AddDays(-3), "Shipped"),
        new(2, 2, [new OrderItem(3, 1, 349.99m)], DateTime.UtcNow.AddDays(-1), "Processing"),
    ];
    public int NextOrderId = 3;

    public List<Todo> Todos { get; } =
    [
        new(1, "Buy groceries", false, "Low", DateTime.UtcNow.AddDays(1)),
        new(2, "Finish report", false, "High", DateTime.UtcNow.AddHours(4)),
        new(3, "Call dentist", true, "Medium", DateTime.UtcNow.AddDays(-1)),
    ];
    public int NextTodoId = 4;
}
