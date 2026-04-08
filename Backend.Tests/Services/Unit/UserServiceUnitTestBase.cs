using Backend.DTOs;
using Backend.Services;
using Moq;
using Xunit;

namespace Backend.Tests.Services.Unit;

/// <summary>
/// Base class for unit tests with common setup
/// </summary>
public class UserServiceUnitTestBase
{
    protected readonly Mock<IEventService> MockEventService;
    protected readonly Mock<IValidationService> MockValidationService;
    protected readonly UserService UserService;

    public UserServiceUnitTestBase()
    {
        MockEventService = new Mock<IEventService>();
        MockValidationService = new Mock<IValidationService>();
        UserService = new UserService(MockValidationService.Object);
        
        // Wire up the mock event service
        UserService.SetEventService(MockEventService.Object);
    }

    protected void SetupValidUserValidation()
    {
        MockValidationService.Setup(v => v.ValidateUserRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string?)null);
    }

    protected void SetupValidationError(string errorMessage)
    {
        MockValidationService.Setup(v => v.ValidateUserRequest(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(errorMessage);
    }

    protected void SetupEventServiceToReturnNoEvents()
    {
        MockEventService.Setup(e => e.GetEventsByUserId(It.IsAny<int>()))
            .Returns(Enumerable.Empty<EventDto>());
    }
}

