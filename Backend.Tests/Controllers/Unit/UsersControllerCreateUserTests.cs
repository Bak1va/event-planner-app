using Backend.Controllers;
using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers.Unit;

/// <summary>
/// BDD-style unit tests for UsersController.CreateUser
/// Given: Valid or invalid user data
/// When: CreateUser endpoint is called
/// Then: Appropriate HTTP response is returned
/// </summary>
public class UsersControllerCreateUserTests : UsersControllerUnitTestBase
{
    [Fact]
    public void CreateUser_GivenValidData_WhenCalled_ThenReturnsCreatedAtAction()
    {
        // Given: Valid user creation request
        var request = new UserCreateRequest 
        { 
            Name = "John", 
            Email = "john@example.com" 
        };
        var createdUser = new UserDto 
        { 
            Id = 1, 
            Name = "John", 
            Email = "john@example.com" 
        };
        MockUserService.Setup(s => s.CreateUser(request))
            .Returns(createdUser);

        // When
        var result = Controller.CreateUser(request);

        // Then
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(UsersController.GetUserById), createdResult.ActionName);
        var returnedUser = Assert.IsType<UserDto>(createdResult.Value);
        Assert.Equal(1, returnedUser.Id);
    }

    [Fact]
    public void CreateUser_GivenInvalidName_WhenCalled_ThenReturnsBadRequest()
    {
        // Given: Service throws validation error
        var request = new UserCreateRequest 
        { 
            Name = "", 
            Email = "test@example.com" 
        };
        MockUserService.Setup(s => s.CreateUser(request))
            .Throws(new ArgumentException("Name is required."));

        // When
        var result = Controller.CreateUser(request);

        // Then
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public void CreateUser_GivenDuplicateEmail_WhenCalled_ThenReturnsConflict()
    {
        // Given: Service throws duplicate email error
        var request = new UserCreateRequest 
        { 
            Name = "John", 
            Email = "duplicate@example.com" 
        };
        MockUserService.Setup(s => s.CreateUser(request))
            .Throws(new InvalidOperationException("A user with this email already exists."));

        // When
        var result = Controller.CreateUser(request);

        // Then
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    [Fact]
    public void CreateUser_GivenValidRequest_WhenCalled_ThenCallsServiceOnce()
    {
        // Given: Valid request
        var request = new UserCreateRequest { Name = "John", Email = "john@example.com" };
        var createdUser = new UserDto { Id = 1, Name = "John", Email = "john@example.com" };
        MockUserService.Setup(s => s.CreateUser(request)).Returns(createdUser);

        // When
        Controller.CreateUser(request);

        // Then
        MockUserService.Verify(s => s.CreateUser(request), Times.Once);
    }
}


