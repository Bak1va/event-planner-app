using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// Get all events
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public ActionResult<IEnumerable<EventDto>> GetAllEvents()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        return Ok(_eventService.GetEventsByUserId(currentUserId.Value));
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public ActionResult<EventDto> GetEventById(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var eventItem = _eventService.GetEventById(id);
        if (eventItem == null)
        {
            return NotFound(new { message = $"Event with id {id} was not found." });
        }

        if (eventItem.UserId != currentUserId.Value)
        {
            return Forbid();
        }

        return Ok(eventItem);
    }

    /// <summary>
    /// Get all events for a specific user
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public ActionResult<IEnumerable<EventDto>> GetEventsByUserId(int userId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (currentUserId.Value != userId)
        {
            return Forbid();
        }

        return Ok(_eventService.GetEventsByUserId(userId));
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public ActionResult<EventDto> CreateEvent(EventCreateRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            request.UserId = currentUserId.Value;
            var eventItem = _eventService.CreateEvent(request);
            return CreatedAtAction(nameof(GetEventById), new { id = eventItem.Id }, eventItem);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update event by ID
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public ActionResult<EventDto> UpdateEvent(int id, EventUpdateRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var existingEvent = _eventService.GetEventById(id);
        if (existingEvent == null)
        {
            return NotFound(new { message = $"Event with id {id} was not found." });
        }

        if (existingEvent.UserId != currentUserId.Value)
        {
            return Forbid();
        }

        try
        {
            request.UserId = currentUserId.Value;
            var eventItem = _eventService.UpdateEvent(id, request);
            return Ok(eventItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete event by ID
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public ActionResult DeleteEvent(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var eventItem = _eventService.GetEventById(id);
        if (eventItem == null)
        {
            return NotFound(new { message = $"Event with id {id} was not found." });
        }

        if (eventItem.UserId != currentUserId.Value)
        {
            return Forbid();
        }

        try
        {
            _eventService.DeleteEvent(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private int? GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idValue, out var currentUserId))
        {
            return null;
        }

        return currentUserId;
    }
}

