using Backend.DTOs;

namespace Backend.Services;

public interface IAuthService
{
    UserDto SignUp(SignUpRequest request);
    UserDto? Login(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly IUserService _userService;

    public AuthService(IUserService userService)
    {
        _userService = userService;
    }

    public UserDto SignUp(SignUpRequest request)
    {
        return _userService.SignUp(request);
    }

    public UserDto? Login(LoginRequest request)
    {
        return _userService.Login(request);
    }
}