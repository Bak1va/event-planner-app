using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Backend.Tests.Controllers.Unit;

/// <summary>
/// BDD-style unit tests for UsersController.GetUserById
/// Given: Service returns user or null
/// When: Endpoint is called with ID
/// Then: Correct HTTP response is returned
/// </summary>
public class UsersControllerGetUserByIdTests : UsersControllerUnitTestBase
{
    [Fact]
    public void GetUserById_GivenUserExists_WhenCalled_ThenReturnsOkWithUser()
    {
        // Given: User exists in service
        var user = new UserDto 
        { 
            Id = 1, 
            Name = "John", 
            Email = "john@example.com" 
        };
        MockUserService.Setup(s => s.GetUserById(1))
            .Returns(user);

        // When
        var result = Controller.GetUserById(1);

        // Then
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedUser = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(1, returnedUser.Id);
    }

    [Fact]
    public void GetUserById_GivenUserDoesNotExist_WhenCalled_ThenReturnsNotFound()
    {
        // Given: Attempting to access another user's data

        // When
        var result = Controller.GetUserById(999);

        // Then
        var forbidResult = Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public void GetUserById_GivenValidUserData_WhenCalled_ThenReturnsCompleteUserData()
    {
        // Given: Service returns user with all properties
        var user = new UserDto 
        { 
            Id = CurrentUser.Id, 
            Name = "Complete User", 
            Email = "complete@example.com",
            DateAdded = new DateTime(2024, 1, 1),
            DateModified = new DateTime(2024, 1, 2)
        };
        MockUserService.Setup(s => s.GetUserById(CurrentUser.Id))
            .Returns(user);

        // When
        var result = Controller.GetUserById(CurrentUser.Id);

        // Then
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUser = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("Complete User", returnedUser.Name);
        Assert.Equal("complete@example.com", returnedUser.Email);
    }
}

