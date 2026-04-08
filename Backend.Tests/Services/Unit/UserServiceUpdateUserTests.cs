using Backend.DTOs;
using Xunit;

namespace Backend.Tests.Services.Unit;

/// <summary>
/// BDD-style unit tests for UserService.UpdateUser
/// Given: User exists or doesn't exist with various data states
/// When: UpdateUser is called
/// Then: User is updated or exception is thrown
/// </summary>
public class UserServiceUpdateUserTests : UserServiceUnitTestBase
{
    [Fact]
    public void UpdateUser_GivenUserExists_WhenCalledWithValidData_ThenUpdatesUserSuccessfully()
    {
        // Given: A user that exists with original data
        SetupValidUserValidation();
        var createdUser = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "Original Name", 
            Email = "original@example.com" 
        });
        var updateRequest = new UserUpdateRequest 
        { 
            Name = "Updated Name", 
            Email = "updated@example.com" 
        };

        // When
        var result = UserService.UpdateUser(createdUser.Id, updateRequest);

        // Then
        Assert.Equal(createdUser.Id, result.Id);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("updated@example.com", result.Email);
        Assert.True(result.DateModified >= createdUser.DateAdded);
    }

    [Fact]
    public void UpdateUser_GivenUserDoesNotExist_WhenCalledWithNonExistentId_ThenThrowsKeyNotFoundException()
    {
        // Given: No user with ID 999 exists
        SetupValidUserValidation();
        var updateRequest = new UserUpdateRequest 
        { 
            Name = "Some Name", 
            Email = "some@example.com" 
        };

        // When & Then
        var exception = Assert.Throws<KeyNotFoundException>(() => UserService.UpdateUser(999, updateRequest));
        Assert.Contains("User with id 999 was not found", exception.Message);
    }

    [Fact]
    public void UpdateUser_GivenMultipleUsersExist_WhenUpdatingWithDuplicateEmail_ThenThrowsInvalidOperationException()
    {
        // Given: Two users exist, updating one with another's email
        SetupValidUserValidation();
        var user1 = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "User One", 
            Email = "user1@example.com" 
        });
        var user2 = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "User Two", 
            Email = "user2@example.com" 
        });
        var updateRequest = new UserUpdateRequest 
        { 
            Name = "User One Updated", 
            Email = "user1@example.com"
        };

        // When & Then
        var exception = Assert.Throws<InvalidOperationException>(
            () => UserService.UpdateUser(user2.Id, updateRequest));
        Assert.Contains("Another user with this email already exists", exception.Message);
    }

    [Fact]
    public void UpdateUser_GivenInvalidData_WhenCalled_ThenThrowsArgumentException()
    {
        // Given: User exists but update data is invalid
        SetupValidUserValidation();
        var createdUser = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "Original", 
            Email = "original@example.com" 
        });
        SetupValidationError("Name is required.");
        var updateRequest = new UserUpdateRequest 
        { 
            Name = "", 
            Email = "updated@example.com" 
        };

        // When & Then
        var exception = Assert.Throws<ArgumentException>(
            () => UserService.UpdateUser(createdUser.Id, updateRequest));
        Assert.Contains("Name is required", exception.Message);
    }

    [Fact]
    public void UpdateUser_GivenUserExists_WhenUpdatingWithSameEmail_ThenUpdatesSuccessfully()
    {
        // Given: User with existing email being updated with same email
        SetupValidUserValidation();
        var user = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "Original", 
            Email = "test@example.com" 
        });
        var updateRequest = new UserUpdateRequest 
        { 
            Name = "Updated", 
            Email = "test@example.com" 
        };

        // When
        var result = UserService.UpdateUser(user.Id, updateRequest);

        // Then
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public void UpdateUser_GivenUserExists_WhenUpdatingWithWhitespace_ThenTrimsWhitespace()
    {
        // Given: A user that exists
        SetupValidUserValidation();
        var user = UserService.CreateUser(new UserCreateRequest 
        { 
            Name = "Original", 
            Email = "original@example.com" 
        });
        var updateRequest = new UserUpdateRequest 
        { 
            Name = "  Updated Name  ", 
            Email = "  updated@example.com  " 
        };

        // When
        var result = UserService.UpdateUser(user.Id, updateRequest);

        // Then
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("updated@example.com", result.Email);
    }
}

