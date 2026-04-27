using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public interface IAuthService
{
    AuthResponse SignUp(SignUpRequest request);
    AuthResponse? Login(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthService(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    public AuthResponse SignUp(SignUpRequest request)
    {
        var user = _userService.SignUp(request);
        return new AuthResponse
        {
            Token = CreateToken(user),
            User = user
        };
    }

    public AuthResponse? Login(LoginRequest request)
    {
        var user = _userService.Login(request);
        if (user is null)
        {
            return null;
        }

        return new AuthResponse
        {
            Token = CreateToken(user),
            User = user
        };
    }

    private string CreateToken(UserDto user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? "EventPlannerApp";
        var audience = jwtSection["Audience"] ?? "EventPlannerApp";
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}