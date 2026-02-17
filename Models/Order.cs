namespace CodeDemo.Models;

public record Order(int Id, int UserId, List<OrderItem> Items, DateTime CreatedAt, string Status);
public record OrderItem(int ProductId, int Quantity, decimal Total);
public record CreateOrderRequest(int UserId, List<CreateOrderItemRequest> Items);
public record CreateOrderItemRequest(int ProductId, int Quantity);
public record UpdateOrderStatusRequest(string Status);
