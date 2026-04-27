using Backend.Data;
using Backend.Entities;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Services.Integration;

/// <summary>
/// Integration tests for UserEventService
/// Tests the many-to-many relationship between users and events
/// </summary>
public class UserEventServiceIntegrationTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = Microsoft.EntityFrameworkCore.InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(
                new DbContextOptionsBuilder<AppDbContext>(),
                Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }

    private async Task<(User user, EventItem eventItem)> SetupTestDataAsync(AppDbContext context)
    {
        var user = new User
        {
            Name = "Test User",
            Email = "testuser@example.com",
            PasswordHash = "hashed_password",
            DateAdded = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Important: UserId must be set AFTER the user is saved so it has a real Id.
        var eventItem = new EventItem
        {
            Name = "Test Event",
            Status = "active",
            Description = "A test event",
            EventDate = DateTime.UtcNow.AddDays(7),
            DateAdded = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
            UserId = user.Id,
            ImageUrl = "https://example.com/image.jpg"
        };

        context.Events.Add(eventItem);
        await context.SaveChangesAsync();

        return (user, eventItem);
    }

    [Fact]
    public async Task AddUserToEvent_WithValidData_ReturnsUserEventDto()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var (user, eventItem) = await SetupTestDataAsync(context);
        var service = new UserEventService(context);

        var dto = new DTOs.CreateUserEventDto
        {
            UserId = user.Id,
            EventId = eventItem.Id
        };

        // Act
        var result = await service.AddUserToEventAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(eventItem.Id, result.EventId);
        Assert.True(result.DateJoined <= DateTime.UtcNow);
    }

    [Fact]
    public async Task AddUserToEvent_WithDuplicateAttendance_ThrowsException()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var (user, eventItem) = await SetupTestDataAsync(context);
        var service = new UserEventService(context);

        var dto = new DTOs.CreateUserEventDto
        {
            UserId = user.Id,
            EventId = eventItem.Id
        };

        await service.AddUserToEventAsync(dto); // First attendance

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddUserToEventAsync(dto)); // Duplicate attempt
        
        Assert.Contains("already attending", ex.Message);
    }

    [Fact]
    public async Task GetUserEvents_ReturnsAllEventsUserAttends()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var (user, eventItem) = await SetupTestDataAsync(context);
        var service = new UserEventService(context);

        var dto = new DTOs.CreateUserEventDto
        {
            UserId = user.Id,
            EventId = eventItem.Id
        };

        await service.AddUserToEventAsync(dto);

        // Act
        var result = (await service.GetUserEventsAsync(user.Id)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(eventItem.Id, result[0].EventId);
    }

    [Fact]
    public async Task RemoveUserFromEvent_WithValidData_ReturnsTrue()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var (user, eventItem) = await SetupTestDataAsync(context);
        var service = new UserEventService(context);

        var dto = new DTOs.CreateUserEventDto
        {
            UserId = user.Id,
            EventId = eventItem.Id
        };

        await service.AddUserToEventAsync(dto);

        // Act
        var result = await service.RemoveUserFromEventAsync(user.Id, eventItem.Id);

        // Assert
        Assert.True(result);
        var remaining = await service.GetUserEventsAsync(user.Id);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task IsUserAttendingEvent_ReturnsCorrectStatus()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var (user, eventItem) = await SetupTestDataAsync(context);
        var service = new UserEventService(context);

        // Act & Assert - Before joining
        var isAttending = await service.IsUserAttendingEventAsync(user.Id, eventItem.Id);
        Assert.False(isAttending);

        // Join event
        var dto = new DTOs.CreateUserEventDto
        {
            UserId = user.Id,
            EventId = eventItem.Id
        };
        await service.AddUserToEventAsync(dto);

        // Act & Assert - After joining
        isAttending = await service.IsUserAttendingEventAsync(user.Id, eventItem.Id);
        Assert.True(isAttending);
    }

    [Fact]
    public async Task GetEventAttendeeCount_ReturnsCorrectCount()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var (user1, eventItem) = await SetupTestDataAsync(context);
        var service = new UserEventService(context);

        var user2 = new User
        {
            Name = "User 2",
            Email = "user2@example.com",
            PasswordHash = "hashed_password",
            DateAdded = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        context.Users.Add(user2);
        await context.SaveChangesAsync();

        // Act - Add first attendee
        var dto1 = new DTOs.CreateUserEventDto
        {
            UserId = user1.Id,
            EventId = eventItem.Id
        };
        await service.AddUserToEventAsync(dto1);

        var count = await service.GetEventAttendeeCountAsync(eventItem.Id);
        Assert.Equal(1, count);

        // Add second attendee
        var dto2 = new DTOs.CreateUserEventDto
        {
            UserId = user2.Id,
            EventId = eventItem.Id
        };
        await service.AddUserToEventAsync(dto2);

        count = await service.GetEventAttendeeCountAsync(eventItem.Id);
        Assert.Equal(2, count);
    }
}
