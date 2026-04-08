using Backend.DTOs;
using Xunit;

namespace Backend.Tests.Services.Unit;

/// <summary>
/// BDD-style unit tests for UserService.GetUserById
/// Given: User exists or doesn't exist
/// When: GetUserById is called
/// Then: Returns the user or null
/// </summary>
public class UserServiceGetUserByIdTests : UserServiceUnitTestBase
{
    [Fact]
    public void GetUserById_GivenUserExists_WhenCalledWithValidId_ThenReturnsUser()
    {
        // Given: A user has been created
        SetupValidUserValidation();
        var createdUser = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "John Doe", 
            Email = "john@example.com" 
        });

        // When
        var result = UserService.GetUserById(createdUser.Id);

        // Then
        Assert.NotNull(result);
        Assert.Equal(createdUser.Id, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public void GetUserById_GivenUserDoesNotExist_WhenCalledWithInvalidId_ThenReturnsNull()
    {
        // Given: No user with ID 999 exists
        var nonExistentId = 999;

        // When
        var result = UserService.GetUserById(nonExistentId);

        // Then
        Assert.Null(result);
    }

    [Fact]
    public void GetUserById_GivenMultipleUsersExist_WhenCalledWithSpecificId_ThenReturnsCorrectUser()
    {
        // Given: Multiple users exist
        SetupValidUserValidation();
        var user1 = UserService.CreateUser(new UserCreateRequest { Name = "User 1", Email = "user1@example.com" });
        var user2 = UserService.CreateUser(new UserCreateRequest { Name = "User 2", Email = "user2@example.com" });
        var user3 = UserService.CreateUser(new UserCreateRequest { Name = "User 3", Email = "user3@example.com" });

        // When
        var result = UserService.GetUserById(user2.Id);

        // Then
        Assert.NotNull(result);
        Assert.Equal(user2.Id, result.Id);
        Assert.Equal("User 2", result.Name);
        Assert.NotEqual(user1.Id, result.Id);
        Assert.NotEqual(user3.Id, result.Id);
    }

    [Fact]
    public void GetUserById_GivenUserExists_WhenCalled_ThenReturnsUserWithCorrectProperties()
    {
        // Given: A user with specific properties
        SetupValidUserValidation();
        var createdUser = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "Test User", 
            Email = "test@example.com" 
        });

        // When
        var result = UserService.GetUserById(createdUser.Id);

        // Then
        Assert.NotNull(result);
        Assert.NotEqual(default(DateTime), result.DateAdded);
        Assert.NotEqual(default(DateTime), result.DateModified);
        Assert.Equal(result.DateAdded, result.DateModified);
    }
}

