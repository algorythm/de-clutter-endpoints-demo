var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ============================================================
// In-memory data stores
// ============================================================

var weatherSummaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

var users = new List<User>
{
    new(1, "Alice", "alice@example.com", "Admin"),
    new(2, "Bob", "bob@example.com", "User"),
    new(3, "Charlie", "charlie@example.com", "User"),
};
var nextUserId = 4;

var products = new List<Product>
{
    new(1, "Laptop", 999.99m, 50, "Electronics"),
    new(2, "Headphones", 79.99m, 200, "Electronics"),
    new(3, "Desk Chair", 349.99m, 30, "Furniture"),
    new(4, "Notebook", 4.99m, 1000, "Stationery"),
};
var nextProductId = 5;

var orders = new List<Order>
{
    new(1, 1, [new OrderItem(1, 1, 999.99m), new OrderItem(2, 2, 79.99m)], DateTime.UtcNow.AddDays(-3), "Shipped"),
    new(2, 2, [new OrderItem(3, 1, 349.99m)], DateTime.UtcNow.AddDays(-1), "Processing"),
};
var nextOrderId = 3;

var todos = new List<Todo>
{
    new(1, "Buy groceries", false, "Low", DateTime.UtcNow.AddDays(1)),
    new(2, "Finish report", false, "High", DateTime.UtcNow.AddHours(4)),
    new(3, "Call dentist", true, "Medium", DateTime.UtcNow.AddDays(-1)),
};
var nextTodoId = 4;

// ============================================================
// Weather endpoints
// ============================================================

app.MapGet("/api/weather/forecast", () =>
{
    var forecast = Enumerable.Range(1, 7).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            weatherSummaries[Random.Shared.Next(weatherSummaries.Length)]
        )).ToArray();
    return Results.Ok(forecast);
});

app.MapGet("/api/weather/forecast/{days:int}", (int days) =>
{
    if (days < 1 || days > 30)
        return Results.BadRequest("Days must be between 1 and 30.");

    var forecast = Enumerable.Range(1, days).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            weatherSummaries[Random.Shared.Next(weatherSummaries.Length)]
        )).ToArray();
    return Results.Ok(forecast);
});

app.MapGet("/api/weather/current", () =>
{
    var current = new
    {
        Temperature = Random.Shared.Next(-10, 40),
        Summary = weatherSummaries[Random.Shared.Next(weatherSummaries.Length)],
        Humidity = Random.Shared.Next(20, 100),
        WindSpeed = Random.Shared.Next(0, 50),
        Timestamp = DateTime.UtcNow,
    };
    return Results.Ok(current);
});

// ============================================================
// User endpoints
// ============================================================

app.MapGet("/api/users", () => Results.Ok(users));

app.MapGet("/api/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is null ? Results.NotFound() : Results.Ok(user);
});

app.MapPost("/api/users", (CreateUserRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest("Name is required.");
    if (string.IsNullOrWhiteSpace(request.Email))
        return Results.BadRequest("Email is required.");
    if (users.Any(u => u.Email == request.Email))
        return Results.Conflict("A user with that email already exists.");

    var user = new User(nextUserId++, request.Name, request.Email, request.Role ?? "User");
    users.Add(user);
    return Results.Created($"/api/users/{user.Id}", user);
});

app.MapPut("/api/users/{id:int}", (int id, UpdateUserRequest request) =>
{
    var index = users.FindIndex(u => u.Id == id);
    if (index == -1)
        return Results.NotFound();

    var existing = users[index];
    users[index] = existing with
    {
        Name = request.Name ?? existing.Name,
        Email = request.Email ?? existing.Email,
        Role = request.Role ?? existing.Role,
    };
    return Results.Ok(users[index]);
});

app.MapDelete("/api/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null)
        return Results.NotFound();

    users.Remove(user);
    return Results.NoContent();
});

