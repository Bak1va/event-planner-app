using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Entities;

namespace Backend.Entities;

[Table("user_events")]
public class UserEvent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("event_id")]
    public int EventId { get; set; }

    [Column("date_joined")]
    public DateTime DateJoined { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(EventId))]
    public EventItem Event { get; set; } = null!;
}
