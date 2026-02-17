using CodeDemo.Models;

namespace CodeDemo.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users");

        group.MapGet("/", (Store store) => Results.Ok(store.Users));

        group.MapGet("/{id:int}", (int id, Store store) =>
        {
            var user = store.Users.FirstOrDefault(u => u.Id == id);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        group.MapPost("/", (CreateUserRequest request, Store store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest("Name is required.");
            if (string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest("Email is required.");
            if (store.Users.Any(u => u.Email == request.Email))
                return Results.Conflict("A user with that email already exists.");

            var user = new User(store.NextUserId++, request.Name, request.Email, request.Role ?? "User");
            store.Users.Add(user);
            return Results.Created($"/api/users/{user.Id}", user);
        });

        group.MapPut("/{id:int}", (int id, UpdateUserRequest request, Store store) =>
        {
            var index = store.Users.FindIndex(u => u.Id == id);
            if (index == -1)
                return Results.NotFound();

            var existing = store.Users[index];
            store.Users[index] = existing with
            {
                Name = request.Name ?? existing.Name,
                Email = request.Email ?? existing.Email,
                Role = request.Role ?? existing.Role,
            };
            return Results.Ok(store.Users[index]);
        });

        group.MapDelete("/{id:int}", (int id, Store store) =>
        {
            var user = store.Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                return Results.NotFound();

            store.Users.Remove(user);
            return Results.NoContent();
        });

        group.MapGet("/{id:int}/orders", (int id, Store store) =>
        {
            if (store.Users.All(u => u.Id != id))
                return Results.NotFound("User not found.");

            var userOrders = store.Orders.Where(o => o.UserId == id).ToList();
            return Results.Ok(userOrders);
        });
    }
}
