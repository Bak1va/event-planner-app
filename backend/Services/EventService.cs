using System.Collections.Concurrent;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Services;

public interface IEventService
{
    IEnumerable<EventDto> GetAllEvents();
    EventDto? GetEventById(int id);
    IEnumerable<EventDto> GetEventsByUserId(int userId);
    EventDto CreateEvent(EventCreateRequest request);
    EventDto UpdateEvent(int id, EventUpdateRequest request);
    bool DeleteEvent(int id);
}

public class EventService : IEventService
{
    private readonly ConcurrentDictionary<int, EventItem> _eventsStore;
    private readonly IValidationService _validationService;
    private IUserService? _userService;
    private int _nextEventId = 1;

    public EventService(IValidationService validationService)
    {
        _eventsStore = new ConcurrentDictionary<int, EventItem>();
        _validationService = validationService;
        _userService = null;
    }

    public void SetUserService(IUserService userService)
    {
        _userService = userService;
    }

    public IEnumerable<EventDto> GetAllEvents()
    {
        return _eventsStore.Values
            .OrderBy(e => e.Id)
            .Select(MapToDto);
    }

    public EventDto? GetEventById(int id)
    {
        if (_eventsStore.TryGetValue(id, out var eventItem))
        {
            return MapToDto(eventItem);
        }
        return null;
    }

    public IEnumerable<EventDto> GetEventsByUserId(int userId)
    {
        return _eventsStore.Values
            .Where(e => e.UserId == userId)
            .Select(MapToDto);
    }

    public EventDto CreateEvent(EventCreateRequest request)
    {
        var validationError = _validationService.ValidateEventRequest(request.Name, request.Status, request.Description, request.ImageUrl);
        if (validationError is not null)
        {
            throw new ArgumentException(validationError);
        }

        if (_userService != null && _userService.GetUserById(request.UserId) == null)
        {
            throw new ArgumentException("Invalid userId. User does not exist.");
        }

        var id = Interlocked.Increment(ref _nextEventId);
        var now = DateTime.UtcNow;

        var eventItem = new EventItem(
            id,
            request.Name.Trim(),
            request.Status.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.ImageUrl?.Trim() ?? string.Empty,
            now,
            now,
            request.UserId
        );

        _eventsStore[id] = eventItem;
        return MapToDto(eventItem);
    }

    public EventDto UpdateEvent(int id, EventUpdateRequest request)
    {
        if (!_eventsStore.TryGetValue(id, out var existingEvent))
        {
            throw new KeyNotFoundException($"Event with id {id} was not found.");
        }

        var validationError = _validationService.ValidateEventRequest(request.Name, request.Status, request.Description, request.ImageUrl);
        if (validationError is not null)
        {
            throw new ArgumentException(validationError);
        }

        if (_userService != null && _userService.GetUserById(request.UserId) == null)
        {
            throw new ArgumentException("Invalid userId. User does not exist.");
        }

        var updated = existingEvent with
        {
            Name = request.Name.Trim(),
            Status = request.Status.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            UserId = request.UserId,
            DateModified = DateTime.UtcNow
        };

        _eventsStore[id] = updated;
        return MapToDto(updated);
    }

    public bool DeleteEvent(int id)
    {
        if (!_eventsStore.ContainsKey(id))
        {
            throw new KeyNotFoundException($"Event with id {id} was not found.");
        }

        return _eventsStore.TryRemove(id, out _);
    }

    private EventDto MapToDto(EventItem eventItem)
    {
        return new EventDto
        {
            Id = eventItem.Id,
            Name = eventItem.Name,
            Status = eventItem.Status,
            Description = eventItem.Description,
            ImageUrl = eventItem.ImageUrl,
            DateAdded = eventItem.DateAdded,
            DateModified = eventItem.DateModified,
            UserId = eventItem.UserId
        };
    }
}

