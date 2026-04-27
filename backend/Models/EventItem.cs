namespace Backend.Models;

public class EventItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateModified { get; set; }
    public int UserId { get; set; }
}