app.MapGet("/api/users/{id:int}/orders", (int id) =>
{
    if (users.All(u => u.Id != id))
        return Results.NotFound("User not found.");

    var userOrders = orders.Where(o => o.UserId == id).ToList();
    return Results.Ok(userOrders);
});

// ============================================================
// Product endpoints
// ============================================================

app.MapGet("/api/products", (string? category) =>
{
    var result = string.IsNullOrWhiteSpace(category)
        ? products
        : products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    return Results.Ok(result);
});

app.MapGet("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapPost("/api/products", (CreateProductRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest("Product name is required.");
    if (request.Price <= 0)
        return Results.BadRequest("Price must be greater than zero.");

    var product = new Product(nextProductId++, request.Name, request.Price, request.Stock, request.Category ?? "General");
    products.Add(product);
    return Results.Created($"/api/products/{product.Id}", product);
});

app.MapPut("/api/products/{id:int}", (int id, UpdateProductRequest request) =>
{
    var index = products.FindIndex(p => p.Id == id);
    if (index == -1)
        return Results.NotFound();

    var existing = products[index];
    products[index] = existing with
    {
        Name = request.Name ?? existing.Name,
        Price = request.Price ?? existing.Price,
        Stock = request.Stock ?? existing.Stock,
        Category = request.Category ?? existing.Category,
    };
    return Results.Ok(products[index]);
});

app.MapDelete("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
        return Results.NotFound();

    products.Remove(product);
    return Results.NoContent();
});

app.MapGet("/api/products/search", (string q) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest("Search query is required.");

    var results = products
        .Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
        .ToList();
    return Results.Ok(results);
});

app.MapPatch("/api/products/{id:int}/stock", (int id, StockUpdateRequest request) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
        return Results.NotFound();

    var index = products.IndexOf(product);
    products[index] = product with { Stock = request.Stock };
    return Results.Ok(products[index]);
});

// ============================================================
// Order endpoints
// ============================================================

app.MapGet("/api/orders", () => Results.Ok(orders));

app.MapGet("/api/orders/{id:int}", (int id) =>
{
    var order = orders.FirstOrDefault(o => o.Id == id);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapPost("/api/orders", (CreateOrderRequest request) =>
{
    if (users.All(u => u.Id != request.UserId))
        return Results.BadRequest("User not found.");
    if (request.Items.Count == 0)
        return Results.BadRequest("Order must contain at least one item.");

    foreach (var item in request.Items)
    {
        var product = products.FirstOrDefault(p => p.Id == item.ProductId);
        if (product is null)
            return Results.BadRequest($"Product {item.ProductId} not found.");
        if (product.Stock < item.Quantity)
            return Results.BadRequest($"Insufficient stock for product {product.Name}.");
    }

    var orderItems = request.Items.Select(i =>
    {
        var product = products.First(p => p.Id == i.ProductId);
        return new OrderItem(i.ProductId, i.Quantity, product.Price * i.Quantity);
    }).ToList();

    var order = new Order(nextOrderId++, request.UserId, orderItems, DateTime.UtcNow, "Pending");
    orders.Add(order);

    foreach (var item in request.Items)
    {
        var pIndex = products.FindIndex(p => p.Id == item.ProductId);
        products[pIndex] = products[pIndex] with { Stock = products[pIndex].Stock - item.Quantity };
    }

    return Results.Created($"/api/orders/{order.Id}", order);
});

app.MapPatch("/api/orders/{id:int}/status", (int id, UpdateOrderStatusRequest request) =>
{
    var order = orders.FirstOrDefault(o => o.Id == id);
    if (order is null)
        return Results.NotFound();

    var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
    if (!validStatuses.Contains(request.Status))
        return Results.BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");

    var index = orders.IndexOf(order);
    orders[index] = order with { Status = request.Status };
    return Results.Ok(orders[index]);
});

app.MapDelete("/api/orders/{id:int}", (int id) =>
{
    var order = orders.FirstOrDefault(o => o.Id == id);
    if (order is null)
        return Results.NotFound();
    if (order.Status == "Shipped" || order.Status == "Delivered")
        return Results.BadRequest("Cannot cancel a shipped or delivered order.");

    orders.Remove(order);
    return Results.NoContent();
});

app.MapGet("/api/orders/stats", () =>
{
    var stats = new
    {
        TotalOrders = orders.Count,
        TotalRevenue = orders.SelectMany(o => o.Items).Sum(i => i.Total),
        ByStatus = orders.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.Count()),
    };
    return Results.Ok(stats);
});

// ============================================================
// Todo endpoints
// ============================================================

app.MapGet("/api/todos", (bool? completed, string? priority) =>
{
    var result = todos.AsEnumerable();
    if (completed.HasValue)
        result = result.Where(t => t.IsCompleted == completed.Value);
    if (!string.IsNullOrWhiteSpace(priority))
        result = result.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(result.ToList());
});

app.MapGet("/api/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    return todo is null ? Results.NotFound() : Results.Ok(todo);
});

