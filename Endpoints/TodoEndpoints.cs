using CodeDemo.Models;

namespace CodeDemo.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/todos");

        group.MapGet("/", (bool? completed, string? priority, Store store) =>
        {
            var result = store.Todos.AsEnumerable();
            if (completed.HasValue)
                result = result.Where(t => t.IsCompleted == completed.Value);
            if (!string.IsNullOrWhiteSpace(priority))
                result = result.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));
            return Results.Ok(result.ToList());
        });

        group.MapGet("/{id:int}", (int id, Store store) =>
        {
            var todo = store.Todos.FirstOrDefault(t => t.Id == id);
            return todo is null ? Results.NotFound() : Results.Ok(todo);
        });

        group.MapPost("/", (CreateTodoRequest request, Store store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest("Title is required.");

            var validPriorities = new[] { "Low", "Medium", "High" };
            var priority = request.Priority ?? "Medium";
            if (!validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase))
                return Results.BadRequest($"Invalid priority. Valid values: {string.Join(", ", validPriorities)}");

            var todo = new Todo(store.NextTodoId++, request.Title, false, priority, request.DueDate);
            store.Todos.Add(todo);
            return Results.Created($"/api/todos/{todo.Id}", todo);
        });

        group.MapPut("/{id:int}", (int id, UpdateTodoRequest request, Store store) =>
        {
            var index = store.Todos.FindIndex(t => t.Id == id);
            if (index == -1)
                return Results.NotFound();

            var existing = store.Todos[index];
            store.Todos[index] = existing with
            {
                Title = request.Title ?? existing.Title,
                IsCompleted = request.IsCompleted ?? existing.IsCompleted,
                Priority = request.Priority ?? existing.Priority,
                DueDate = request.DueDate ?? existing.DueDate,
            };
            return Results.Ok(store.Todos[index]);
        });

        group.MapDelete("/{id:int}", (int id, Store store) =>
        {
            var todo = store.Todos.FirstOrDefault(t => t.Id == id);
            if (todo is null)
                return Results.NotFound();

            store.Todos.Remove(todo);
            return Results.NoContent();
        });

        group.MapPatch("/{id:int}/complete", (int id, Store store) =>
        {
            var todo = store.Todos.FirstOrDefault(t => t.Id == id);
            if (todo is null)
                return Results.NotFound();

            var index = store.Todos.IndexOf(todo);
            store.Todos[index] = todo with { IsCompleted = true };
            return Results.Ok(store.Todos[index]);
        });

        group.MapGet("/overdue", (Store store) =>
        {
            var overdue = store.Todos
                .Where(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow)
                .ToList();
            return Results.Ok(overdue);
        });
    }
}
