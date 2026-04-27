using Backend.Controllers;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Backend.Tests.Controllers.Unit;

/// <summary>
/// Base class for controller unit tests with mocked services
/// </summary>
public class UsersControllerUnitTestBase
{
    protected readonly Mock<IUserService> MockUserService;
    protected readonly UsersController Controller;
    protected readonly UserDto CurrentUser;

    public UsersControllerUnitTestBase()
    {
        MockUserService = new Mock<IUserService>();
        CurrentUser = new UserDto
        {
            Id = 1,
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "555-0000"
        };
        MockUserService.Setup(s => s.GetUserById(1)).Returns(CurrentUser);
        Controller = new UsersController(MockUserService.Object);
        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Email, "john@example.com")
                }, "TestAuth"))
            }
        };
    }
}

