using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Entities;

[Table("events")]
public class EventItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [Column("event_date")]
    public DateTime EventDate { get; set; }

    [Column("date_added")]
    public DateTime DateAdded { get; set; }

    [Column("date_modified")]
    public DateTime DateModified { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    // Users attending this event
    public ICollection<UserEvent> Attendees { get; set; } = [];
}
