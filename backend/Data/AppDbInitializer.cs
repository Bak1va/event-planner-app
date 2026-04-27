using Backend.Models;
using Backend.Services;

namespace Backend.Data;

public static class AppDbInitializer
{
    public static async Task InitializeAsync(AppDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (!dbContext.Users.Any())
        {
            var now = DateTime.UtcNow;

            dbContext.Users.AddRange(
                new User
                {
                    Name = "Event Team",
                    FirstName = "Event",
                    LastName = "Team",
                    PhoneNumber = "+1-555-0100",
                    Email = "team@eventplanner.local",
                    PasswordHash = UserService.HashPassword("password123"),
                    DateAdded = now,
                    DateModified = now
                },
                new User
                {
                    Name = "Community Host",
                    FirstName = "Community",
                    LastName = "Host",
                    PhoneNumber = "+1-555-0101",
                    Email = "host@eventplanner.local",
                    PasswordHash = UserService.HashPassword("password123"),
                    DateAdded = now,
                    DateModified = now
                });

            await dbContext.SaveChangesAsync();
        }

        if (dbContext.Events.Any())
        {
            return;
        }

        var baseDate = DateTime.UtcNow.Date.AddDays(2).AddHours(18);
        var createdAt = DateTime.UtcNow;

        dbContext.Events.AddRange(
            new EventItem { Name = "Spring Product Launch", Status = "Scheduled", Description = "A product reveal night with demos, founder Q&A, and networking.", ImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=900&q=80", EventDate = baseDate, DateAdded = createdAt, DateModified = createdAt, UserId = 1 },
            new EventItem { Name = "Design Systems Lab", Status = "Open", Description = "Hands-on workshop for building reusable UI patterns across teams.", ImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=900&q=80", EventDate = baseDate.AddDays(1), DateAdded = createdAt, DateModified = createdAt, UserId = 1 },
            new EventItem { Name = "Riverside Food Fair", Status = "Selling Fast", Description = "An outdoor evening packed with local chefs, live music, and tasting menus.", ImageUrl = "https://images.unsplash.com/photo-1414235077428-338989a2e8c0?auto=format&fit=crop&w=900&q=80", EventDate = baseDate.AddDays(2), DateAdded = createdAt, DateModified = createdAt, UserId = 2 },
            new EventItem { Name = "Startup Pitch Night", Status = "Scheduled", Description = "Early-stage founders present product ideas to mentors and investors.", ImageUrl = "https://images.unsplash.com/photo-1552664730-d307ca884978?auto=format&fit=crop&w=900&q=80", EventDate = baseDate.AddDays(3), DateAdded = createdAt, DateModified = createdAt, UserId = 1 },
            new EventItem { Name = "City Jazz Under Lights", Status = "Open", Description = "A late-night concert series with headline jazz performers downtown.", ImageUrl = "https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=900&q=80", EventDate = baseDate.AddDays(4), DateAdded = createdAt, DateModified = createdAt, UserId = 2 },
            new EventItem { Name = "Women in Tech Breakfast", Status = "Scheduled", Description = "Morning talks and small-group mentoring for engineering and product leaders.", ImageUrl = "https://images.unsplash.com/photo-1517457373958-b7bdd4587205?auto=format&fit=crop&w=900&q=80", EventDate = baseDate.AddDays(5).AddHours(-8), DateAdded = createdAt, DateModified = createdAt, UserId = 1 },
            new EventItem { Name = "Weekend Makers Market", Status = "Open", Description = "Shop handmade goods, ceramics, prints, and home decor from regional artists.", ImageUrl = "https://images.unsplash.com/photo-1520607162513-77705c0f0d4a?auto=format&fit=crop&w=900&q=80", EventDate = baseDate.AddDays(6).AddHours(-2), DateAdded = createdAt, DateModified = createdAt, UserId = 2 },
            new EventItem { Name = "Angular Frontend Clinic", Status = "Scheduled", Description = "A practical session on forms, validation, and API-driven UI architecture.", ImageUrl = "https://images.unsplash.com/photo-1516321497487-e288fb19713f?auto=format&fit=crop&w=900&q=80", EventDate = baseDate.AddDays(7), DateAdded = createdAt, DateModified = createdAt, UserId = 1 },
            new EventItem { Name = "Wellness on the Rooftop", Status = "Open", Description = "Sunset yoga, guided breathing, and healthy snacks with skyline views.", ImageUrl = "https://images.unsplash.com/photo-1506126613408-eca07ce68773?auto=format&fit=crop&w=900&q=80", EventDate = baseDate.AddDays(8), DateAdded = createdAt, DateModified = createdAt, UserId = 2 },
            new EventItem { Name = "Community Hack Day", Status = "Registration Required", Description = "Build civic tools with local developers, designers, and nonprofit teams.", ImageUrl = "https://images.unsplash.com/photo-1515169067868-5387ec356754?auto=format&fit=crop&w=900&q=80", EventDate = baseDate.AddDays(9), DateAdded = createdAt, DateModified = createdAt, UserId = 1 });

        await dbContext.SaveChangesAsync();
    }
}
