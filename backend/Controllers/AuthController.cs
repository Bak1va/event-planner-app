using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public ActionResult<AuthResponse> SignUp(SignUpRequest request)
    {
        try
        {
            var response = _authService.SignUp(request);
            return Created($"/api/users/{response.User.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public ActionResult<AuthResponse> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var response = _authService.Login(request);
        if (response is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(response);
    }
}