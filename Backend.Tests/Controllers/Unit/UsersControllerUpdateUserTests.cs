using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Backend.Tests.Controllers.Unit;

/// <summary>
/// BDD-style unit tests for UsersController.UpdateUser
/// Given: User exists or doesn't exist
/// When: UpdateUser endpoint is called
/// Then: Appropriate HTTP response is returned
/// </summary>
public class UsersControllerUpdateUserTests : UsersControllerUnitTestBase
{
    [Fact]
    public void UpdateUser_GivenUserExists_WhenCalledWithValidData_ThenReturnsOkWithUpdatedUser()
    {
        // Given: User exists with update data
        var userId = 1;
        var request = new UserUpdateRequest 
        { 
            Name = "John Updated", 
            Email = "john.updated@example.com" 
        };
        var updatedUser = new UserDto 
        { 
            Id = userId, 
            Name = "John Updated", 
            Email = "john.updated@example.com" 
        };
        MockUserService.Setup(s => s.UpdateUser(userId, request))
            .Returns(updatedUser);

        // When
        var result = Controller.UpdateUser(userId, request);

        // Then
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedUser = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("John Updated", returnedUser.Name);
    }

    [Fact]
    public void UpdateUser_GivenUserNotFound_WhenCalled_ThenReturnsNotFound()
    {
        // Given: Attempting to update another user's data
        var request = new UserUpdateRequest 
        { 
            Name = "John", 
            Email = "john@example.com" 
        };

        // When
        var result = Controller.UpdateUser(999, request);

        // Then
        var forbidResult = Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public void UpdateUser_GivenInvalidData_WhenCalled_ThenReturnsBadRequest()
    {
        // Given: Invalid update data
        var userId = 1;
        var request = new UserUpdateRequest 
        { 
            Name = "", 
            Email = "john@example.com" 
        };
        MockUserService.Setup(s => s.UpdateUser(userId, request))
            .Throws(new ArgumentException("Name is required."));

        // When
        var result = Controller.UpdateUser(userId, request);

        // Then
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public void UpdateUser_GivenDuplicateEmail_WhenCalled_ThenReturnsConflict()
    {
        // Given: Attempting to update with existing email from another user
        var userId = 1;
        var request = new UserUpdateRequest 
        { 
            Name = "John", 
            Email = "existing@example.com" 
        };
        MockUserService.Setup(s => s.UpdateUser(userId, request))
            .Throws(new InvalidOperationException("Another user with this email already exists."));

        // When
        var result = Controller.UpdateUser(userId, request);

        // Then
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
    }
}

