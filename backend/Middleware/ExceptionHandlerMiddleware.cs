using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Helpers;

namespace QM_AI.API.Middleware;

/// <summary>
/// Global exception handler middleware that maps exception types to proper HTTP status codes
/// and returns consistent ProblemDetails responses.
/// </summary>
public class AppExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly ILogger<AppExceptionHandlerMiddleware> _logger;

    public AppExceptionHandlerMiddleware(
        RequestDelegate next,
        IHostEnvironment env,
        ILogger<AppExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _env = env;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(
            exception,
            "[{TraceId}] {Method} {Path} — {ExceptionType}: {Message}",
            context.TraceIdentifier,
            context.Request.Method,
            context.Request.Path,
            exception.GetType().Name,
            exception.Message);

        // Add correlation ID header for debugging
        context.Response.Headers["X-Request-Id"] = context.TraceIdentifier;

        (string title, int statusCode, string? detail) result = exception switch
        {
            // Validation / business rule errors → 400
            InvalidOperationException => ("Invalid operation", 400,
                _env.IsDevelopment() ? exception.Message : null),
            ArgumentException => ("Bad request", 400,
                _env.IsDevelopment() ? exception.Message : null),

            // Missing resources → 404
            KeyNotFoundException => ("Not found", 404,
                _env.IsDevelopment() ? exception.Message : null),

            // Database conflicts → 409
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => ("Resource conflict", 409,
                _env.IsDevelopment() ? exception.Message : null),

            // MySql DateTime 转换异常：数据库中存在无效日期值，返回 500 并提示管理员检查
            Exception ex when ex.Message.Contains("MySqlDateTime", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("IsValidDateTime", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("Cannot convert", StringComparison.OrdinalIgnoreCase)
                => ("Database datetime error", 500,
                _env.IsDevelopment() ? "数据库中存在无效的 DateTime 值，请检查 datetime 类型字段是否有异常数据" : null),

            // Database errors → 500
            Microsoft.EntityFrameworkCore.DbUpdateException => ("Database error", 500,
                _env.IsDevelopment() ? exception.Message : null),

            // Default → 500
            _ => ("Internal server error", 500,
                _env.IsDevelopment() ? exception.Message : null),
        };

        context.Response.StatusCode = result.statusCode;
        context.Response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Status = result.statusCode,
            Title = result.title,
            Detail = result.detail,
            Instance = context.Request.Path,
        };

        // Include validation errors if present
        if (exception is ArgumentException argEx && !string.IsNullOrEmpty(argEx.ParamName))
        {
            problemDetails.Extensions["invalidParams"] = new Dictionary<string, string>
            {
                { argEx.ParamName, argEx.Message }
            };
        }

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
