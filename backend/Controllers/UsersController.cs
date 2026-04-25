using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        return Ok(_userService.GetAllUsers());
    }

    /// <summary>
    /// Check if an email address is already registered
    /// </summary>
    [HttpGet("email/{email}")]
    [ProducesResponseType(200)]
    public ActionResult<bool> CheckEmailExists(string email)
    {
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
    public ActionResult<UserDto> GetUserById(int id)
    {
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
    public ActionResult<UserDto> UpdateUser(int id, UserUpdateRequest request)
    {
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
    public ActionResult DeleteUser(int id)
    {
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
}

