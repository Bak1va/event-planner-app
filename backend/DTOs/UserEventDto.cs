namespace Backend.DTOs;

public class UserEventDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public DateTime DateJoined { get; set; }
}

public class CreateUserEventDto
{
    public int UserId { get; set; }
    public int EventId { get; set; }
}

