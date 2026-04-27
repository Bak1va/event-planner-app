using Backend.Data;
using Backend.DTOs;
using Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class UserEventService(AppDbContext context)
{
    public async Task<UserEventDto> AddUserToEventAsync(CreateUserEventDto dto)
    {
        var userExists = await context.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExists)
            throw new InvalidOperationException($"User with ID {dto.UserId} not found.");

        var eventExists = await context.Events.AnyAsync(e => e.Id == dto.EventId);
        if (!eventExists)
            throw new InvalidOperationException($"Event with ID {dto.EventId} not found.");

        var alreadyAttending = await context.UserEvents
            .AnyAsync(ue => ue.UserId == dto.UserId && ue.EventId == dto.EventId);

        if (alreadyAttending)
            throw new InvalidOperationException($"User {dto.UserId} is already attending event {dto.EventId}.");

        var userEvent = new UserEvent
        {
            UserId = dto.UserId,
            EventId = dto.EventId,
            DateJoined = DateTime.UtcNow
        };

        context.UserEvents.Add(userEvent);
        await context.SaveChangesAsync();

        return new UserEventDto
        {
            Id = userEvent.Id,
            UserId = userEvent.UserId,
            EventId = userEvent.EventId,
            DateJoined = userEvent.DateJoined
        };
    }

    public async Task<IReadOnlyList<UserEventDto>> GetUserEventsAsync(int userId)
    {
        return await context.UserEvents
            .Where(ue => ue.UserId == userId)
            .OrderByDescending(ue => ue.DateJoined)
            .Select(ue => new UserEventDto
            {
                Id = ue.Id,
                UserId = ue.UserId,
                EventId = ue.EventId,
                DateJoined = ue.DateJoined
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UserEventDto>> GetEventAttendeesAsync(int eventId)
    {
        return await context.UserEvents
            .Where(ue => ue.EventId == eventId)
            .OrderByDescending(ue => ue.DateJoined)
            .Select(ue => new UserEventDto
            {
                Id = ue.Id,
                UserId = ue.UserId,
                EventId = ue.EventId,
                DateJoined = ue.DateJoined
            })
            .ToListAsync();
    }

    public async Task<bool> RemoveUserFromEventAsync(int userId, int eventId)
    {
        var userEvent = await context.UserEvents
            .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.EventId == eventId);

        if (userEvent == null)
            return false;

        context.UserEvents.Remove(userEvent);
        await context.SaveChangesAsync();
        return true;
    }

    public Task<bool> IsUserAttendingEventAsync(int userId, int eventId)
    {
        return context.UserEvents.AnyAsync(ue => ue.UserId == userId && ue.EventId == eventId);
    }

    public Task<int> GetEventAttendeeCountAsync(int eventId)
    {
        return context.UserEvents.CountAsync(ue => ue.EventId == eventId);
    }
}
