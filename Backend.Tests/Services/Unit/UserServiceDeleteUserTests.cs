using Backend.DTOs;
using Xunit;

namespace Backend.Tests.Services.Unit;

/// <summary>
/// BDD-style unit tests for UserService.DeleteUser
/// Given: User exists or doesn't exist with various related data
/// When: DeleteUser is called
/// Then: User is deleted or exception is thrown
/// </summary>
public class UserServiceDeleteUserTests : UserServiceUnitTestBase
{
    [Fact]
    public void DeleteUser_GivenUserExistsWithNoEvents_WhenCalled_ThenDeletesUserSuccessfully()
    {
        // Given: A user exists with no related events
        SetupValidUserValidation();
        SetupEventServiceToReturnNoEvents();
        var createdUser = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "User to Delete", 
            Email = "delete@example.com" 
        });

        // When
        var result = UserService.DeleteUser(createdUser.Id);

        // Then
        Assert.True(result);
        Assert.Null(UserService.GetUserById(createdUser.Id));
    }

    [Fact]
    public void DeleteUser_GivenUserDoesNotExist_WhenCalled_ThenThrowsKeyNotFoundException()
    {
        // Given: No user with ID 999 exists
        SetupEventServiceToReturnNoEvents();

        // When & Then
        var exception = Assert.Throws<KeyNotFoundException>(() => UserService.DeleteUser(999));
        Assert.Contains("User with id 999 was not found", exception.Message);
    }

    [Fact]
    public void DeleteUser_GivenUserExistsWithRelatedEvents_WhenCalled_ThenThrowsInvalidOperationException()
    {
        // Given: A user exists with related events
        SetupValidUserValidation();
        var createdUser = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "User with Events", 
            Email = "user@example.com" 
        });
        var mockEvents = new List<EventDto> 
        { 
            new EventDto { Id = 1, Name = "Event", UserId = createdUser.Id } 
        };
        MockEventService.Setup(e => e.GetEventsByUserId(createdUser.Id))
            .Returns(mockEvents);

        // When & Then
        var exception = Assert.Throws<InvalidOperationException>(
            () => UserService.DeleteUser(createdUser.Id));
        Assert.Contains("Cannot delete user that has events", exception.Message);
    }

    [Fact]
    public void DeleteUser_GivenMultipleUsersExist_WhenDeletingOne_ThenOnlyThatUserIsDeleted()
    {
        // Given: Multiple users exist
        SetupValidUserValidation();
        SetupEventServiceToReturnNoEvents();
        var user1 = UserService.CreateUser(new UserCreateRequest { Name = "User 1", Email = "user1@example.com" });
        var user2 = UserService.CreateUser(new UserCreateRequest { Name = "User 2", Email = "user2@example.com" });
        var user3 = UserService.CreateUser(new UserCreateRequest { Name = "User 3", Email = "user3@example.com" });

        // When
        UserService.DeleteUser(user2.Id);

        // Then
        Assert.NotNull(UserService.GetUserById(user1.Id));
        Assert.Null(UserService.GetUserById(user2.Id));
        Assert.NotNull(UserService.GetUserById(user3.Id));
    }

    [Fact]
    public void DeleteUser_GivenUserExistsWithMultipleEvents_WhenCalled_ThenThrowsInvalidOperationException()
    {
        // Given: A user exists with multiple related events
        SetupValidUserValidation();
        var createdUser = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "User with Multiple Events", 
            Email = "user@example.com" 
        });
        var mockEvents = new List<EventDto> 
        { 
            new EventDto { Id = 1, Name = "Event 1", UserId = createdUser.Id },
            new EventDto { Id = 2, Name = "Event 2", UserId = createdUser.Id },
            new EventDto { Id = 3, Name = "Event 3", UserId = createdUser.Id }
        };
        MockEventService.Setup(e => e.GetEventsByUserId(createdUser.Id))
            .Returns(mockEvents);

        // When & Then
        var exception = Assert.Throws<InvalidOperationException>(
            () => UserService.DeleteUser(createdUser.Id));
        Assert.Contains("Cannot delete user that has events", exception.Message);
    }

    [Fact]
    public void DeleteUser_GivenDeletedUserAndNewUser_WhenCreatingNewUser_ThenCanReuseOldIdRange()
    {
        // Given: A user exists and is deleted, then another is created
        SetupValidUserValidation();
        SetupEventServiceToReturnNoEvents();
        var user1 = UserService.CreateUser(new UserCreateRequest { Name = "First User", Email = "first@example.com" });
        UserService.DeleteUser(user1.Id);
        
        // When
        var user2 = UserService.CreateUser(new UserCreateRequest { Name = "Second User", Email = "second@example.com" });

        // Then
        Assert.NotNull(user2);
        Assert.True(user2.Id > user1.Id); // IDs increment regardless of deletion
    }
}

