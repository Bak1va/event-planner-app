using Backend.DTOs;
using Backend.Services;
using Xunit;

namespace Backend.Tests.Services;

public class AuthServiceTests
{
    private static AuthService CreateService() =>
        new AuthService(new UserService(new ValidationService()));

    [Fact]
    public void SignUp_WithValidData_CreatesUserWithProfileFields()
    {
        var service = CreateService();

        var result = service.SignUp(new SignUpRequest
        {
            FirstName = "Ava",
            LastName = "Stone",
            Email = "ava@example.com",
            Password = "secret123",
            PhoneNumber = "+40 700 123 456"
        });

        Assert.Equal("Ava", result.FirstName);
        Assert.Equal("Stone", result.LastName);
        Assert.Equal("+40 700 123 456", result.PhoneNumber);
        Assert.Equal("Ava Stone", result.Name);
    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsMatchingUser()
    {
        var service = CreateService();
        service.SignUp(new SignUpRequest
        {
            FirstName = "Ben",
            LastName = "Hart",
            Email = "ben@example.com",
            Password = "password123",
            PhoneNumber = "123456789"
        });

        var result = service.Login(new LoginRequest
        {
            Email = "ben@example.com",
            Password = "password123"
        });

        Assert.NotNull(result);
        Assert.Equal("Ben", result!.FirstName);
        Assert.Equal("Hart", result.LastName);
        Assert.Equal("Ben Hart", result.Name);
    }
}