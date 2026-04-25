namespace Backend.Models;

public record User(
    int Id,
    string Name,
    string Email,
    string PasswordHash,
    DateTime DateAdded,
    DateTime DateModified
);

