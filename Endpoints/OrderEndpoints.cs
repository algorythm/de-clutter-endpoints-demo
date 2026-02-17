using CodeDemo.Models;

namespace CodeDemo.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapGet("/", (Store store) => Results.Ok(store.Orders));

        group.MapGet("/{id:int}", (int id, Store store) =>
        {
            var order = store.Orders.FirstOrDefault(o => o.Id == id);
            return order is null ? Results.NotFound() : Results.Ok(order);
        });

        group.MapPost("/", (CreateOrderRequest request, Store store) =>
        {
            if (store.Users.All(u => u.Id != request.UserId))
                return Results.BadRequest("User not found.");
            if (request.Items.Count == 0)
                return Results.BadRequest("Order must contain at least one item.");

            foreach (var item in request.Items)
            {
                var product = store.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is null)
                    return Results.BadRequest($"Product {item.ProductId} not found.");
                if (product.Stock < item.Quantity)
                    return Results.BadRequest($"Insufficient stock for product {product.Name}.");
            }

            var orderItems = request.Items.Select(i =>
            {
                var product = store.Products.First(p => p.Id == i.ProductId);
                return new OrderItem(i.ProductId, i.Quantity, product.Price * i.Quantity);
            }).ToList();

            var order = new Order(store.NextOrderId++, request.UserId, orderItems, DateTime.UtcNow, "Pending");
            store.Orders.Add(order);

            foreach (var item in request.Items)
            {
                var pIndex = store.Products.FindIndex(p => p.Id == item.ProductId);
                store.Products[pIndex] = store.Products[pIndex] with { Stock = store.Products[pIndex].Stock - item.Quantity };
            }

            return Results.Created($"/api/orders/{order.Id}", order);
        });

        group.MapPatch("/{id:int}/status", (int id, UpdateOrderStatusRequest request, Store store) =>
        {
            var order = store.Orders.FirstOrDefault(o => o.Id == id);
            if (order is null)
                return Results.NotFound();

            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
                return Results.BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");

            var index = store.Orders.IndexOf(order);
            store.Orders[index] = order with { Status = request.Status };
            return Results.Ok(store.Orders[index]);
        });

        group.MapDelete("/{id:int}", (int id, Store store) =>
        {
            var order = store.Orders.FirstOrDefault(o => o.Id == id);
            if (order is null)
                return Results.NotFound();
            if (order.Status == "Shipped" || order.Status == "Delivered")
                return Results.BadRequest("Cannot cancel a shipped or delivered order.");

            store.Orders.Remove(order);
            return Results.NoContent();
        });

        group.MapGet("/stats", (Store store) =>
        {
            var stats = new
            {
                TotalOrders = store.Orders.Count,
                TotalRevenue = store.Orders.SelectMany(o => o.Items).Sum(i => i.Total),
                ByStatus = store.Orders.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.Count()),
            };
            return Results.Ok(stats);
        });
    }
}
