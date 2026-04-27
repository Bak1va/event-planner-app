using Backend.Controllers;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers.Unit;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_authService.Object);
    }

    [Fact]
    public void SignUp_WithValidRequest_ReturnsCreatedResponse()
    {
        var request = new SignUpRequest
        {
            FirstName = "Mia",
            LastName = "Cole",
            Email = "mia@example.com",
            Password = "password123",
            PhoneNumber = "5551234"
        };
        var createdUser = new UserDto { Id = 1, FirstName = "Mia", LastName = "Cole", PhoneNumber = "5551234", Email = "mia@example.com" };
        _authService.Setup(service => service.SignUp(request)).Returns(createdUser);

        var result = _controller.SignUp(request);

        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.IsType<UserDto>(createdResult.Value);
    }

    [Fact]
    public void Login_WithMissingCredentials_ReturnsBadRequest()
    {
        var result = _controller.Login(new LoginRequest { Email = string.Empty, Password = string.Empty });

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsOk()
    {
        var request = new LoginRequest { Email = "mia@example.com", Password = "password123" };
        var user = new UserDto { Id = 1, FirstName = "Mia", LastName = "Cole", PhoneNumber = "5551234", Email = "mia@example.com" };
        _authService.Setup(service => service.Login(request)).Returns(user);

        var result = _controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.IsType<UserDto>(okResult.Value);
    }
}