using Backend.DTOs;
using Backend.Services;
using Xunit;

namespace Backend.Tests.Services.Unit;

/// <summary>
/// Unit tests for event data validation rules.
/// Given: Various event creation/update inputs
/// When: CreateEvent / UpdateEvent is called
/// Then: Validation errors are raised for invalid data
/// </summary>
public class EventValidationTests
{
    private static (EventService service, UserService userService) CreateServices()
    {
        var validation = new ValidationService();
        var userService = new UserService(validation);
        var eventService = new EventService(validation);
        userService.SetEventService(eventService);
        eventService.SetUserService(userService);
        return (eventService, userService);
    }

    private static UserDto CreateUser(UserService userService, string email = "owner@example.com")
        => userService.CreateUser(new UserCreateRequest
        {
            Name = "Owner",
            Email = email,
            Password = "password123"
        });

    // -------------------------------------------------------------------------
    // Empty name
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateEvent_WithEmptyName_ThrowsArgumentException()
    {
        var (service, userService) = CreateServices();
        var user = CreateUser(userService);

        var ex = Assert.Throws<ArgumentException>(() =>
            service.CreateEvent(new EventCreateRequest
            {
                Name = "",
                Status = "Planned",
                EventDate = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            }));

        Assert.Equal("Event name is required.", ex.Message);
    }

    [Fact]
    public void CreateEvent_WithWhitespaceName_ThrowsArgumentException()
    {
        var (service, userService) = CreateServices();
        var user = CreateUser(userService);

        var ex = Assert.Throws<ArgumentException>(() =>
            service.CreateEvent(new EventCreateRequest
            {
                Name = "   ",
                Status = "Planned",
                EventDate = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            }));

        Assert.Equal("Event name is required.", ex.Message);
    }

    [Fact]
    public void UpdateEvent_WithEmptyName_ThrowsArgumentException()
    {
        var (service, userService) = CreateServices();
        var user = CreateUser(userService);
        var created = service.CreateEvent(new EventCreateRequest
        {
            Name = "Valid Name",
            Status = "Planned",
            EventDate = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        });

        var ex = Assert.Throws<ArgumentException>(() =>
            service.UpdateEvent(created.Id, new EventUpdateRequest
            {
                Name = "",
                Status = "Planned",
                EventDate = DateTime.UtcNow.AddDays(14),
                UserId = user.Id
            }));

        Assert.Equal("Event name is required.", ex.Message);
    }

    // -------------------------------------------------------------------------
    // Past date
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateEvent_WithPastDate_ThrowsArgumentException()
    {
        var (service, userService) = CreateServices();
        var user = CreateUser(userService);

        var ex = Assert.Throws<ArgumentException>(() =>
            service.CreateEvent(new EventCreateRequest
            {
                Name = "Past Event",
                Status = "Planned",
                EventDate = DateTime.UtcNow.AddDays(-1),
                UserId = user.Id
            }));

        Assert.Equal("Event date must be in the future.", ex.Message);
    }

    [Fact]
    public void CreateEvent_WithCurrentDateTime_ThrowsArgumentException()
    {
        var (service, userService) = CreateServices();
        var user = CreateUser(userService);

        // DateTime.UtcNow is not in the future
        var ex = Assert.Throws<ArgumentException>(() =>
            service.CreateEvent(new EventCreateRequest
            {
                Name = "Now Event",
                Status = "Planned",
                EventDate = DateTime.UtcNow,
                UserId = user.Id
            }));

        Assert.Equal("Event date must be in the future.", ex.Message);
    }

    [Fact]
    public void UpdateEvent_WithPastDate_ThrowsArgumentException()
    {
        var (service, userService) = CreateServices();
        var user = CreateUser(userService);
        var created = service.CreateEvent(new EventCreateRequest
        {
            Name = "Future Event",
            Status = "Planned",
            EventDate = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        });

        var ex = Assert.Throws<ArgumentException>(() =>
            service.UpdateEvent(created.Id, new EventUpdateRequest
            {
                Name = "Future Event",
                Status = "Planned",
                EventDate = DateTime.UtcNow.AddDays(-1),
                UserId = user.Id
            }));

        Assert.Equal("Event date must be in the future.", ex.Message);
    }

    // -------------------------------------------------------------------------
    // Happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateEvent_WithValidNameAndFutureDate_Succeeds()
    {
        var (service, userService) = CreateServices();
        var user = CreateUser(userService);
        var futureDate = DateTime.UtcNow.AddDays(30);

        var result = service.CreateEvent(new EventCreateRequest
        {
            Name = "Summer Conference",
            Status = "Planned",
            Description = "Annual tech summit",
            EventDate = futureDate,
            UserId = user.Id
        });

        Assert.NotNull(result);
        Assert.Equal("Summer Conference", result.Name);
        Assert.Equal(futureDate.Date, result.EventDate.Date);
    }

    [Fact]
    public void CreateEvent_WithNameExceeding150Chars_ThrowsArgumentException()
    {
        var (service, userService) = CreateServices();
        var user = CreateUser(userService);

        var ex = Assert.Throws<ArgumentException>(() =>
            service.CreateEvent(new EventCreateRequest
            {
                Name = new string('A', 151),
                Status = "Planned",
                EventDate = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            }));

        Assert.Contains("Event name cannot exceed 150 characters", ex.Message);
    }
}
