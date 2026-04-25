using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Backend.Tests.Controllers.Integration;

/// <summary>
/// Integration tests for UsersController with real services
/// Given: Controller with real service implementations
/// When: HTTP operations are performed
/// Then: Correct responses are returned
/// </summary>
public class UsersControllerIntegrationTests : UsersControllerIntegrationTestBase
{
    [Fact]
    public void CreateUserEndpoint_GivenValidUserData_WhenCalled_ThenReturnsCreatedResponse()
    {
        // Given: Valid user creation request
        var request = new UserCreateRequest
        {
            Name = "Integration User",
            Email = "integration@example.com",
            Password = "password123"
        };

        // When
        var result = Controller.CreateUser(request);

        // Then
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        var returnedUser = Assert.IsType<UserDto>(createdResult.Value);
        Assert.Equal("Integration User", returnedUser.Name);
    }

    [Fact]
    public void GetUserEndpoint_GivenCreatedUser_WhenCalled_ThenReturnsUserData()
    {
        // Given: A user has been created
        var createRequest = new UserCreateRequest
        {
            Name = "Get User Test",
            Email = "getuser@example.com",
            Password = "password123"
        };
        var createdResult = Controller.CreateUser(createRequest);
        var createdUser = ((CreatedAtActionResult)createdResult.Result!).Value as UserDto;

        // When
        var result = Controller.GetUserById(createdUser!.Id);

        // Then
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedUser = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(createdUser.Id, returnedUser.Id);
    }

    [Fact]
    public void GetAllUsersEndpoint_GivenMultipleUsersCreated_WhenCalled_ThenReturnsAllUsers()
    {
        // Given: Multiple users are created
        var user1Request = new UserCreateRequest { Name = "User 1", Email = "user1@example.com", Password = "password123" };
        var user2Request = new UserCreateRequest { Name = "User 2", Email = "user2@example.com", Password = "password123" };
        Controller.CreateUser(user1Request);
        Controller.CreateUser(user2Request);

        // When
        var result = Controller.GetAllUsers();

        // Then
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var users = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value).ToList();
        Assert.True(users.Count >= 2);
    }

    [Fact]
    public void UpdateUserEndpoint_GivenCreatedUser_WhenUpdated_ThenReturnsUpdatedData()
    {
        // Given: A user has been created
        var createRequest = new UserCreateRequest
        {
            Name = "Original Name",
            Email = "original@example.com",
            Password = "password123"
        };
        var createdResult = Controller.CreateUser(createRequest);
        var createdUser = ((CreatedAtActionResult)createdResult.Result!).Value as UserDto;

        // When: User is updated
        var updateRequest = new UserUpdateRequest
        {
            Name = "Updated Name",
            Email = "updated@example.com"
        };
        var result = Controller.UpdateUser(createdUser!.Id, updateRequest);

        // Then
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedUser = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("Updated Name", returnedUser.Name);
    }

    [Fact]
    public void DeleteUserEndpoint_GivenCreatedUser_WhenDeleted_ThenReturnsNoContent()
    {
        // Given: A user has been created
        var createRequest = new UserCreateRequest
        {
            Name = "User to Delete",
            Email = "delete@example.com",
            Password = "password123"
        };
        var createdResult = Controller.CreateUser(createRequest);
        var createdUser = ((CreatedAtActionResult)createdResult.Result!).Value as UserDto;

        // When: User is deleted
        var result = Controller.DeleteUser(createdUser!.Id);

        // Then
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public void GetDeletedUserEndpoint_GivenDeletedUser_WhenCalled_ThenReturnsNotFound()
    {
        // Given: A user is created and then deleted
        var createRequest = new UserCreateRequest
        {
            Name = "Temporary User",
            Email = "temp@example.com",
            Password = "password123"
        };
        var createdResult = Controller.CreateUser(createRequest);
        var createdUser = ((CreatedAtActionResult)createdResult.Result!).Value as UserDto;
        Controller.DeleteUser(createdUser!.Id);

        // When: Attempting to get the deleted user
        var result = Controller.GetUserById(createdUser.Id);

        // Then
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public void CreateUserWithInvalidEmail_GivenInvalidData_WhenCalled_ThenReturnsBadRequest()
    {
        // Given: Invalid user creation request (empty email)
        var request = new UserCreateRequest
        {
            Name = "Invalid User",
            Email = "",
            Password = "password123"
        };

        // When
        var result = Controller.CreateUser(request);

        // Then
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public void CreateDuplicateUser_GivenExistingEmail_WhenCreating_ThenReturnsConflict()
    {
        // Given: First user is created
        var request1 = new UserCreateRequest
        {
            Name = "User One",
            Email = "duplicate@example.com",
            Password = "password123"
        };
        Controller.CreateUser(request1);

        // When: Creating user with same email
        var request2 = new UserCreateRequest
        {
            Name = "User Two",
            Email = "duplicate@example.com",
            Password = "password456"
        };
        var result = Controller.CreateUser(request2);

        // Then
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    [Fact]
    public void GetNonExistentUser_GivenInvalidId_WhenCalled_ThenReturnsNotFound()
    {
        // Given: No user with ID 999

        // When
        var result = Controller.GetUserById(999);

        // Then
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public void FullUserWorkflow_GivenCompleteUserLifecycle_WhenPerformed_ThenAllOperationsSucceed()
    {
        // Given: No users exist initially
        var allUsersResult1 = Controller.GetAllUsers();
        var okResult1 = Assert.IsType<OkObjectResult>(allUsersResult1.Result);
        var initialUsers = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult1.Value).ToList();
        var initialCount = initialUsers.Count;

        // When: Create a user
        var createRequest = new UserCreateRequest
        {
            Name = "Workflow User",
            Email = "workflow@example.com",
            Password = "password123"
        };
        var createResult = Controller.CreateUser(createRequest);
        var createdUser = ((CreatedAtActionResult)createResult.Result!).Value as UserDto;

        // And: Get the user
        var getResult = Controller.GetUserById(createdUser!.Id);
        var getUserDto = ((OkObjectResult)getResult.Result!).Value as UserDto;

        // And: Update the user
        var updateRequest = new UserUpdateRequest
        {
            Name = "Updated Workflow User",
            Email = "updated-workflow@example.com"
        };
        var updateResult = Controller.UpdateUser(createdUser.Id, updateRequest);
        var updatedUser = ((OkObjectResult)updateResult.Result!).Value as UserDto;

        // And: Get all users
        var allUsersResult2 = Controller.GetAllUsers();
        var okResult2 = Assert.IsType<OkObjectResult>(allUsersResult2.Result);
        var allUsersAfterCreate = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult2.Value).ToList();

        // And: Delete the user
        Controller.DeleteUser(createdUser.Id);

        // And: Verify user is deleted
        var getDeletedResult = Controller.GetUserById(createdUser.Id);
        var deletedNotFound = Assert.IsType<NotFoundObjectResult>(getDeletedResult.Result);

        // Then: Verify all operations succeeded
        Assert.NotNull(createdUser);
        Assert.NotNull(getUserDto);
        Assert.Equal("Updated Workflow User", updatedUser!.Name);
        Assert.Equal(initialCount + 1, allUsersAfterCreate.Count);
        Assert.Equal(404, deletedNotFound.StatusCode);
    }
}
