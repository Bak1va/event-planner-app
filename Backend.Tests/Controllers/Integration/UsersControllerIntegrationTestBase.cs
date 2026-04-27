using Backend.Controllers;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Tests.Controllers.Integration;

/// <summary>
/// Base class for controller integration tests
/// Integration tests use real service implementations
/// </summary>
public class UsersControllerIntegrationTestBase
{
    protected readonly ValidationService ValidationService;
    protected readonly EventService EventService;
    protected readonly UserService UserService;
    protected readonly UsersController Controller;
    protected readonly UserDto CurrentUser;

    public UsersControllerIntegrationTestBase()
    {
        ValidationService = new ValidationService();
        EventService = new EventService(ValidationService);
        UserService = new UserService(ValidationService);
        
        // Wire up services to resolve circular dependency
        UserService.SetEventService(EventService);
        EventService.SetUserService(UserService);

        CurrentUser = UserService.SignUp(new SignUpRequest
        {
            FirstName = "Integration",
            LastName = "User",
            Email = "integration-user@example.com",
            Password = "password123",
            PhoneNumber = "555-0100"
        });
        
        Controller = new UsersController(UserService);
        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, CurrentUser.Id.ToString()),
                    new Claim(ClaimTypes.Email, CurrentUser.Email)
                }, "TestAuth"))
            }
        };
    }
}

