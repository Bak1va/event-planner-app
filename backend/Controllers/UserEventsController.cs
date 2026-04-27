using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserEventsController(UserEventService userEventService) : ControllerBase
{
    /// <summary>
    /// Add a user to an event (user attends event)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserEventDto>> AddUserToEvent(CreateUserEventDto dto)
    {
        try
        {
            var result = await userEventService.AddUserToEventAsync(dto);
            return CreatedAtAction(nameof(GetEventAttendees), new { eventId = dto.EventId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all events a user is attending
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserEventDto>>> GetUserEvents(int userId)
    {
        var events = await userEventService.GetUserEventsAsync(userId);
        return Ok(events);
    }

    /// <summary>
    /// Get all users attending an event
    /// </summary>
    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<IEnumerable<UserEventDto>>> GetEventAttendees(int eventId)
    {
        var attendees = await userEventService.GetEventAttendeesAsync(eventId);
        return Ok(attendees);
    }

    /// <summary>
    /// Remove a user from an event
    /// </summary>
    [HttpDelete("user/{userId}/event/{eventId}")]
    public async Task<IActionResult> RemoveUserFromEvent(int userId, int eventId)
    {
        var removed = await userEventService.RemoveUserFromEventAsync(userId, eventId);
        if (!removed)
            return NotFound(new { message = $"User {userId} is not attending event {eventId}." });

        return NoContent();
    }

    /// <summary>
    /// Check if a user is attending an event
    /// </summary>
    [HttpGet("user/{userId}/event/{eventId}")]
    public async Task<ActionResult<bool>> IsUserAttendingEvent(int userId, int eventId)
    {
        var isAttending = await userEventService.IsUserAttendingEventAsync(userId, eventId);
        return Ok(new { isAttending });
    }

    /// <summary>
    /// Get the number of attendees for an event
    /// </summary>
    [HttpGet("event/{eventId}/count")]
    public async Task<ActionResult<int>> GetEventAttendeeCount(int eventId)
    {
        var count = await userEventService.GetEventAttendeeCountAsync(eventId);
        return Ok(new { attendeeCount = count });
    }
}

