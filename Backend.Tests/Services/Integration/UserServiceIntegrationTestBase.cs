using Backend.DTOs;
using Backend.Services;
using Xunit;

namespace Backend.Tests.Services.Integration;

/// <summary>
/// Base class for integration tests with shared setup
/// Integration tests use real implementations without mocks
/// </summary>
public class UserServiceIntegrationTestBase
{
    protected readonly ValidationService ValidationService;
    protected readonly EventService EventService;
    protected readonly UserService UserService;

    public UserServiceIntegrationTestBase()
    {
        ValidationService = new ValidationService();
        EventService = new EventService(ValidationService);
        UserService = new UserService(ValidationService);
        
        // Wire up services to resolve circular dependency
        UserService.SetEventService(EventService);
        EventService.SetUserService(UserService);
    }
}