app.MapPost("/api/todos", (CreateTodoRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest("Title is required.");

    var validPriorities = new[] { "Low", "Medium", "High" };
    var priority = request.Priority ?? "Medium";
    if (!validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase))
        return Results.BadRequest($"Invalid priority. Valid values: {string.Join(", ", validPriorities)}");

    var todo = new Todo(nextTodoId++, request.Title, false, priority, request.DueDate);
    todos.Add(todo);
    return Results.Created($"/api/todos/{todo.Id}", todo);
});

app.MapPut("/api/todos/{id:int}", (int id, UpdateTodoRequest request) =>
{
    var index = todos.FindIndex(t => t.Id == id);
    if (index == -1)
        return Results.NotFound();

    var existing = todos[index];
    todos[index] = existing with
    {
        Title = request.Title ?? existing.Title,
        IsCompleted = request.IsCompleted ?? existing.IsCompleted,
        Priority = request.Priority ?? existing.Priority,
        DueDate = request.DueDate ?? existing.DueDate,
    };
    return Results.Ok(todos[index]);
});

app.MapDelete("/api/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null)
        return Results.NotFound();

    todos.Remove(todo);
    return Results.NoContent();
});

app.MapPatch("/api/todos/{id:int}/complete", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null)
        return Results.NotFound();

    var index = todos.IndexOf(todo);
    todos[index] = todo with { IsCompleted = true };
    return Results.Ok(todos[index]);
});

app.MapGet("/api/todos/overdue", () =>
{
    var overdue = todos
        .Where(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow)
        .ToList();
    return Results.Ok(overdue);
});

app.Run();

// ============================================================
// All models crammed at the bottom of this file too...
// ============================================================

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record User(int Id, string Name, string Email, string Role);
record CreateUserRequest(string Name, string Email, string? Role);
record UpdateUserRequest(string? Name, string? Email, string? Role);

record Product(int Id, string Name, decimal Price, int Stock, string Category);
record CreateProductRequest(string Name, decimal Price, int Stock, string? Category);
record UpdateProductRequest(string? Name, decimal? Price, int? Stock, string? Category);
record StockUpdateRequest(int Stock);

record Order(int Id, int UserId, List<OrderItem> Items, DateTime CreatedAt, string Status);
record OrderItem(int ProductId, int Quantity, decimal Total);
record CreateOrderRequest(int UserId, List<CreateOrderItemRequest> Items);
record CreateOrderItemRequest(int ProductId, int Quantity);
record UpdateOrderStatusRequest(string Status);

record Todo(int Id, string Title, bool IsCompleted, string Priority, DateTime? DueDate);
record CreateTodoRequest(string Title, string? Priority, DateTime? DueDate);
record UpdateTodoRequest(string? Title, bool? IsCompleted, string? Priority, DateTime? DueDate);
