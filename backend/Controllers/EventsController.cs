using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        return Ok(_eventService.GetAllEvents());
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<EventDto> GetEventById(int id)
    {
        var eventItem = _eventService.GetEventById(id);
        if (eventItem == null)
        {
            return NotFound(new { message = $"Event with id {id} was not found." });
        }
        return Ok(eventItem);
    }

    /// <summary>
    /// Get all events for a specific user
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(200)]
    public ActionResult<IEnumerable<EventDto>> GetEventsByUserId(int userId)
    {
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
        try
        {
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
    public ActionResult<EventDto> UpdateEvent(int id, EventUpdateRequest request)
    {
        try
        {
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
    public ActionResult DeleteEvent(int id)
    {
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
}

