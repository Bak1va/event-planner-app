namespace Backend.Models;

public record User(
    int Id,
    string Name,
    string Email,
    DateTime DateAdded,
    DateTime DateModified
);

