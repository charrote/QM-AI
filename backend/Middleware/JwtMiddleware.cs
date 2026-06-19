using System.Security.Claims;

namespace QM_AI.API.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, Services.AuthService authService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var principal = authService.ValidateToken(token);
            if (principal != null)
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                                 ?? principal.FindFirst("sub");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await authService.GetUserById(userId);
                    if (user != null)
                    {
                        context.Items["User"] = user;
                        context.Items["UserId"] = userId;
                    }
                }
            }
        }

        await _next(context);
    }
}
