namespace Backend.Models;

public record EventItem(
    int Id,
    string Name,
    string Status,
    string Description,
    string ImageUrl,
    DateTime DateAdded,
    DateTime DateModified,
    int UserId
);

