using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs.Auth;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.Login(request);

        if (result == null)
            return Unauthorized(new { message = "Invalid username or password" });

        return Ok(result);
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] RefreshTokenRequest request)
    {
        _authService.Logout(request.RefreshToken);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshToken(request);

        if (result == null)
            return Unauthorized(new { message = "Invalid or expired refresh token" });

        return Ok(result);
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var user = HttpContext.Items["User"] as UserInfoDto;

        if (user == null)
            return Unauthorized(new { message = "Not authenticated" });

        return Ok(user);
    }
}
