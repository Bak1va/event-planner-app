using Backend.DTOs;
using Backend.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Backend.Tests.Services;

public class AuthServiceTests
{
    private static AuthService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "EventPlannerApp",
                ["Jwt:Audience"] = "EventPlannerApp",
                ["Jwt:Key"] = "change-this-development-key-to-a-long-random-secret"
            })
            .Build();

        return new AuthService(new UserService(new ValidationService()), configuration);
    }

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

        Assert.NotEmpty(result.Token);
        Assert.Equal("Ava", result.User.FirstName);
        Assert.Equal("Stone", result.User.LastName);
        Assert.Equal("+40 700 123 456", result.User.PhoneNumber);
        Assert.Equal("Ava Stone", result.User.Name);
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
        Assert.NotEmpty(result!.Token);
        Assert.Equal("Ben", result.User.FirstName);
        Assert.Equal("Hart", result.User.LastName);
        Assert.Equal("Ben Hart", result.User.Name);
    }
}