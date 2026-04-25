using Backend.DTOs;
using Xunit;

namespace Backend.Tests.Services.Integration;

/// <summary>
/// Integration tests for UserService with real ValidationService
/// Given: Real service dependencies
/// When: Operations are performed
/// Then: Business logic works correctly end-to-end
/// </summary>
public class UserServiceIntegrationTests : UserServiceIntegrationTestBase
{
    [Fact]
    public void CreateAndRetrieveUser_GivenValidUserData_WhenCreatedAndRetrieved_ThenDataIsPreserved()
    {
        // Given: Valid user creation request
        var request = new UserCreateRequest 
        { 
            Name = "Integration Test User", 
            Email = "integration@example.com" 
        };

        // When
        var createdUser = UserService.CreateUser(request);
        var retrievedUser = UserService.GetUserById(createdUser.Id);

        // Then
        Assert.NotNull(retrievedUser);
        Assert.Equal("Integration Test User", retrievedUser.Name);
        Assert.Equal("integration@example.com", retrievedUser.Email);
    }

    [Fact]
    public void CreateUpdateAndRetrieveUser_GivenUserOperations_WhenPerformed_ThenAllChangesArePersisted()
    {
        // Given: A user is created
        var createRequest = new UserCreateRequest 
        { 
            Name = "Original Name", 
            Email = "original@example.com" 
        };
        var createdUser = UserService.CreateUser(createRequest);

        // When: User is updated
        var updateRequest = new UserUpdateRequest 
        { 
            Name = "Updated Name", 
            Email = "updated@example.com" 
        };
        var updatedUser = UserService.UpdateUser(createdUser.Id, updateRequest);

        // And: User is retrieved
        var retrievedUser = UserService.GetUserById(createdUser.Id);

        // Then
        Assert.Equal("Updated Name", updatedUser.Name);
        Assert.Equal("updated@example.com", updatedUser.Email);
        Assert.Equal(updatedUser.Name, retrievedUser!.Name);
        Assert.Equal(updatedUser.Email, retrievedUser.Email);
    }

    [Fact]
    public void CreateAndDeleteUser_GivenCreatedUser_WhenDeleted_ThenCannotBeRetrieved()
    {
        // Given: A user is created
        var request = new UserCreateRequest 
        { 
            Name = "User to Delete", 
            Email = "delete@example.com" 
        };
        var createdUser = UserService.CreateUser(request);

        // When: User is deleted
        UserService.DeleteUser(createdUser.Id);

        // Then: User cannot be retrieved
        var deletedUser = UserService.GetUserById(createdUser.Id);
        Assert.Null(deletedUser);
    }

    [Fact]
    public void ValidateUserCreation_GivenNameTooLong_WhenCreating_ThenThrowsArgumentException()
    {
        // Given: User request with name exceeding 100 characters
        var request = new UserCreateRequest 
        { 
            Name = new string('A', 101),
            Email = "test@example.com" 
        };

        // When & Then
        var exception = Assert.Throws<ArgumentException>(() => UserService.CreateUser(request));
        Assert.Contains("Name cannot exceed 100 characters", exception.Message);
    }

    [Fact]
    public void ValidateUserCreation_GivenEmailTooLong_WhenCreating_ThenThrowsArgumentException()
    {
        // Given: User request with email exceeding 150 characters
        var request = new UserCreateRequest 
        { 
            Name = "Valid Name",
            Email = new string('a', 140) + "@example.com" // > 150 chars
        };

        // When & Then
        var exception = Assert.Throws<ArgumentException>(() => UserService.CreateUser(request));
        Assert.Contains("Email cannot exceed 150 characters", exception.Message);
    }

    [Fact]
    public void CreateMultipleUsersAndListAll_GivenMultipleUsers_WhenListed_ThenAllAreReturned()
    {
        // Given: Multiple users are created
        var users = new List<UserDto>();
        for (int i = 1; i <= 5; i++)
        {
            var request = new UserCreateRequest 
            { 
                Name = $"User {i}", 
                Email = $"user{i}@example.com" 
            };
            users.Add(UserService.CreateUser(request));
        }

        // When
        var allUsers = UserService.GetAllUsers().ToList();

        // Then
        Assert.Equal(5, allUsers.Count);
        Assert.All(users, u => Assert.Single(allUsers, a => a.Id == u.Id));
    }

    [Fact]
    public void CreateUserWithWhitespace_GivenRequestWithWhitespace_WhenCreated_ThenWhitespaceIsTrimmed()
    {
        // Given: User request with leading/trailing whitespace
        var request = new UserCreateRequest 
        { 
            Name = "  Padded Name  ", 
            Email = "  padded@example.com  " 
        };

        // When
        var createdUser = UserService.CreateUser(request);

        // Then
        Assert.Equal("Padded Name", createdUser.Name);
        Assert.Equal("padded@example.com", createdUser.Email);
    }

    [Fact]
    public void DuplicateEmailValidation_GivenTwoUsersWithSameEmail_WhenSecondCreated_ThenThrowsException()
    {
        // Given: First user is created
        var request1 = new UserCreateRequest 
        { 
            Name = "User One", 
            Email = "duplicate@example.com" 
        };
        UserService.CreateUser(request1);

        // When & Then: Creating user with same email fails
        var request2 = new UserCreateRequest 
        { 
            Name = "User Two", 
            Email = "duplicate@example.com" 
        };
        var exception = Assert.Throws<InvalidOperationException>(() => UserService.CreateUser(request2));
        Assert.Contains("A user with this email already exists", exception.Message);
    }

    [Fact]
    public void CaseInsensitiveEmailValidation_GivenUserWithMixedCaseEmail_WhenDuplicateCreated_ThenThrowsException()
    {
        // Given: User is created with specific email case
        var request1 = new UserCreateRequest 
        { 
            Name = "User One", 
            Email = "Test@Example.Com" 
        };
        UserService.CreateUser(request1);

        // When & Then: Creating user with different case of same email fails
        var request2 = new UserCreateRequest 
        { 
            Name = "User Two", 
            Email = "test@example.com" 
        };
        var exception = Assert.Throws<InvalidOperationException>(() => UserService.CreateUser(request2));
        Assert.Contains("A user with this email already exists", exception.Message);
    }
}

