using Backend.Controllers;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers.Unit;

/// <summary>
/// Base class for controller unit tests with mocked services
/// </summary>
public class UsersControllerUnitTestBase
{
    protected readonly Mock<IUserService> MockUserService;
    protected readonly UsersController Controller;

    public UsersControllerUnitTestBase()
    {
        MockUserService = new Mock<IUserService>();
        Controller = new UsersController(MockUserService.Object);
    }
}

