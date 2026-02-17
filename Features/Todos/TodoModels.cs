namespace CodeDemo.Features.Todos;

public record Todo(int Id, string Title, bool IsCompleted, string Priority, DateTime? DueDate);
public record CreateTodoRequest(string Title, string? Priority, DateTime? DueDate);
public record UpdateTodoRequest(string? Title, bool? IsCompleted, string? Priority, DateTime? DueDate);
