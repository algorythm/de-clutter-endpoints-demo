namespace CodeDemo.Models;

public record Product(int Id, string Name, decimal Price, int Stock, string Category);
public record CreateProductRequest(string Name, decimal Price, int Stock, string? Category);
public record UpdateProductRequest(string? Name, decimal? Price, int? Stock, string? Category);
public record StockUpdateRequest(int Stock);
