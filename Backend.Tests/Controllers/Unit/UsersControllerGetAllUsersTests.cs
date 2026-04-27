using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Backend.Tests.Controllers.Unit;

/// <summary>
/// BDD-style unit tests for UsersController.GetAllUsers
/// Given: Service returns various states
/// When: Endpoint is called
/// Then: Correct HTTP response is returned
/// </summary>
public class UsersControllerGetAllUsersTests : UsersControllerUnitTestBase
{
    [Fact]
    public void GetAllUsers_GivenNoUsers_WhenCalled_ThenReturnsOkWithEmptyList()
    {
        // Given: Service returns no users
        MockUserService.Setup(s => s.GetAllUsers())
            .Returns(Enumerable.Empty<UserDto>());

        // When
        var result = Controller.GetAllUsers();

        // Then
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public void GetAllUsers_GivenMultipleUsers_WhenCalled_ThenReturnsOkWithAllUsers()
    {
        // Given: Service returns multiple users
        var users = new List<UserDto>
        {
            new UserDto { Id = 1, Name = "John", Email = "john@example.com" },
            new UserDto { Id = 2, Name = "Jane", Email = "jane@example.com" }
        };
        MockUserService.Setup(s => s.GetAllUsers())
            .Returns(users);

        // When
        var result = Controller.GetAllUsers();

        // Then
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value).ToList();
        Assert.Single(returnedUsers);
        Assert.Equal(CurrentUser.Id, returnedUsers[0].Id);
    }

    [Fact]
    public void GetAllUsers_GivenLargeUserList_WhenCalled_ThenReturnsAllUsers()
    {
        // Given: Service returns many users
        var users = Enumerable.Range(1, 100)
            .Select(i => new UserDto { Id = i, Name = $"User {i}", Email = $"user{i}@example.com" })
            .ToList();
        MockUserService.Setup(s => s.GetAllUsers())
            .Returns(users);

        // When
        var result = Controller.GetAllUsers();

        // Then
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value).ToList();
        Assert.Single(returnedUsers);
        Assert.Equal(CurrentUser.Id, returnedUsers[0].Id);
    }
}

