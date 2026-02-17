namespace CodeDemo.Models;

public record User(int Id, string Name, string Email, string Role);
public record CreateUserRequest(string Name, string Email, string? Role);
public record UpdateUserRequest(string? Name, string? Email, string? Role);
