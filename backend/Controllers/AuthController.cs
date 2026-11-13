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
    public IActionResult Logout()
    {
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
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult GetCurrentUser()
    {
        // JwtMiddleware 现在存的是 User 模型
        var user = HttpContext.Items["User"] as QM_AI.API.Models.User;

        if (user == null)
            return Unauthorized(new { message = "Not authenticated" });

        var permissions = HttpContext.Items["Permissions"] as HashSet<string> ?? new HashSet<string>();

        var dto = new QM_AI.API.DTOs.Auth.UserInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Avatar = user.Avatar,
            Email = user.Email,
            Role = user.Role?.Name,
            DataScope = user.Role?.DataScope,
            Permissions = permissions.ToList(),
            CreatedAt = user.CreatedAt,
        };

        return Ok(dto);
    }
}
