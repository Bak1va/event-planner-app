using Backend.DTOs;
using Xunit;

namespace Backend.Tests.Services.Unit;

/// <summary>
/// BDD-style unit tests for UserService.CreateUser
/// Given: User data with various validation states
/// When: CreateUser is called
/// Then: User is created or exception is thrown
/// </summary>
public class UserServiceCreateUserTests : UserServiceUnitTestBase
{
    [Fact]
    public void CreateUser_GivenValidData_WhenCalled_ThenCreatesUserSuccessfully()
    {
        // Given: Valid user data
        SetupValidUserValidation();
        var request = new UserCreateRequest 
        { 
            Name = "Jane Smith", 
            Email = "jane@example.com" 
        };

        // When
        var result = UserService.CreateUser(request);

        // Then
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Jane Smith", result.Name);
        Assert.Equal("jane@example.com", result.Email);
        Assert.NotEqual(default(DateTime), result.DateAdded);
    }

    [Fact]
    public void CreateUser_GivenInvalidEmail_WhenCalled_ThenThrowsArgumentException()
    {
        // Given: Invalid email validation error from service
        const string validationError = "Email is required.";
        SetupValidationError(validationError);
        var request = new UserCreateRequest 
        { 
            Name = "Jane Smith", 
            Email = "" 
        };

        // When & Then
        var exception = Assert.Throws<ArgumentException>(() => UserService.CreateUser(request));
        Assert.Contains("Email is required", exception.Message);
    }

    [Fact]
    public void CreateUser_GivenDuplicateEmail_WhenCalled_ThenThrowsInvalidOperationException()
    {
        // Given: Two users with the same email
        SetupValidUserValidation();
        var request1 = new UserCreateRequest 
        { 
            Name = "User One", 
            Email = "duplicate@example.com" 
        };
        var request2 = new UserCreateRequest 
        { 
            Name = "User Two", 
            Email = "duplicate@example.com" 
        };
        UserService.CreateUser(request1);

        // When & Then
        var exception = Assert.Throws<InvalidOperationException>(() => UserService.CreateUser(request2));
        Assert.Contains("A user with this email already exists", exception.Message);
    }

    [Fact]
    public void CreateUser_GivenWhitespaceInData_WhenCalled_ThenTrimsWhitespace()
    {
        // Given: User data with leading and trailing whitespace
        SetupValidUserValidation();
        var request = new UserCreateRequest 
        { 
            Name = "  John Doe  ", 
            Email = "  john@example.com  " 
        };

        // When
        var result = UserService.CreateUser(request);

        // Then
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public void CreateUser_GivenMultipleValidUsers_WhenCreated_ThenEachHasUniqueId()
    {
        // Given: Multiple valid user requests
        SetupValidUserValidation();
        var request1 = new UserCreateRequest { Name = "User 1", Email = "user1@example.com" };
        var request2 = new UserCreateRequest { Name = "User 2", Email = "user2@example.com" };
        var request3 = new UserCreateRequest { Name = "User 3", Email = "user3@example.com" };

        // When
        var user1 = UserService.CreateUser(request1);
        var user2 = UserService.CreateUser(request2);
        var user3 = UserService.CreateUser(request3);

        // Then
        Assert.NotEqual(user1.Id, user2.Id);
        Assert.NotEqual(user2.Id, user3.Id);
        Assert.NotEqual(user1.Id, user3.Id);
    }

    [Fact]
    public void CreateUser_GivenValidNameButInvalidEmail_WhenCalled_ThenThrowsArgumentException()
    {
        // Given: Valid name but invalid email
        SetupValidationError("Email is required.");
        var request = new UserCreateRequest 
        { 
            Name = "Valid Name", 
            Email = "" 
        };

        // When & Then
        var exception = Assert.Throws<ArgumentException>(() => UserService.CreateUser(request));
        Assert.Contains("Email is required", exception.Message);
    }

    [Fact]
    public void CreateUser_GivenCaseInsensitiveDuplicateEmail_WhenCalled_ThenThrowsInvalidOperationException()
    {
        // Given: Two users with same email but different case
        SetupValidUserValidation();
        UserService.CreateUser(new UserCreateRequest { Name = "User One", Email = "Test@Example.com" });

        // When & Then
        var exception = Assert.Throws<InvalidOperationException>(
            () => UserService.CreateUser(new UserCreateRequest { Name = "User Two", Email = "test@example.com" }));
        Assert.Contains("A user with this email already exists", exception.Message);
    }
}

