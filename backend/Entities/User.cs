using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Entities;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("date_added")]
    public DateTime DateAdded { get; set; }

    [Column("date_modified")]
    public DateTime DateModified { get; set; }

    // Events created/organized by this user
    public ICollection<EventItem> OrganizedEvents { get; set; } = [];

    // Events this user is attending
    public ICollection<UserEvent> AttendingEvents { get; set; } = [];
}
