using Backend.Controllers;
using Backend.Services;

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

    public UsersControllerIntegrationTestBase()
    {
        ValidationService = new ValidationService();
        EventService = new EventService(ValidationService);
        UserService = new UserService(ValidationService);
        
        // Wire up services to resolve circular dependency
        UserService.SetEventService(EventService);
        EventService.SetUserService(UserService);
        
        Controller = new UsersController(UserService);
    }
}

