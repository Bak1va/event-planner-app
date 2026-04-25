using Backend.DTOs;
using Backend.Services;
using Xunit;

namespace Backend.Tests;

public class UserServiceTests
{
    private static UserService CreateService() =>
        new UserService(new ValidationService());

    // -------------------------------------------------------------------------
    // Registration – duplicate email prevention
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateUser_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var service = CreateService();
        var request = new UserCreateRequest
        {
            Name = "Alice",
            Email = "alice@example.com",
            Password = "secret123"
        };

        service.CreateUser(request);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.CreateUser(new UserCreateRequest
            {
                Name = "Alice Again",
                Email = "alice@example.com",
                Password = "another123"
            }));

        Assert.Equal("A user with this email already exists.", ex.Message);
    }

    [Fact]
    public void CreateUser_WithDuplicateEmailDifferentCase_ThrowsInvalidOperationException()
    {
        var service = CreateService();
        service.CreateUser(new UserCreateRequest
        {
            Name = "Bob",
            Email = "bob@example.com",
            Password = "password1"
        });

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateUser(new UserCreateRequest
            {
                Name = "Bob Upper",
                Email = "BOB@EXAMPLE.COM",
                Password = "password2"
            }));
    }

    [Fact]
    public void CreateUser_WithUniqueEmails_CreatesBothUsers()
    {
        var service = CreateService();

        var user1 = service.CreateUser(new UserCreateRequest
        {
            Name = "Alice",
            Email = "alice@example.com",
            Password = "pass1234"
        });
        var user2 = service.CreateUser(new UserCreateRequest
        {
            Name = "Bob",
            Email = "bob@example.com",
            Password = "pass5678"
        });

        Assert.NotEqual(user1.Id, user2.Id);
        Assert.Equal(2, service.GetAllUsers().Count());
    }

    // -------------------------------------------------------------------------
    // Login flow
    // -------------------------------------------------------------------------

    [Fact]
    public void Login_WithCorrectCredentials_ReturnsUserDtoWithoutPassword()
    {
        var service = CreateService();
        var created = service.CreateUser(new UserCreateRequest
        {
            Name = "Carol",
            Email = "carol@example.com",
            Password = "mypassword"
        });

        var result = service.Login(new LoginRequest
        {
            Email = "carol@example.com",
            Password = "mypassword"
        });

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Carol", result.Name);
        Assert.Equal("carol@example.com", result.Email);
        Assert.True(result.DateAdded > DateTime.MinValue);
        Assert.True(result.DateModified > DateTime.MinValue);
        // UserDto must not expose a password field
        Assert.False(
            typeof(UserDto).GetProperties().Any(p => p.Name.Contains("Password", StringComparison.OrdinalIgnoreCase)),
            "UserDto must not contain a Password property.");
    }

    [Fact]
    public void Login_WithWrongPassword_ReturnsNull()
    {
        var service = CreateService();
        service.CreateUser(new UserCreateRequest
        {
            Name = "Dave",
            Email = "dave@example.com",
            Password = "correct123"
        });

        var result = service.Login(new LoginRequest
        {
            Email = "dave@example.com",
            Password = "wrong_password"
        });

        Assert.Null(result);
    }

    [Fact]
    public void Login_WithUnknownEmail_ReturnsNull()
    {
        var service = CreateService();

        var result = service.Login(new LoginRequest
        {
            Email = "nobody@example.com",
            Password = "doesntmatter"
        });

        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // EmailExists helper
    // -------------------------------------------------------------------------

    [Fact]
    public void EmailExists_AfterRegistration_ReturnsTrue()
    {
        var service = CreateService();
        service.CreateUser(new UserCreateRequest
        {
            Name = "Eve",
            Email = "eve@example.com",
            Password = "pass1234"
        });

        Assert.True(service.EmailExists("eve@example.com"));
        Assert.True(service.EmailExists("EVE@EXAMPLE.COM"));
    }

    [Fact]
    public void EmailExists_ForUnregisteredEmail_ReturnsFalse()
    {
        var service = CreateService();

        Assert.False(service.EmailExists("ghost@example.com"));
    }
}
