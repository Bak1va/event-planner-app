using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public ActionResult<IEnumerable<UserDto>> GetAllUsers()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var currentUser = _userService.GetAllUsers().FirstOrDefault(user => user.Id == currentUserId.Value);
        if (currentUser is null)
        {
            return NotFound(new { message = $"User with id {currentUserId.Value} was not found." });
        }

        return Ok(new[] { currentUser });
    }

    /// <summary>
    /// Check if an email address is already registered
    /// </summary>
    [HttpGet("email/{email}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public ActionResult<bool> CheckEmailExists(string email)
    {
        var currentUserEmail = GetCurrentUserEmail();
        if (currentUserEmail is null)
        {
            return Unauthorized();
        }

        if (!email.Equals(currentUserEmail, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        return Ok(_userService.EmailExists(email));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public ActionResult<UserDto> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var user = _userService.Login(request);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(user);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public ActionResult<UserDto> GetUserById(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (currentUserId.Value != id)
        {
            return Forbid();
        }

        var user = _userService.GetUserById(id);
        if (user == null)
        {
            return NotFound(new { message = $"User with id {id} was not found." });
        }
        return Ok(user);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public ActionResult<UserDto> CreateUser(UserCreateRequest request)
    {
        try
        {
            var user = _userService.CreateUser(request);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
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

    /// <summary>
    /// Update user by ID
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(403)]
    public ActionResult<UserDto> UpdateUser(int id, UserUpdateRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (currentUserId.Value != id)
        {
            return Forbid();
        }

        try
        {
            var user = _userService.UpdateUser(id, request);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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

    /// <summary>
    /// Delete user by ID
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(403)]
    public ActionResult DeleteUser(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (currentUserId.Value != id)
        {
            return Forbid();
        }

        try
        {
            _userService.DeleteUser(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private int? GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idValue, out var currentUserId))
        {
            return null;
        }

        return currentUserId;
    }

    private string? GetCurrentUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email);
    }
}

