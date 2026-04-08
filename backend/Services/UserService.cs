using System.Collections.Concurrent;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Services;

public interface IUserService
{
    IEnumerable<UserDto> GetAllUsers();
    UserDto? GetUserById(int id);
    UserDto CreateUser(UserCreateRequest request);
    UserDto UpdateUser(int id, UserUpdateRequest request);
    bool DeleteUser(int id);
}

public class UserService : IUserService
{
    private readonly ConcurrentDictionary<int, User> _usersStore;
    private readonly IValidationService _validationService;
    private IEventService? _eventService;
    private int _nextUserId = 1;

    public UserService(IValidationService validationService)
    {
        _usersStore = new ConcurrentDictionary<int, User>();
        _validationService = validationService;
        _eventService = null;
    }

    public void SetEventService(IEventService eventService)
    {
        _eventService = eventService;
    }

    public IEnumerable<UserDto> GetAllUsers()
    {
        return _usersStore.Values
            .OrderBy(u => u.Id)
            .Select(MapToDto);
    }

    public UserDto? GetUserById(int id)
    {
        if (_usersStore.TryGetValue(id, out var user))
        {
            return MapToDto(user);
        }
        return null;
    }

    public UserDto CreateUser(UserCreateRequest request)
    {
        var validationError = _validationService.ValidateUserRequest(request.Name, request.Email);
        if (validationError is not null)
        {
            throw new ArgumentException(validationError);
        }

        if (_usersStore.Values.Any(u => u.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var id = Interlocked.Increment(ref _nextUserId);
        var now = DateTime.UtcNow;

        var user = new User(
            id,
            request.Name.Trim(),
            request.Email.Trim(),
            now,
            now
        );

        _usersStore[id] = user;
        return MapToDto(user);
    }

    public UserDto UpdateUser(int id, UserUpdateRequest request)
    {
        if (!_usersStore.TryGetValue(id, out var existingUser))
        {
            throw new KeyNotFoundException($"User with id {id} was not found.");
        }

        var validationError = _validationService.ValidateUserRequest(request.Name, request.Email);
        if (validationError is not null)
        {
            throw new ArgumentException(validationError);
        }

        if (_usersStore.Values.Any(u => u.Id != id && u.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Another user with this email already exists.");
        }

        var updated = existingUser with
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            DateModified = DateTime.UtcNow
        };

        _usersStore[id] = updated;
        return MapToDto(updated);
    }

    public bool DeleteUser(int id)
    {
        if (!_usersStore.ContainsKey(id))
        {
            throw new KeyNotFoundException($"User with id {id} was not found.");
        }

        if (_eventService != null && _eventService.GetEventsByUserId(id).Any())
        {
            throw new InvalidOperationException("Cannot delete user that has events. Delete events first.");
        }

        return _usersStore.TryRemove(id, out _);
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            DateAdded = user.DateAdded,
            DateModified = user.DateModified
        };
    }
}

