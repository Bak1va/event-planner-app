using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Services;

public interface IUserService
{
    IEnumerable<UserDto> GetAllUsers();
    UserDto? GetUserById(int id);
    bool EmailExists(string email);
    UserDto CreateUser(UserCreateRequest request);
    UserDto SignUp(SignUpRequest request);
    UserDto? Login(LoginRequest request);
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

    public bool EmailExists(string email)
    {
        return _usersStore.Values.Any(u =>
            u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public UserDto CreateUser(UserCreateRequest request)
    {
        var validationError = _validationService.ValidateUserRequest(request.Name, request.Email, request.Password);
        if (validationError is not null)
        {
            throw new ArgumentException(validationError);
        }

        if (EmailExists(request.Email))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var id = Interlocked.Increment(ref _nextUserId);
        var now = DateTime.UtcNow;
        var name = request.Name.Trim();

        var user = new User
        {
            Id = id,
            Name = name,
            FirstName = name,
            LastName = string.Empty,
            PhoneNumber = string.Empty,
            Email = request.Email.Trim(),
            PasswordHash = HashPassword(request.Password),
            DateAdded = now,
            DateModified = now
        };

        _usersStore[id] = user;
        return MapToDto(user);
    }

    public UserDto SignUp(SignUpRequest request)
    {
        var validationError = _validationService.ValidateSignUpRequest(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.PhoneNumber);
        if (validationError is not null)
        {
            throw new ArgumentException(validationError);
        }

        if (EmailExists(request.Email))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var id = Interlocked.Increment(ref _nextUserId);
        var now = DateTime.UtcNow;
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();

        var user = new User
        {
            Id = id,
            Name = string.Join(' ', new[] { firstName, lastName }.Where(part => !string.IsNullOrWhiteSpace(part))),
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = request.PhoneNumber.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = HashPassword(request.Password),
            DateAdded = now,
            DateModified = now
        };

        _usersStore[id] = user;
        return MapToDto(user);
    }

    public UserDto? Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = _usersStore.Values.FirstOrDefault(u =>
            u.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

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
            FirstName = request.Name.Trim(),
            LastName = string.Empty,
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

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] storedHashBytes = Convert.FromBase64String(parts[1]);
        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);
        return CryptographicOperations.FixedTimeEquals(inputHash, storedHashBytes);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            DateAdded = user.DateAdded,
            DateModified = user.DateModified
        };
    }
}
