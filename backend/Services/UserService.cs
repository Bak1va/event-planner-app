using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

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
    private readonly AppDbContext? _dbContext;
    private readonly ConcurrentDictionary<int, User>? _usersStore;
    private readonly IValidationService _validationService;
    private IEventService? _eventService;
    private int _nextUserId;

    public UserService(IValidationService validationService)
    {
        _usersStore = new ConcurrentDictionary<int, User>();
        _validationService = validationService;
    }

    public UserService(AppDbContext dbContext, IValidationService validationService)
    {
        _dbContext = dbContext;
        _validationService = validationService;
    }

    public void SetEventService(IEventService eventService)
    {
        _eventService = eventService;
    }

    public IEnumerable<UserDto> GetAllUsers()
    {
        if (_dbContext is not null)
        {
            return _dbContext.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    DateAdded = user.DateAdded,
                    DateModified = user.DateModified
                })
                .ToList();
        }

        return _usersStore!.Values
            .OrderBy(u => u.Id)
            .Select(MapToDto);
    }

    public UserDto? GetUserById(int id)
    {
        if (_dbContext is not null)
        {
            var dbUser = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
            return dbUser is null ? null : MapToDto(dbUser);
        }

        if (_usersStore!.TryGetValue(id, out var storedUser))
        {
            return MapToDto(storedUser);
        }

        return null;
    }

    public bool EmailExists(string email)
    {
        var normalizedEmail = email.Trim();

        if (_dbContext is not null)
        {
            return _dbContext.Users.Any(u =>
                u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
        }

        return _usersStore!.Values.Any(u =>
            u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
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

        var now = DateTime.UtcNow;
        var name = request.Name.Trim();
        var email = request.Email.Trim();

        if (_dbContext is not null)
        {
            var user = new User
            {
                Name = name,
                FirstName = name,
                LastName = string.Empty,
                PhoneNumber = string.Empty,
                Email = email,
                PasswordHash = HashPassword(request.Password),
                DateAdded = now,
                DateModified = now
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return MapToDto(user);
        }

        var id = Interlocked.Increment(ref _nextUserId);
        var inMemoryUser = new User
        {
            Id = id,
            Name = name,
            FirstName = name,
            LastName = string.Empty,
            PhoneNumber = string.Empty,
            Email = email,
            PasswordHash = HashPassword(request.Password),
            DateAdded = now,
            DateModified = now
        };

        _usersStore![id] = inMemoryUser;
        return MapToDto(inMemoryUser);
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

        var now = DateTime.UtcNow;
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var fullName = string.Join(' ', new[] { firstName, lastName }.Where(part => !string.IsNullOrWhiteSpace(part)));

        if (_dbContext is not null)
        {
            var user = new User
            {
                Name = fullName,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = request.PhoneNumber.Trim(),
                Email = request.Email.Trim(),
                PasswordHash = HashPassword(request.Password),
                DateAdded = now,
                DateModified = now
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return MapToDto(user);
        }

        var id = Interlocked.Increment(ref _nextUserId);
        var inMemoryUser = new User
        {
            Id = id,
            Name = fullName,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = request.PhoneNumber.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = HashPassword(request.Password),
            DateAdded = now,
            DateModified = now
        };

        _usersStore![id] = inMemoryUser;
        return MapToDto(inMemoryUser);
    }

    public UserDto? Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        if (_dbContext is not null)
        {
            var dbUser = _dbContext.Users.FirstOrDefault(u =>
                u.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

            if (dbUser is null || !VerifyPassword(request.Password, dbUser.PasswordHash))
            {
                return null;
            }

            return MapToDto(dbUser);
        }

        var user = _usersStore!.Values.FirstOrDefault(u =>
            u.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        return MapToDto(user);
    }

    public UserDto UpdateUser(int id, UserUpdateRequest request)
    {
        var validationError = _validationService.ValidateUserRequest(request.Name, request.Email);
        if (validationError is not null)
        {
            throw new ArgumentException(validationError);
        }

        var normalizedEmail = request.Email.Trim();

        if (_dbContext is not null)
        {
            var existingUser = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (existingUser is null)
            {
                throw new KeyNotFoundException($"User with id {id} was not found.");
            }

            var emailInUse = _dbContext.Users.Any(u =>
                u.Id != id &&
                u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));

            if (emailInUse)
            {
                throw new InvalidOperationException("Another user with this email already exists.");
            }

            existingUser.Name = request.Name.Trim();
            existingUser.FirstName = request.Name.Trim();
            existingUser.LastName = string.Empty;
            existingUser.Email = normalizedEmail;
            existingUser.DateModified = DateTime.UtcNow;

            _dbContext.SaveChanges();
            return MapToDto(existingUser);
        }

        if (!_usersStore!.TryGetValue(id, out var storedUser))
        {
            throw new KeyNotFoundException($"User with id {id} was not found.");
        }

        var inMemoryEmailInUse = _usersStore.Values.Any(u =>
            u.Id != id &&
            u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));

        if (inMemoryEmailInUse)
        {
            throw new InvalidOperationException("Another user with this email already exists.");
        }

        storedUser.Name = request.Name.Trim();
        storedUser.FirstName = request.Name.Trim();
        storedUser.LastName = string.Empty;
        storedUser.Email = normalizedEmail;
        storedUser.DateModified = DateTime.UtcNow;

        return MapToDto(storedUser);
    }

    public bool DeleteUser(int id)
    {
        if (_dbContext is not null)
        {
            var existingUser = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (existingUser is null)
            {
                throw new KeyNotFoundException($"User with id {id} was not found.");
            }

            var hasEvents = _dbContext.Events.Any(e => e.UserId == id);
            if (hasEvents)
            {
                throw new InvalidOperationException("Cannot delete user that has events. Delete events first.");
            }

            _dbContext.Users.Remove(existingUser);
            _dbContext.SaveChanges();
            return true;
        }

        if (!_usersStore!.ContainsKey(id))
        {
            throw new KeyNotFoundException($"User with id {id} was not found.");
        }

        if (_eventService != null && _eventService.GetEventsByUserId(id).Any())
        {
            throw new InvalidOperationException("Cannot delete user that has events. Delete events first.");
        }

        return _usersStore.TryRemove(id, out _);
    }

    internal static string HashPassword(string password)
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
