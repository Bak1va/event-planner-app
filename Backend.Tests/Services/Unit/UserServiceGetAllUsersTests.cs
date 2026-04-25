using Backend.DTOs;
using Xunit;

namespace Backend.Tests.Services.Unit;

/// <summary>
/// BDD-style unit tests for UserService.GetAllUsers
/// Given: Various user store states
/// When: GetAllUsers is called
/// Then: Returns appropriate user lists
/// </summary>
public class UserServiceGetAllUsersTests : UserServiceUnitTestBase
{
    [Fact]
    public void GetAllUsers_GivenNoUsersExist_WhenCalled_ThenReturnsEmptyList()
    {
        // Given: No users exist in the store

        // When
        var result = UserService.GetAllUsers();

        // Then
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllUsers_GivenMultipleUsersExist_WhenCalled_ThenReturnsAllUsers()
    {
        // Given: Multiple users exist in the store
        SetupValidUserValidation();
        var user1 = UserService.CreateUser(new UserCreateRequest { Name = "John Doe", Email = "john@example.com" });
        var user2 = UserService.CreateUser(new UserCreateRequest { Name = "Jane Smith", Email = "jane@example.com" });

        // When
        var result = UserService.GetAllUsers().ToList();

        // Then
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == user1.Id && u.Name == "John Doe");
        Assert.Contains(result, u => u.Id == user2.Id && u.Name == "Jane Smith");
    }

    [Fact]
    public void GetAllUsers_GivenMultipleUsersExist_WhenCalled_ThenReturnsUsersOrderedById()
    {
        // Given: Multiple users in different order
        SetupValidUserValidation();
        UserService.CreateUser(new UserCreateRequest { Name = "User A", Email = "a@example.com" });
        UserService.CreateUser(new UserCreateRequest { Name = "User B", Email = "b@example.com" });
        UserService.CreateUser(new UserCreateRequest { Name = "User C", Email = "c@example.com" });

        // When
        var result = UserService.GetAllUsers().ToList();

        // Then
        Assert.Equal(3, result.Count);
        Assert.True(result[0].Id < result[1].Id && result[1].Id < result[2].Id, 
            "Users should be ordered by ID");
    }

    [Fact]
    public void GetAllUsers_GivenThreeUsersExist_WhenCalled_ThenReturnsAllThreeUsers()
    {
        // Given: Three users have been created
        SetupValidUserValidation();
        var createdIds = new List<int>();
        for (int i = 1; i <= 3; i++)
        {
            var user = UserService.CreateUser(new UserCreateRequest 
            { 
                Name = $"User {i}", 
                Email = $"user{i}@example.com" 
            });
            createdIds.Add(user.Id);
        }

        // When
        var result = UserService.GetAllUsers().ToList();

        // Then
        Assert.Equal(3, result.Count);
        Assert.All(createdIds, id => Assert.Single(result, u => u.Id == id));
    }
}

