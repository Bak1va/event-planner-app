using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers.Unit;

/// <summary>
/// BDD-style unit tests for UsersController.DeleteUser
/// Given: User exists or doesn't exist
/// When: DeleteUser endpoint is called
/// Then: Appropriate HTTP response is returned
/// </summary>
public class UsersControllerDeleteUserTests : UsersControllerUnitTestBase
{
    [Fact]
    public void DeleteUser_GivenUserExists_WhenCalled_ThenReturnsNoContent()
    {
        // Given: User exists
        MockUserService.Setup(s => s.DeleteUser(1))
            .Returns(true);

        // When
        var result = Controller.DeleteUser(1);

        // Then
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public void DeleteUser_GivenUserNotFound_WhenCalled_ThenReturnsNotFound()
    {
        // Given: User does not exist
        MockUserService.Setup(s => s.DeleteUser(999))
            .Throws(new KeyNotFoundException("User with id 999 was not found."));

        // When
        var result = Controller.DeleteUser(999);

        // Then
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public void DeleteUser_GivenUserWithEvents_WhenCalled_ThenReturnsConflict()
    {
        // Given: User has related events
        MockUserService.Setup(s => s.DeleteUser(1))
            .Throws(new InvalidOperationException("Cannot delete user that has events."));

        // When
        var result = Controller.DeleteUser(1);

        // Then
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    [Fact]
    public void DeleteUser_GivenValidUser_WhenCalled_ThenCallsServiceOnce()
    {
        // Given: User exists
        MockUserService.Setup(s => s.DeleteUser(1))
            .Returns(true);

        // When
        Controller.DeleteUser(1);

        // Then
        MockUserService.Verify(s => s.DeleteUser(1), Times.Once);
    }
}


