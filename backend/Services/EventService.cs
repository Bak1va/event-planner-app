using System.Collections.Concurrent;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

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
    private readonly AppDbContext? _dbContext;
    private readonly ConcurrentDictionary<int, EventItem>? _eventsStore;
    private readonly IValidationService _validationService;
    private IUserService? _userService;
    private int _nextEventId;

    public EventService(IValidationService validationService)
    {
        _eventsStore = new ConcurrentDictionary<int, EventItem>();
        _validationService = validationService;
    }

    public EventService(AppDbContext dbContext, IValidationService validationService)
    {
        _dbContext = dbContext;
        _validationService = validationService;
    }

    public void SetUserService(IUserService userService)
    {
        _userService = userService;
    }

    public IEnumerable<EventDto> GetAllEvents()
    {
        if (_dbContext is not null)
        {
            return _dbContext.Events
                .AsNoTracking()
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.Id)
                .Select(eventItem => new EventDto
                {
                    Id = eventItem.Id,
                    Name = eventItem.Name,
                    Status = eventItem.Status,
                    Description = eventItem.Description,
                    ImageUrl = eventItem.ImageUrl,
                    EventDate = eventItem.EventDate,
                    DateAdded = eventItem.DateAdded,
                    DateModified = eventItem.DateModified,
                    UserId = eventItem.UserId
                })
                .ToList();
        }

        return _eventsStore!.Values
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.Id)
            .Select(MapToDto);
    }

    public EventDto? GetEventById(int id)
    {
        if (_dbContext is not null)
        {
            var eventItem = _dbContext.Events.AsNoTracking().FirstOrDefault(e => e.Id == id);
            return eventItem is null ? null : MapToDto(eventItem);
        }

        if (_eventsStore!.TryGetValue(id, out var storedEvent))
        {
            return MapToDto(storedEvent);
        }

        return null;
    }

    public IEnumerable<EventDto> GetEventsByUserId(int userId)
    {
        if (_dbContext is not null)
        {
            return _dbContext.Events
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.Id)
                .Select(eventItem => new EventDto
                {
                    Id = eventItem.Id,
                    Name = eventItem.Name,
                    Status = eventItem.Status,
                    Description = eventItem.Description,
                    ImageUrl = eventItem.ImageUrl,
                    EventDate = eventItem.EventDate,
                    DateAdded = eventItem.DateAdded,
                    DateModified = eventItem.DateModified,
                    UserId = eventItem.UserId
                })
                .ToList();
        }

        return _eventsStore!.Values
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.Id)
            .Select(MapToDto);
    }

    public EventDto CreateEvent(EventCreateRequest request)
    {
        ValidateRequest(request.Name, request.Status, request.Description, request.ImageUrl, request.EventDate, request.UserId);

        var now = DateTime.UtcNow;

        if (_dbContext is not null)
        {
            var eventItem = new EventItem
            {
                Name = request.Name.Trim(),
                Status = request.Status.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
                EventDate = request.EventDate.ToUniversalTime(),
                DateAdded = now,
                DateModified = now,
                UserId = request.UserId
            };

            _dbContext.Events.Add(eventItem);
            _dbContext.SaveChanges();

            return MapToDto(eventItem);
        }

        var id = Interlocked.Increment(ref _nextEventId);
        var inMemoryEvent = new EventItem
        {
            Id = id,
            Name = request.Name.Trim(),
            Status = request.Status.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            EventDate = request.EventDate.ToUniversalTime(),
            DateAdded = now,
            DateModified = now,
            UserId = request.UserId
        };

        _eventsStore![id] = inMemoryEvent;
        return MapToDto(inMemoryEvent);
    }

    public EventDto UpdateEvent(int id, EventUpdateRequest request)
    {
        ValidateRequest(request.Name, request.Status, request.Description, request.ImageUrl, request.EventDate, request.UserId);

        if (_dbContext is not null)
        {
            var existingEvent = _dbContext.Events.FirstOrDefault(e => e.Id == id);
            if (existingEvent is null)
            {
                throw new KeyNotFoundException($"Event with id {id} was not found.");
            }

            existingEvent.Name = request.Name.Trim();
            existingEvent.Status = request.Status.Trim();
            existingEvent.Description = request.Description?.Trim() ?? string.Empty;
            existingEvent.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
            existingEvent.EventDate = request.EventDate.ToUniversalTime();
            existingEvent.UserId = request.UserId;
            existingEvent.DateModified = DateTime.UtcNow;

            _dbContext.SaveChanges();
            return MapToDto(existingEvent);
        }

        if (!_eventsStore!.TryGetValue(id, out var storedEvent))
        {
            throw new KeyNotFoundException($"Event with id {id} was not found.");
        }

        storedEvent.Name = request.Name.Trim();
        storedEvent.Status = request.Status.Trim();
        storedEvent.Description = request.Description?.Trim() ?? string.Empty;
        storedEvent.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
        storedEvent.EventDate = request.EventDate.ToUniversalTime();
        storedEvent.UserId = request.UserId;
        storedEvent.DateModified = DateTime.UtcNow;

        return MapToDto(storedEvent);
    }

    public bool DeleteEvent(int id)
    {
        if (_dbContext is not null)
        {
            var existingEvent = _dbContext.Events.FirstOrDefault(e => e.Id == id);
            if (existingEvent is null)
            {
                throw new KeyNotFoundException($"Event with id {id} was not found.");
            }

            _dbContext.Events.Remove(existingEvent);
            _dbContext.SaveChanges();
            return true;
        }

        if (!_eventsStore!.ContainsKey(id))
        {
            throw new KeyNotFoundException($"Event with id {id} was not found.");
        }

        return _eventsStore.TryRemove(id, out _);
    }

    private void ValidateRequest(string? name, string? status, string? description, string? imageUrl, DateTime eventDate, int userId)
    {
        var validationError = _validationService.ValidateEventRequest(name, status, description, imageUrl, eventDate);
        if (validationError is not null)
        {
            throw new ArgumentException(validationError);
        }

        if (_dbContext is not null)
        {
            var userExists = _dbContext.Users.Any(user => user.Id == userId);
            if (!userExists)
            {
                throw new ArgumentException("Invalid userId. User does not exist.");
            }

            return;
        }

        if (_userService != null && _userService.GetUserById(userId) == null)
        {
            throw new ArgumentException("Invalid userId. User does not exist.");
        }
    }

    private static EventDto MapToDto(EventItem eventItem)
    {
        return new EventDto
        {
            Id = eventItem.Id,
            Name = eventItem.Name,
            Status = eventItem.Status,
            Description = eventItem.Description,
            ImageUrl = eventItem.ImageUrl,
            EventDate = eventItem.EventDate,
            DateAdded = eventItem.DateAdded,
            DateModified = eventItem.DateModified,
            UserId = eventItem.UserId
        };
    }
}
