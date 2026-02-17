using CodeDemo.Models;

namespace CodeDemo.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products");

        group.MapGet("/", (string? category, Store store) =>
        {
            var result = string.IsNullOrWhiteSpace(category)
                ? store.Products
                : store.Products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", (int id, Store store) =>
        {
            var product = store.Products.FirstOrDefault(p => p.Id == id);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        group.MapPost("/", (CreateProductRequest request, Store store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest("Product name is required.");
            if (request.Price <= 0)
                return Results.BadRequest("Price must be greater than zero.");

            var product = new Product(store.NextProductId++, request.Name, request.Price, request.Stock, request.Category ?? "General");
            store.Products.Add(product);
            return Results.Created($"/api/products/{product.Id}", product);
        });

        group.MapPut("/{id:int}", (int id, UpdateProductRequest request, Store store) =>
        {
            var index = store.Products.FindIndex(p => p.Id == id);
            if (index == -1)
                return Results.NotFound();

            var existing = store.Products[index];
            store.Products[index] = existing with
            {
                Name = request.Name ?? existing.Name,
                Price = request.Price ?? existing.Price,
                Stock = request.Stock ?? existing.Stock,
                Category = request.Category ?? existing.Category,
            };
            return Results.Ok(store.Products[index]);
        });

        group.MapDelete("/{id:int}", (int id, Store store) =>
        {
            var product = store.Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return Results.NotFound();

            store.Products.Remove(product);
            return Results.NoContent();
        });

        group.MapGet("/search", (string q, Store store) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest("Search query is required.");

            var results = store.Products
                .Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Results.Ok(results);
        });

        group.MapPatch("/{id:int}/stock", (int id, StockUpdateRequest request, Store store) =>
        {
            var product = store.Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return Results.NotFound();

            var index = store.Products.IndexOf(product);
            store.Products[index] = product with { Stock = request.Stock };
            return Results.Ok(store.Products[index]);
        });
    }
}
